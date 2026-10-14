using Dapper;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Bus;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Services.Data;
using BusEntity = sms.api.Models.Bus;

namespace sms.api.Managers.Bus
{
    public class BusManager : GenericManager<BusEntity>, IBusManager
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public BusManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<PagedResult<BusMaintenanceDTO>> GetMaintenanceRecordsPagedAsync(
            int pageNumber, int pageSize, Guid? busId, string? searchTerm, string? maintenanceType)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@BusId", busId);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@MaintenanceType", string.IsNullOrWhiteSpace(maintenanceType) ? null : maintenanceType.Trim());

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<BusMaintenanceDTO>(
                "dbo.sp_BusMaintenanceRecords_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<BusMaintenanceDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }

        public async Task<IEnumerable<BusDTO>> GetAllBusesAsync()
        {
            var buses = await _dbSet
                .Include(b => b.CreatedBy)
                .Include(b => b.BusRoutes)
                .OrderBy(b => b.BusNumber)
                .ToListAsync();

            return buses.Select(MapToDTO);
        }

        public async Task<BusDTO?> GetBusByIdAsync(Guid id)
        {
            var bus = await _dbSet
                .Include(b => b.CreatedBy)
                .Include(b => b.BusRoutes)
                    .ThenInclude(br => br.BusStops)
                .Include(b => b.MaintenanceRecords)
                .FirstOrDefaultAsync(b => b.Id == id);

            return bus != null ? MapToDTO(bus) : null;
        }

        public async Task<BusDTO?> GetBusByNumberAsync(string busNumber)
        {
            var bus = await _dbSet
                .Include(b => b.CreatedBy)
                .Include(b => b.BusRoutes)
                .FirstOrDefaultAsync(b => b.BusNumber == busNumber);

            return bus != null ? MapToDTO(bus) : null;
        }

        public async Task<BusDTO> CreateBusAsync(CreateBusDTO createDto, Guid createdByUserId)
        {
            // Validate unique bus number
            if (!await IsBusNumberUniqueAsync(createDto.BusNumber))
            {
                throw new InvalidOperationException($"Bus number '{createDto.BusNumber}' already exists.");
            }

            // Validate unique registration number
            if (!await IsRegistrationNumberUniqueAsync(createDto.RegistrationNumber))
            {
                throw new InvalidOperationException($"Registration number '{createDto.RegistrationNumber}' already exists.");
            }

            var bus = new BusEntity
            {
                Id = Guid.NewGuid(),
                BusNumber = createDto.BusNumber,
                BusName = createDto.BusName,
                RegistrationNumber = createDto.RegistrationNumber,
                Capacity = createDto.Capacity,
                DriverName = createDto.DriverName,
                DriverPhone = createDto.DriverPhone,
                ConductorName = createDto.ConductorName,
                ConductorPhone = createDto.ConductorPhone,
                Make = createDto.Make,
                Model = createDto.Model,
                Year = createDto.Year,
                FuelType = createDto.FuelType,
                FuelCapacity = createDto.FuelCapacity,
                Mileage = createDto.Mileage,
                Description = createDto.Description,
                InsuranceExpiryDate = createDto.InsuranceExpiryDate,
                FitnessExpiryDate = createDto.FitnessExpiryDate,
                IsActive = true,
                IsAvailable = true,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _dbSet.Add(bus);
            await _context.SaveChangesAsync();

            return await GetBusByIdAsync(bus.Id) ?? throw new InvalidOperationException("Failed to retrieve created bus.");
        }

        public async Task<BusDTO?> UpdateBusAsync(Guid id, UpdateBusDTO updateDto)
        {
            var bus = await _dbSet.FindAsync(id);
            if (bus == null) return null;

            // Update properties if provided
            if (!string.IsNullOrEmpty(updateDto.BusName))
                bus.BusName = updateDto.BusName;

            if (updateDto.Capacity.HasValue)
                bus.Capacity = updateDto.Capacity.Value;

            if (updateDto.DriverName != null)
                bus.DriverName = updateDto.DriverName;

            if (updateDto.DriverPhone != null)
                bus.DriverPhone = updateDto.DriverPhone;

            if (updateDto.ConductorName != null)
                bus.ConductorName = updateDto.ConductorName;

            if (updateDto.ConductorPhone != null)
                bus.ConductorPhone = updateDto.ConductorPhone;

            if (updateDto.Make != null)
                bus.Make = updateDto.Make;

            if (updateDto.Model != null)
                bus.Model = updateDto.Model;

            if (updateDto.Year.HasValue)
                bus.Year = updateDto.Year.Value;

            if (updateDto.FuelType != null)
                bus.FuelType = updateDto.FuelType;

            if (updateDto.FuelCapacity.HasValue)
                bus.FuelCapacity = updateDto.FuelCapacity.Value;

            if (updateDto.Mileage.HasValue)
                bus.Mileage = updateDto.Mileage.Value;

            if (updateDto.Description != null)
                bus.Description = updateDto.Description;

            if (updateDto.IsActive.HasValue)
                bus.IsActive = updateDto.IsActive.Value;

            if (updateDto.IsAvailable.HasValue)
                bus.IsAvailable = updateDto.IsAvailable.Value;

            if (updateDto.InsuranceExpiryDate.HasValue)
                bus.InsuranceExpiryDate = updateDto.InsuranceExpiryDate.Value;

            if (updateDto.FitnessExpiryDate.HasValue)
                bus.FitnessExpiryDate = updateDto.FitnessExpiryDate.Value;

            bus.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetBusByIdAsync(id);
        }

        public async Task<bool> DeleteBusAsync(Guid id)
        {
            if (!await CanDeleteBusAsync(id))
                return false;

            var bus = await _dbSet.FindAsync(id);
            if (bus == null) return false;

            _dbSet.Remove(bus);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBusLocationAsync(Guid busId, BusLocationUpdateDTO locationDto)
        {
            var bus = await _dbSet.FindAsync(busId);
            if (bus == null) return false;

            bus.CurrentLatitude = locationDto.Latitude;
            bus.CurrentLongitude = locationDto.Longitude;
            bus.LastLocationUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetBusAvailabilityAsync(Guid busId, bool isAvailable)
        {
            var bus = await _dbSet.FindAsync(busId);
            if (bus == null) return false;

            bus.IsAvailable = isAvailable;
            bus.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BusMaintenanceDTO>> GetBusMaintenanceRecordsAsync(Guid busId)
        {
            var records = await _context.BusMaintenanceRecords
                .Include(bmr => bmr.Bus)
                .Include(bmr => bmr.CreatedBy)
                .Where(bmr => bmr.BusId == busId)
                .OrderByDescending(bmr => bmr.MaintenanceDate)
                .ToListAsync();

            return records.Select(MapMaintenanceToDTO);
        }

        public async Task<IEnumerable<BusMaintenanceDTO>> GetAllMaintenanceRecordsAsync(Guid? busId = null, string? searchTerm = null, string? maintenanceType = null)
        {
            var query = _context.BusMaintenanceRecords
                .Include(bmr => bmr.Bus)
                .Include(bmr => bmr.CreatedBy)
                .AsQueryable();

            if (busId.HasValue) query = query.Where(bmr => bmr.BusId == busId.Value);
            if (!string.IsNullOrWhiteSpace(maintenanceType)) query = query.Where(bmr => bmr.MaintenanceType == maintenanceType);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(bmr =>
                    (bmr.Bus != null && bmr.Bus.BusNumber.ToLower().Contains(term)) ||
                    (bmr.MaintenanceType != null && bmr.MaintenanceType.ToLower().Contains(term)) ||
                    (bmr.Description != null && bmr.Description.ToLower().Contains(term)) ||
                    (bmr.ServiceProvider != null && bmr.ServiceProvider.ToLower().Contains(term)) ||
                    (bmr.PartsReplaced != null && bmr.PartsReplaced.ToLower().Contains(term)) ||
                    (bmr.Remarks != null && bmr.Remarks.ToLower().Contains(term)));
            }

            var records = await query.OrderByDescending(bmr => bmr.MaintenanceDate).Take(500).ToListAsync();
            return records.Select(MapMaintenanceToDTO);
        }

        public async Task<BusMaintenanceDTO?> GetMaintenanceRecordByIdAsync(Guid id)
        {
            var record = await _context.BusMaintenanceRecords
                .Include(bmr => bmr.Bus)
                .Include(bmr => bmr.CreatedBy)
                .FirstOrDefaultAsync(bmr => bmr.Id == id);
            return record == null ? null : MapMaintenanceToDTO(record);
        }

        public async Task<BusMaintenanceDTO> CreateMaintenanceRecordAsync(CreateBusMaintenanceDTO createDto, Guid createdByUserId)
        {
            var maintenance = new BusMaintenanceRecord
            {
                Id = Guid.NewGuid(),
                BusId = createDto.BusId,
                MaintenanceType = createDto.MaintenanceType,
                MaintenanceDate = createDto.MaintenanceDate,
                Description = createDto.Description,
                Cost = createDto.Cost,
                ServiceProvider = createDto.ServiceProvider,
                ServiceProviderPhone = createDto.ServiceProviderPhone,
                Odometer = createDto.Odometer,
                NextServiceDate = createDto.NextServiceDate,
                PartsReplaced = createDto.PartsReplaced,
                Remarks = createDto.Remarks,
                IsCompleted = true,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.BusMaintenanceRecords.Add(maintenance);

            // Update bus maintenance dates
            var bus = await _dbSet.FindAsync(createDto.BusId);
            if (bus != null)
            {
                bus.LastMaintenanceDate = createDto.MaintenanceDate;
                if (createDto.NextServiceDate.HasValue)
                    bus.NextMaintenanceDate = createDto.NextServiceDate.Value;
            }

            await _context.SaveChangesAsync();

            var record = await _context.BusMaintenanceRecords
                .Include(bmr => bmr.Bus)
                .Include(bmr => bmr.CreatedBy)
                .FirstAsync(bmr => bmr.Id == maintenance.Id);

            return MapMaintenanceToDTO(record);
        }

        public async Task<BusMaintenanceDTO?> UpdateMaintenanceRecordAsync(Guid id, UpdateBusMaintenanceDTO updateDto, Guid updatedByUserId)
        {
            var maintenance = await _context.BusMaintenanceRecords.FindAsync(id);
            if (maintenance == null) return null;

            maintenance.BusId = updateDto.BusId;
            maintenance.MaintenanceType = updateDto.MaintenanceType;
            maintenance.MaintenanceDate = updateDto.MaintenanceDate;
            maintenance.Description = updateDto.Description;
            maintenance.Cost = updateDto.Cost;
            maintenance.ServiceProvider = updateDto.ServiceProvider;
            maintenance.ServiceProviderPhone = updateDto.ServiceProviderPhone;
            maintenance.Odometer = updateDto.Odometer;
            maintenance.NextServiceDate = updateDto.NextServiceDate;
            maintenance.PartsReplaced = updateDto.PartsReplaced;
            maintenance.Remarks = updateDto.Remarks;
            maintenance.IsCompleted = updateDto.IsCompleted;
            maintenance.UpdatedByUserId = updatedByUserId;
            maintenance.UpdatedAt = DateTime.UtcNow;

            var bus = await _dbSet.FindAsync(updateDto.BusId);
            if (bus != null)
            {
                bus.LastMaintenanceDate = updateDto.MaintenanceDate;
                if (updateDto.NextServiceDate.HasValue)
                    bus.NextMaintenanceDate = updateDto.NextServiceDate.Value;
            }

            await _context.SaveChangesAsync();

            var record = await _context.BusMaintenanceRecords
                .Include(bmr => bmr.Bus)
                .Include(bmr => bmr.CreatedBy)
                .FirstAsync(bmr => bmr.Id == id);
            return MapMaintenanceToDTO(record);
        }

        public async Task<bool> DeleteMaintenanceRecordAsync(Guid id)
        {
            var maintenance = await _context.BusMaintenanceRecords.FindAsync(id);
            if (maintenance == null) return false;
            _context.BusMaintenanceRecords.Remove(maintenance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BusDTO>> GetBusesDueForMaintenanceAsync()
        {
            var buses = await _dbSet
                .Include(b => b.CreatedBy)
                .Where(b => b.IsActive &&
                           (b.NextMaintenanceDate == null || b.NextMaintenanceDate <= DateTime.Today.AddDays(7)))
                .OrderBy(b => b.NextMaintenanceDate)
                .ToListAsync();

            return buses.Select(MapToDTO);
        }

        public async Task<IEnumerable<BusExpenseDTO>> GetBusExpensesAsync(Guid busId)
        {
            var expenses = await _context.BusExpenses
                .Include(be => be.Bus)
                .Include(be => be.CreatedBy)
                .Where(be => be.BusId == busId)
                .OrderByDescending(be => be.ExpenseDate)
                .ToListAsync();

            return expenses.Select(MapExpenseToDTO);
        }

        public async Task<BusExpenseDTO> CreateBusExpenseAsync(CreateBusExpenseDTO createDto, Guid createdByUserId)
        {
            var expense = new BusExpense
            {
                Id = Guid.NewGuid(),
                BusId = createDto.BusId,
                ExpenseType = createDto.ExpenseType,
                ExpenseDate = createDto.ExpenseDate,
                Amount = createDto.Amount,
                Description = createDto.Description,
                VendorName = createDto.VendorName,
                BillNumber = createDto.BillNumber,
                Remarks = createDto.Remarks,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.BusExpenses.Add(expense);
            await _context.SaveChangesAsync();

            var record = await _context.BusExpenses
                .Include(be => be.Bus)
                .Include(be => be.CreatedBy)
                .FirstAsync(be => be.Id == expense.Id);

            return MapExpenseToDTO(record);
        }

        public async Task<decimal> GetBusMonthlyExpensesAsync(Guid busId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await _context.BusExpenses
                .Where(be => be.BusId == busId &&
                            be.ExpenseDate >= startDate &&
                            be.ExpenseDate <= endDate)
                .SumAsync(be => be.Amount);
        }

        public async Task<bool> IsBusNumberUniqueAsync(string busNumber, Guid? excludeId = null)
        {
            return !await _dbSet.AnyAsync(b => b.BusNumber == busNumber &&
                                              (excludeId == null || b.Id != excludeId));
        }

        public async Task<bool> IsRegistrationNumberUniqueAsync(string registrationNumber, Guid? excludeId = null)
        {
            return !await _dbSet.AnyAsync(b => b.RegistrationNumber == registrationNumber &&
                                              (excludeId == null || b.Id != excludeId));
        }

        public async Task<bool> CanDeleteBusAsync(Guid busId)
        {
            // Cannot delete if bus has active routes
            var hasActiveRoutes = await _context.BusRoutes
                .AnyAsync(br => br.BusId == busId && br.IsActive);

            return !hasActiveRoutes;
        }

        private BusDTO MapToDTO(BusEntity bus)
        {
            return new BusDTO
            {
                Id = bus.Id,
                BusNumber = bus.BusNumber,
                BusName = bus.BusName,
                RegistrationNumber = bus.RegistrationNumber,
                Capacity = bus.Capacity,
                DriverName = bus.DriverName,
                DriverPhone = bus.DriverPhone,
                ConductorName = bus.ConductorName,
                ConductorPhone = bus.ConductorPhone,
                Make = bus.Make,
                Model = bus.Model,
                Year = bus.Year,
                FuelType = bus.FuelType,
                FuelCapacity = bus.FuelCapacity,
                Mileage = bus.Mileage,
                Description = bus.Description,
                IsActive = bus.IsActive,
                IsAvailable = bus.IsAvailable,
                CurrentLatitude = bus.CurrentLatitude,
                CurrentLongitude = bus.CurrentLongitude,
                LastLocationUpdate = bus.LastLocationUpdate,
                LastMaintenanceDate = bus.LastMaintenanceDate,
                NextMaintenanceDate = bus.NextMaintenanceDate,
                InsuranceExpiryDate = bus.InsuranceExpiryDate,
                FitnessExpiryDate = bus.FitnessExpiryDate,
                CreatedByName = bus.CreatedBy?.FirstName + " " + bus.CreatedBy?.LastName,
                CreatedAt = bus.CreatedAt,
                UpdatedAt = bus.UpdatedAt,
                Routes = bus.BusRoutes?.Select(MapRouteToDTO).ToList() ?? new List<BusRouteDTO>()
            };
        }

        private BusRouteDTO MapRouteToDTO(BusRoute route)
        {
            return new BusRouteDTO
            {
                Id = route.Id,
                RouteName = route.RouteName,
                RouteCode = route.RouteCode,
                BusId = route.BusId ?? Guid.Empty,
                Description = route.Description,
                TotalDistance = route.TotalDistance,
                EstimatedDuration = route.EstimatedDuration,
                MorningStartTime = route.MorningStartTime,
                MorningEndTime = route.MorningEndTime,
                EveningStartTime = route.EveningStartTime,
                EveningEndTime = route.EveningEndTime,
                IsActive = route.IsActive,
                CreatedAt = route.CreatedAt,
                UpdatedAt = route.UpdatedAt
            };
        }

        private BusMaintenanceDTO MapMaintenanceToDTO(BusMaintenanceRecord maintenance)
        {
            return new BusMaintenanceDTO
            {
                Id = maintenance.Id,
                BusId = maintenance.BusId,
                BusNumber = maintenance.Bus?.BusNumber ?? "",
                MaintenanceType = maintenance.MaintenanceType,
                MaintenanceDate = maintenance.MaintenanceDate,
                Description = maintenance.Description,
                Cost = maintenance.Cost,
                ServiceProvider = maintenance.ServiceProvider,
                ServiceProviderPhone = maintenance.ServiceProviderPhone,
                Odometer = maintenance.Odometer,
                NextServiceDate = maintenance.NextServiceDate,
                PartsReplaced = maintenance.PartsReplaced,
                Remarks = maintenance.Remarks,
                IsCompleted = maintenance.IsCompleted,
                CreatedByName = maintenance.CreatedBy?.FirstName + " " + maintenance.CreatedBy?.LastName,
                CreatedAt = maintenance.CreatedAt
            };
        }

        private BusExpenseDTO MapExpenseToDTO(BusExpense expense)
        {
            return new BusExpenseDTO
            {
                Id = expense.Id,
                BusId = expense.BusId,
                BusNumber = expense.Bus?.BusNumber ?? "",
                ExpenseType = expense.ExpenseType,
                ExpenseDate = expense.ExpenseDate,
                Amount = expense.Amount,
                Description = expense.Description,
                VendorName = expense.VendorName,
                BillNumber = expense.BillNumber,
                Remarks = expense.Remarks,
                CreatedByName = expense.CreatedBy?.FirstName + " " + expense.CreatedBy?.LastName,
                CreatedAt = expense.CreatedAt
            };
        }
    }
}
