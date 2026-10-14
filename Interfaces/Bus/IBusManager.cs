using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Bus
{
    public interface IBusManager : IGenericManager<Models.Bus>
    {
        Task<IEnumerable<BusDTO>> GetAllBusesAsync();
        Task<BusDTO?> GetBusByIdAsync(Guid id);
        Task<BusDTO?> GetBusByNumberAsync(string busNumber);
        Task<BusDTO> CreateBusAsync(CreateBusDTO createDto, Guid createdByUserId);
        Task<BusDTO?> UpdateBusAsync(Guid id, UpdateBusDTO updateDto);
        Task<bool> DeleteBusAsync(Guid id);
        Task<bool> UpdateBusLocationAsync(Guid busId, BusLocationUpdateDTO locationDto);
        Task<bool> SetBusAvailabilityAsync(Guid busId, bool isAvailable);
        Task<IEnumerable<BusMaintenanceDTO>> GetBusMaintenanceRecordsAsync(Guid busId);
        Task<IEnumerable<BusMaintenanceDTO>> GetAllMaintenanceRecordsAsync(Guid? busId = null, string? searchTerm = null, string? maintenanceType = null);
        Task<PagedResult<BusMaintenanceDTO>> GetMaintenanceRecordsPagedAsync(int pageNumber, int pageSize, Guid? busId, string? searchTerm, string? maintenanceType);
        Task<BusMaintenanceDTO?> GetMaintenanceRecordByIdAsync(Guid id);
        Task<BusMaintenanceDTO> CreateMaintenanceRecordAsync(CreateBusMaintenanceDTO createDto, Guid createdByUserId);
        Task<BusMaintenanceDTO?> UpdateMaintenanceRecordAsync(Guid id, UpdateBusMaintenanceDTO updateDto, Guid updatedByUserId);
        Task<bool> DeleteMaintenanceRecordAsync(Guid id);
        Task<IEnumerable<BusDTO>> GetBusesDueForMaintenanceAsync();
        Task<IEnumerable<BusExpenseDTO>> GetBusExpensesAsync(Guid busId);
        Task<BusExpenseDTO> CreateBusExpenseAsync(CreateBusExpenseDTO createDto, Guid createdByUserId);
        Task<decimal> GetBusMonthlyExpensesAsync(Guid busId, int year, int month);
        Task<bool> IsBusNumberUniqueAsync(string busNumber, Guid? excludeId = null);
        Task<bool> IsRegistrationNumberUniqueAsync(string registrationNumber, Guid? excludeId = null);
        Task<bool> CanDeleteBusAsync(Guid busId);
    }

    public interface IBusRouteManager : IGenericManager<BusRoute>
    {
        Task<IEnumerable<BusRouteDTO>> GetAllRoutesAsync();
        Task<BusRouteDTO?> GetRouteByIdAsync(Guid id);
        Task<BusRouteDTO?> GetRouteByCodeAsync(string routeCode);
        Task<BusRouteDTO> CreateRouteAsync(CreateBusRouteDTO createDto, Guid createdByUserId);
        Task<BusRouteDTO?> UpdateRouteAsync(Guid id, UpdateBusRouteDTO updateDto);
        Task<bool> DeleteRouteAsync(Guid id);
        Task<IEnumerable<BusRouteDTO>> GetRoutesByBusIdAsync(Guid busId);
        Task<IEnumerable<BusRouteDTO>> GetOptimizedRoutesAsync();
        Task<decimal> CalculateRouteProfitabilityAsync(Guid routeId);
    }

    public interface IBusStopManager : IGenericManager<BusStop>
    {
        Task<IEnumerable<BusStopDTO>> GetAllStopsAsync();
        Task<BusStopDTO?> GetStopByIdAsync(Guid id);
        Task<BusStopDTO?> GetStopByCodeAsync(string stopCode);
        Task<IEnumerable<BusStopDTO>> GetStopsByRouteIdAsync(Guid routeId);
        Task<BusStopDTO> CreateStopAsync(CreateBusStopDTO createDto, Guid createdByUserId);
        Task<BusStopDTO?> UpdateStopAsync(Guid id, UpdateBusStopDTO updateDto);
        Task<bool> DeleteStopAsync(Guid id);
        Task<bool> ReorderStopsAsync(Guid routeId, List<Guid> stopIds);
        Task<IEnumerable<BusStopDTO>> GetNearbyStopsAsync(decimal latitude, decimal longitude, double radiusKm);
        Task<bool> IsStopCodeUniqueAsync(string stopCode, Guid? excludeId = null);
        Task<bool> CanDeleteStopAsync(Guid stopId);
    }
}
