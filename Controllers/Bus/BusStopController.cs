using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.DTOs;
using sms.api.Interfaces;
using System.Security.Claims;

namespace sms.api.Controllers.Bus
{
    [Route("api/busstops")]
    public class BusStopController : ApiControllerBase
    {
        private readonly IBusStopManager _busStopManager;

        public BusStopController(IBusStopManager busStopManager)
        {
            _busStopManager = busStopManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllStops()
        {
            try
            {
                var stops = await _busStopManager.GetAllStopsAsync();
                return Ok(stops);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving bus stops", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStopById(Guid id)
        {
            try
            {
                var stop = await _busStopManager.GetStopByIdAsync(id);
                if (stop == null)
                    return NotFound(new { message = "Bus stop not found" });

                return Ok(stop);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving bus stop", error = ex.Message });
            }
        }

        [HttpGet("code/{stopCode}")]
        public async Task<IActionResult> GetStopByCode(string stopCode)
        {
            try
            {
                var stop = await _busStopManager.GetStopByCodeAsync(stopCode);
                if (stop == null)
                    return NotFound(new { message = "Bus stop not found" });

                return Ok(stop);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving bus stop", error = ex.Message });
            }
        }

        [HttpGet("route/{routeId}")]
        public async Task<IActionResult> GetStopsByRouteId(Guid routeId)
        {
            try
            {
                var stops = await _busStopManager.GetStopsByRouteIdAsync(routeId);
                return Ok(stops);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving bus stops", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateStop([FromBody] CreateBusStopDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var stop = await _busStopManager.CreateStopAsync(createDto, userId);
                
                return CreatedAtAction(nameof(GetStopById), new { id = stop.Id }, stop);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Error creating bus stop", error = ex.Message, details = inner });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Error creating bus stop", error = ex.Message, details = inner });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStop(Guid id, [FromBody] UpdateBusStopDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var stop = await _busStopManager.UpdateStopAsync(id, updateDto);
                if (stop == null)
                    return NotFound(new { message = "Bus stop not found" });

                return Ok(stop);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating bus stop", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStop(Guid id)
        {
            try
            {
                var result = await _busStopManager.DeleteStopAsync(id);
                if (!result)
                    return BadRequest(new { message = "Cannot delete bus stop. It may not exist or have active student assignments." });

                return Ok(new { message = "Bus stop deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting bus stop", error = ex.Message });
            }
        }

        [HttpPost("{routeId}/reorder")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReorderStops(Guid routeId, [FromBody] List<Guid> stopIds)
        {
            try
            {
                var result = await _busStopManager.ReorderStopsAsync(routeId, stopIds);
                if (!result)
                    return BadRequest(new { message = "Failed to reorder stops" });

                return Ok(new { message = "Stops reordered successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error reordering stops", error = ex.Message });
            }
        }

        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyStops([FromQuery] decimal latitude, [FromQuery] decimal longitude, [FromQuery] double radiusKm = 5.0)
        {
            try
            {
                var stops = await _busStopManager.GetNearbyStopsAsync(latitude, longitude, radiusKm);
                return Ok(stops);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving nearby stops", error = ex.Message });
            }
        }
    }
}
