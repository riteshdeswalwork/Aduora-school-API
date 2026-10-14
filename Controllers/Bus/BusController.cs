using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.DTOs;
using sms.api.Interfaces;
using System.Security.Claims;

namespace sms.api.Controllers.Bus
{
    [Authorize]
    public class BusController : ApiControllerBase
    {
        private readonly IBusManager _busManager;

        public BusController(IBusManager busManager)
        {
            _busManager = busManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBuses()
        {
            try
            {
                var buses = await _busManager.GetAllBusesAsync();
                return Ok(buses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving buses", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBusById(Guid id)
        {
            try
            {
                var bus = await _busManager.GetBusByIdAsync(id);
                if (bus == null)
                    return NotFound(new { message = "Bus not found" });

                return Ok(bus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving bus", error = ex.Message });
            }
        }

        [HttpGet("by-number/{busNumber}")]
        public async Task<IActionResult> GetBusByNumber(string busNumber)
        {
            try
            {
                var bus = await _busManager.GetBusByNumberAsync(busNumber);
                if (bus == null)
                    return NotFound(new { message = "Bus not found" });

                return Ok(bus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving bus", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBus([FromBody] CreateBusDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var bus = await _busManager.CreateBusAsync(createDto, userId);
                
                return CreatedAtAction(nameof(GetBusById), new { id = bus.Id }, bus);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating bus", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBus(Guid id, [FromBody] UpdateBusDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var bus = await _busManager.UpdateBusAsync(id, updateDto);
                if (bus == null)
                    return NotFound(new { message = "Bus not found" });

                return Ok(bus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating bus", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBus(Guid id)
        {
            try
            {
                var result = await _busManager.DeleteBusAsync(id);
                if (!result)
                    return BadRequest(new { message = "Cannot delete bus. It may not exist or have active routes." });

                return Ok(new { message = "Bus deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting bus", error = ex.Message });
            }
        }

        [HttpPost("{id}/location")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateBusLocation(Guid id, [FromBody] BusLocationUpdateDTO locationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _busManager.UpdateBusLocationAsync(id, locationDto);
                if (!result)
                    return NotFound(new { message = "Bus not found" });

                return Ok(new { message = "Bus location updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating bus location", error = ex.Message });
            }
        }

        [HttpPost("{id}/availability")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetBusAvailability(Guid id, [FromBody] bool isAvailable)
        {
            try
            {
                var result = await _busManager.SetBusAvailabilityAsync(id, isAvailable);
                if (!result)
                    return NotFound(new { message = "Bus not found" });

                return Ok(new { message = $"Bus availability set to {(isAvailable ? "available" : "unavailable")}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error setting bus availability", error = ex.Message });
            }
        }

        [HttpGet("due-for-maintenance")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetBusesDueForMaintenance()
        {
            try
            {
                var buses = await _busManager.GetBusesDueForMaintenanceAsync();
                return Ok(buses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving buses due for maintenance", error = ex.Message });
            }
        }

        [HttpGet("maintenance/paged")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetMaintenanceRecordsPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Guid? busId = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? maintenanceType = null)
        {
            try
            {
                var result = await _busManager.GetMaintenanceRecordsPagedAsync(pageNumber, pageSize, busId, searchTerm, maintenanceType);
                var totalPages = result.TotalPages;
                var first = result.TotalCount == 0 ? 0 : (pageNumber - 1) * pageSize + 1;
                var last = result.TotalCount == 0 ? 0 : Math.Min(pageNumber * pageSize, result.TotalCount);
                return Ok(new
                {
                    items = result.Items,
                    totalCount = result.TotalCount,
                    pageNumber,
                    pageSize,
                    totalPages,
                    hasPreviousPage = pageNumber > 1,
                    hasNextPage = pageNumber < totalPages,
                    firstItemIndex = first,
                    lastItemIndex = last
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving maintenance records", error = ex.Message });
            }
        }

        [HttpGet("maintenance")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllMaintenanceRecords(
            [FromQuery] Guid? busId = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? maintenanceType = null)
        {
            try
            {
                var records = await _busManager.GetAllMaintenanceRecordsAsync(busId, searchTerm, maintenanceType);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving maintenance records", error = ex.Message });
            }
        }

        [HttpGet("maintenance/record/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetMaintenanceRecordById(Guid id)
        {
            try
            {
                var record = await _busManager.GetMaintenanceRecordByIdAsync(id);
                return record == null ? NotFound(new { message = "Maintenance record not found" }) : Ok(record);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving maintenance record", error = ex.Message });
            }
        }

        [HttpGet("{id}/maintenance")]
        public async Task<IActionResult> GetBusMaintenanceRecords(Guid id)
        {
            try
            {
                var records = await _busManager.GetBusMaintenanceRecordsAsync(id);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving maintenance records", error = ex.Message });
            }
        }

        [HttpPost("maintenance")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateMaintenanceRecord([FromBody] CreateBusMaintenanceDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var maintenance = await _busManager.CreateMaintenanceRecordAsync(createDto, userId);
                
                return Ok(maintenance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating maintenance record", error = ex.Message });
            }
        }

        [HttpPut("maintenance/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateMaintenanceRecord(Guid id, [FromBody] UpdateBusMaintenanceDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var updated = await _busManager.UpdateMaintenanceRecordAsync(id, updateDto, userId);
                if (updated == null) return NotFound(new { message = "Maintenance record not found" });
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating maintenance record", error = ex.Message });
            }
        }

        [HttpDelete("maintenance/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteMaintenanceRecord(Guid id)
        {
            try
            {
                var deleted = await _busManager.DeleteMaintenanceRecordAsync(id);
                if (!deleted) return NotFound(new { message = "Maintenance record not found" });
                return Ok(new { message = "Deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting maintenance record", error = ex.Message });
            }
        }

        [HttpGet("{id}/expenses")]
        public async Task<IActionResult> GetBusExpenses(Guid id)
        {
            try
            {
                var expenses = await _busManager.GetBusExpensesAsync(id);
                return Ok(expenses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving bus expenses", error = ex.Message });
            }
        }

        [HttpPost("expenses")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBusExpense([FromBody] CreateBusExpenseDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var expense = await _busManager.CreateBusExpenseAsync(createDto, userId);
                
                return Ok(expense);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating bus expense", error = ex.Message });
            }
        }

        [HttpGet("{id}/monthly-expenses")]
        public async Task<IActionResult> GetBusMonthlyExpenses(Guid id, [FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var totalExpenses = await _busManager.GetBusMonthlyExpensesAsync(id, year, month);
                return Ok(new { totalExpenses, year, month });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving monthly expenses", error = ex.Message });
            }
        }

        [HttpGet("validate-bus-number/{busNumber}")]
        public async Task<IActionResult> ValidateBusNumber(string busNumber, [FromQuery] Guid? excludeId = null)
        {
            try
            {
                var isUnique = await _busManager.IsBusNumberUniqueAsync(busNumber, excludeId);
                return Ok(new { isUnique });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error validating bus number", error = ex.Message });
            }
        }

        [HttpGet("validate-registration/{registrationNumber}")]
        public async Task<IActionResult> ValidateRegistrationNumber(string registrationNumber, [FromQuery] Guid? excludeId = null)
        {
            try
            {
                var isUnique = await _busManager.IsRegistrationNumberUniqueAsync(registrationNumber, excludeId);
                return Ok(new { isUnique });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error validating registration number", error = ex.Message });
            }
        }
    }
}
