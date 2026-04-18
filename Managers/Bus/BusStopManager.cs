using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Bus;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Bus
{
    public class BusStopManager : GenericManager<BusStop>, IBusStopManager
    {
        public BusStopManager(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BusStopDTO>> GetAllStopsAsync()
        {
            var stops = await _dbSet
                .Include(s => s.Route)
                    .ThenInclude(r => r!.Bus)
                .Include(s => s.CreatedBy)
                .OrderBy(s => s.Route != null ? s.Route.RouteName : "")
                .ThenBy(s => s.StopOrder)
                .ToListAsync();

            return stops.Select(MapToDTO);
        }

        public async Task<BusStopDTO?> GetStopByIdAsync(Guid id)
        {
            var stop = await _dbSet
                .Include(s => s.Route)
                    .ThenInclude(r => r.Bus)
                .Include(s => s.CreatedBy)
                .FirstOrDefaultAsync(s => s.Id == id);

            return stop != null ? MapToDTO(stop) : null;
        }

        public async Task<BusStopDTO?> GetStopByCodeAsync(string stopCode)
        {
            var stop = await _dbSet
                .Include(s => s.Route)
                    .ThenInclude(r => r.Bus)
                .Include(s => s.CreatedBy)
                .FirstOrDefaultAsync(s => s.StopCode == stopCode);

            return stop != null ? MapToDTO(stop) : null;
        }

        public async Task<IEnumerable<BusStopDTO>> GetStopsByRouteIdAsync(Guid routeId)
        {
            var stops = await _dbSet
                .Include(s => s.Route)
                    .ThenInclude(r => r.Bus)
                .Include(s => s.CreatedBy)
                .Where(s => s.RouteId == routeId)
                .OrderBy(s => s.StopOrder)
                .ToListAsync();

            return stops.Select(MapToDTO);
        }

        public async Task<BusStopDTO> CreateStopAsync(CreateBusStopDTO createDto, Guid createdByUserId)
        {
            // Validate unique stop code
            if (!await IsStopCodeUniqueAsync(createDto.StopCode))
            {
                throw new InvalidOperationException($"Stop code '{createDto.StopCode}' already exists.");
            }

            Guid? routeId = createDto.RouteId;
            int stopOrder = Math.Clamp(createDto.StopOrder, 1, 100);

            if (routeId.HasValue)
            {
                var route = await _context.BusRoutes.FindAsync(routeId.Value);
                if (route == null)
                    throw new InvalidOperationException("Selected route does not exist.");
                if (!route.IsActive)
                    throw new InvalidOperationException("Selected route is not active.");
                var maxOrder = await _dbSet
                    .Where(s => s.RouteId == routeId.Value)
                    .MaxAsync(s => (int?)s.StopOrder) ?? 0;
                stopOrder = maxOrder + 1;
            }

            var stop = new BusStop
            {
                Id = Guid.NewGuid(),
                StopName = createDto.StopName,
                StopCode = createDto.StopCode,
                RouteId = routeId,
                StopOrder = stopOrder,
                Latitude = createDto.Latitude?.Trim() ?? "",
                Longitude = createDto.Longitude?.Trim() ?? "",
                Address = createDto.Address,
                Landmark = createDto.Landmark,
                DistanceFromSchool = createDto.DistanceFromSchool,
                MorningPickupTime = createDto.MorningPickupTime ?? TimeSpan.FromHours(7.5),
                EveningDropTime = createDto.EveningDropTime ?? TimeSpan.FromHours(16.5),
                MonthlyFee = createDto.MonthlyFee,
                Description = createDto.Description,
                IsActive = true,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _dbSet.Add(stop);
            await _context.SaveChangesAsync();

            var createdStop = await _dbSet
                .Include(s => s.Route)
                    .ThenInclude(r => r.Bus)
                .Include(s => s.CreatedBy)
                .FirstAsync(s => s.Id == stop.Id);

            return MapToDTO(createdStop);
        }

        public async Task<BusStopDTO?> UpdateStopAsync(Guid id, UpdateBusStopDTO updateDto)
        {
            var stop = await _dbSet.FindAsync(id);
            if (stop == null)
                return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateDto.StopName))
                stop.StopName = updateDto.StopName;

            if (updateDto.RouteId.HasValue)
            {
                var route = await _context.BusRoutes.FindAsync(updateDto.RouteId.Value);
                if (route == null || !route.IsActive)
                    throw new InvalidOperationException("Selected route does not exist or is not active.");
               
                stop.RouteId = updateDto.RouteId.Value;
            }

            if (!string.IsNullOrEmpty(updateDto.Latitude))
                stop.Latitude = updateDto.Latitude.Trim();

            if (!string.IsNullOrEmpty(updateDto.Longitude))
                stop.Longitude = updateDto.Longitude.Trim();

            if (!string.IsNullOrEmpty(updateDto.Address))
                stop.Address = updateDto.Address;

            if (!string.IsNullOrEmpty(updateDto.Landmark))
                stop.Landmark = updateDto.Landmark;

            if (updateDto.DistanceFromSchool.HasValue)
                stop.DistanceFromSchool = updateDto.DistanceFromSchool.Value;

            if (updateDto.MorningPickupTime.HasValue)
                stop.MorningPickupTime = updateDto.MorningPickupTime.Value;

            if (updateDto.EveningDropTime.HasValue)
                stop.EveningDropTime = updateDto.EveningDropTime.Value;

            if (updateDto.MonthlyFee.HasValue)
                stop.MonthlyFee = updateDto.MonthlyFee.Value;

            if (!string.IsNullOrEmpty(updateDto.Description))
                stop.Description = updateDto.Description;

            if (updateDto.IsActive.HasValue)
                stop.IsActive = updateDto.IsActive.Value;

            stop.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedStop = await _dbSet
                .Include(s => s.Route)
                    .ThenInclude(r => r.Bus)
                .Include(s => s.CreatedBy)
                .FirstAsync(s => s.Id == id);

            return MapToDTO(updatedStop);
        }

        public async Task<bool> DeleteStopAsync(Guid id)
        {
            var stop = await _dbSet.FindAsync(id);
            if (stop == null)
                return false;

            // Check if stop can be deleted
            if (!await CanDeleteStopAsync(id))
            {
                throw new InvalidOperationException("Cannot delete bus stop with active student assignments.");
            }

            _dbSet.Remove(stop);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderStopsAsync(Guid routeId, List<Guid> stopIds)
        {
            var stops = await _dbSet
                .Where(s => s.RouteId == routeId && stopIds.Contains(s.Id))
                .ToListAsync();

            if (stops.Count != stopIds.Count)
                return false;

            for (int i = 0; i < stopIds.Count; i++)
            {
                var stop = stops.First(s => s.Id == stopIds[i]);
                stop.StopOrder = i + 1;
                stop.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BusStopDTO>> GetNearbyStopsAsync(decimal latitude, decimal longitude, double radiusKm)
        {
            // Simple distance calculation (can be enhanced with proper geospatial queries)
            var stops = await _dbSet
                .Include(s => s.Route)
                    .ThenInclude(r => r.Bus)
                .Include(s => s.CreatedBy)
                .Where(s => s.IsActive)
                .ToListAsync();

            var nearbyStops = stops.Where(s =>
            {
                if (!TryParseCoord(s.Latitude, s.Longitude, out var lat, out var lon)) return false;
                var distance = CalculateDistance(latitude, longitude, (decimal)lat, (decimal)lon);
                return distance <= radiusKm;
            }).OrderBy(s =>
            {
                TryParseCoord(s.Latitude, s.Longitude, out var lat, out var lon);
                return CalculateDistance(latitude, longitude, (decimal)lat, (decimal)lon);
            });

            return nearbyStops.Select(MapToDTO);
        }

        public async Task<bool> IsStopCodeUniqueAsync(string stopCode, Guid? excludeId = null)
        {
            return !await _dbSet.AnyAsync(s => s.StopCode == stopCode &&
                                              (excludeId == null || s.Id != excludeId));
        }

        public async Task<bool> CanDeleteStopAsync(Guid stopId)
        {
            // Student bus assignments removed - always allow deletion
            return true;
        }

        private double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            // Haversine formula for calculating distance between two points
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRadians((double)(lat2 - lat1));
            var dLon = ToRadians((double)(lon2 - lon1));

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private static bool TryParseCoord(string? latStr, string? lngStr, out double lat, out double lon)
        {
            lat = 0; lon = 0;
            return !string.IsNullOrWhiteSpace(latStr) && double.TryParse(latStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out lat)
                   && !string.IsNullOrWhiteSpace(lngStr) && double.TryParse(lngStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out lon);
        }

        private BusStopDTO MapToDTO(BusStop stop)
        {
            return new BusStopDTO
            {
                Id = stop.Id,
                StopName = stop.StopName,
                StopCode = stop.StopCode,
                RouteId = stop.RouteId,
                RouteName = stop.Route?.RouteName ?? "",
                StopOrder = stop.StopOrder,
                Latitude = stop.Latitude,
                Longitude = stop.Longitude,
                Address = stop.Address,
                Landmark = stop.Landmark,
                DistanceFromSchool = stop.DistanceFromSchool,
                MorningPickupTime = stop.MorningPickupTime,
                EveningDropTime = stop.EveningDropTime,
                MonthlyFee = stop.MonthlyFee,
                IsActive = stop.IsActive,
                Description = stop.Description,
                CreatedByName = stop.CreatedBy?.FirstName + " " + stop.CreatedBy?.LastName ?? "",
                CreatedAt = stop.CreatedAt,
                UpdatedAt = stop.UpdatedAt,
                TotalStudents = 0 // TODO: Calculate from student assignments
            };
        }
    }
}
