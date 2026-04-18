using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Bus;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Bus
{
    public class BusRouteManager : GenericManager<BusRoute>, IBusRouteManager
    {
        public BusRouteManager(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BusRouteDTO>> GetAllRoutesAsync()
        {
            var routes = await _dbSet
                .Include(r => r.Bus)
                .Include(r => r.CreatedBy)
                .Include(r => r.BusStops)
                .OrderBy(r => r.RouteName)
                .ToListAsync();

            return routes.Select(MapToDTO);
        }

        public async Task<BusRouteDTO?> GetRouteByIdAsync(Guid id)
        {
            var route = await _dbSet
                .Include(r => r.Bus)
                .Include(r => r.CreatedBy)
                .Include(r => r.BusStops)
                .FirstOrDefaultAsync(r => r.Id == id);

            return route != null ? MapToDTO(route) : null;
        }

        public async Task<BusRouteDTO?> GetRouteByCodeAsync(string routeCode)
        {
            var route = await _dbSet
                .Include(r => r.Bus)
                .Include(r => r.CreatedBy)
                .Include(r => r.BusStops)
                .FirstOrDefaultAsync(r => r.RouteCode == routeCode);

            return route != null ? MapToDTO(route) : null;
        }

        public async Task<BusRouteDTO> CreateRouteAsync(CreateBusRouteDTO createDto, Guid createdByUserId)
        {
            // Validate unique route code
            if (!await IsRouteCodeUniqueAsync(createDto.RouteCode))
            {
                throw new InvalidOperationException($"Route code '{createDto.RouteCode}' already exists.");
            }

            // Validate bus exists and is available
            var bus = await _context.Buses.FindAsync(createDto.BusId);
            if (bus == null)
            {
                throw new InvalidOperationException("Selected bus does not exist.");
            }

            if (!bus.IsActive)
            {
                throw new InvalidOperationException("Selected bus is not active.");
            }

            var route = new BusRoute
            {
                Id = Guid.NewGuid(),
                RouteName = createDto.RouteName,
                RouteCode = createDto.RouteCode,
                BusId = createDto.BusId,
                Description = createDto.Description,
                TotalDistance = createDto.TotalDistance,
                EstimatedDuration = createDto.EstimatedDuration,
                MorningStartTime = createDto.MorningStartTime,
                MorningEndTime = createDto.MorningEndTime,
                EveningStartTime = createDto.EveningStartTime,
                EveningEndTime = createDto.EveningEndTime,
                RouteWaypoints = createDto.RouteWaypoints,
                IsActive = true,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _dbSet.Add(route);
            await _context.SaveChangesAsync();

            var createdRoute = await _dbSet
                .Include(r => r.Bus)
                .Include(r => r.CreatedBy)
                .Include(r => r.BusStops)
                .FirstAsync(r => r.Id == route.Id);

            return MapToDTO(createdRoute);
        }

        public async Task<BusRouteDTO?> UpdateRouteAsync(Guid id, UpdateBusRouteDTO updateDto)
        {
            var route = await _dbSet.FindAsync(id);
            if (route == null)
                return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateDto.RouteName))
                route.RouteName = updateDto.RouteName;

            if (updateDto.BusId.HasValue)
            {
                var bus = await _context.Buses.FindAsync(updateDto.BusId.Value);
                if (bus == null || !bus.IsActive)
                    throw new InvalidOperationException("Selected bus does not exist or is not active.");
               
                route.BusId = updateDto.BusId.Value;
            }

            if (!string.IsNullOrEmpty(updateDto.Description))
                route.Description = updateDto.Description;

            if (updateDto.TotalDistance.HasValue)
                route.TotalDistance = updateDto.TotalDistance.Value;

            if (updateDto.EstimatedDuration.HasValue)
                route.EstimatedDuration = updateDto.EstimatedDuration.Value;

            if (updateDto.MorningStartTime.HasValue)
                route.MorningStartTime = updateDto.MorningStartTime.Value;

            if (updateDto.MorningEndTime.HasValue)
                route.MorningEndTime = updateDto.MorningEndTime.Value;

            if (updateDto.EveningStartTime.HasValue)
                route.EveningStartTime = updateDto.EveningStartTime.Value;

            if (updateDto.EveningEndTime.HasValue)
                route.EveningEndTime = updateDto.EveningEndTime.Value;

            if (updateDto.IsActive.HasValue)
                route.IsActive = updateDto.IsActive.Value;

            if (!string.IsNullOrEmpty(updateDto.RouteWaypoints))
                route.RouteWaypoints = updateDto.RouteWaypoints;

            route.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedRoute = await _dbSet
                .Include(r => r.Bus)
                .Include(r => r.CreatedBy)
                .Include(r => r.BusStops)
                .FirstAsync(r => r.Id == id);

            return MapToDTO(updatedRoute);
        }

        public async Task<bool> DeleteRouteAsync(Guid id)
        {
            var route = await _dbSet
                .Include(r => r.BusStops)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null)
                return false;

            // Check if route has active bus stops
            if (route.BusStops.Any(s => s.IsActive))
            {
                throw new InvalidOperationException("Cannot delete route with active bus stops. Please deactivate or remove all bus stops first.");
            }

            _dbSet.Remove(route);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BusRouteDTO>> GetRoutesByBusIdAsync(Guid busId)
        {
            var routes = await _dbSet
                .Include(r => r.Bus)
                .Include(r => r.CreatedBy)
                .Include(r => r.BusStops)
                .Where(r => r.BusId == busId)
                .OrderBy(r => r.RouteName)
                .ToListAsync();

            return routes.Select(MapToDTO);
        }

        public async Task<IEnumerable<BusRouteDTO>> GetOptimizedRoutesAsync()
        {
            // Simple optimization: routes with most students and least distance
            var routes = await _dbSet
                .Include(r => r.Bus)
                .Include(r => r.CreatedBy)
                .Include(r => r.BusStops)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.BusStops.Count) // More stops = potentially more students
                .ThenBy(r => r.TotalDistance) // Less distance = more efficient
                .ToListAsync();

            return routes.Select(MapToDTO);
        }

        public async Task<decimal> CalculateRouteProfitabilityAsync(Guid routeId)
        {
            var route = await _dbSet
                .Include(r => r.BusStops)
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
                return 0;

            // Calculate total monthly revenue from all stops
            var totalRevenue = route.BusStops.Sum(s => s.MonthlyFee);

            // Simple cost calculation (can be enhanced with actual operational costs)
            var estimatedMonthlyCost = route.TotalDistance * 10; // ₹10 per km per month

            return totalRevenue - estimatedMonthlyCost;
        }

        public async Task<bool> IsRouteCodeUniqueAsync(string routeCode, Guid? excludeId = null)
        {
            return !await _dbSet.AnyAsync(r => r.RouteCode == routeCode &&
                                              (excludeId == null || r.Id != excludeId));
        }

        public async Task<bool> CanDeleteRouteAsync(Guid routeId)
        {
            var route = await _dbSet
                .Include(r => r.BusStops)
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
                return false;

            // Cannot delete if route has active bus stops
            return !route.BusStops.Any(s => s.IsActive);
        }

        private BusRouteDTO MapToDTO(BusRoute route)
        {
            return new BusRouteDTO
            {
                Id = route.Id,
                RouteName = route.RouteName,
                RouteCode = route.RouteCode,
                BusId = (Guid)route.BusId,
                BusNumber = route.Bus?.BusNumber ?? "",
                BusName = route.Bus?.BusName ?? "",
                Description = route.Description,
                TotalDistance = route.TotalDistance,
                EstimatedDuration = route.EstimatedDuration,
                MorningStartTime = route.MorningStartTime,
                MorningEndTime = route.MorningEndTime,
                EveningStartTime = route.EveningStartTime,
                EveningEndTime = route.EveningEndTime,
                IsActive = route.IsActive,
                RouteWaypoints = route.RouteWaypoints,
                CreatedByName = route.CreatedBy?.FirstName + " " + route.CreatedBy?.LastName ?? "",
                CreatedAt = route.CreatedAt,
                UpdatedAt = route.UpdatedAt,
                BusStops = route.BusStops?.Select(MapStopToDTO).ToList() ?? new List<BusStopDTO>(),
                TotalStops = route.BusStops?.Count ?? 0,
                TotalStudents = 0, // TODO: Calculate from student assignments
                TotalRevenue = route.BusStops?.Sum(s => s.MonthlyFee) ?? 0
            };
        }

        private BusStopDTO MapStopToDTO(BusStop stop)
        {
            return new BusStopDTO
            {
                Id = stop.Id,
                StopName = stop.StopName,
                StopCode = stop.StopCode,
                RouteId = stop.RouteId,
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
                UpdatedAt = stop.UpdatedAt
            };
        }
    }
}
