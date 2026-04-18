using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.DTOs;
using sms.api.Interfaces;
using System.Security.Claims;

namespace sms.api.Controllers.Bus
{
    [Authorize]
    public class BusRouteController : ApiControllerBase
    {
        private readonly IBusRouteManager _busRouteManager;

        public BusRouteController(IBusRouteManager busRouteManager)
        {
            _busRouteManager = busRouteManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoutes()
        {
            try
            {
                var routes = await _busRouteManager.GetAllRoutesAsync();
                return Ok(routes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving bus routes", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRouteById(Guid id)
        {
            try
            {
                var route = await _busRouteManager.GetRouteByIdAsync(id);
                if (route == null)
                    return NotFound(new { message = "Bus route not found" });

                return Ok(route);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving bus route", error = ex.Message });
            }
        }

        [HttpGet("code/{routeCode}")]
        public async Task<IActionResult> GetRouteByCode(string routeCode)
        {
            try
            {
                var route = await _busRouteManager.GetRouteByCodeAsync(routeCode);
                if (route == null)
                    return NotFound(new { message = "Bus route not found" });

                return Ok(route);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving bus route", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRoute([FromBody] CreateBusRouteDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var route = await _busRouteManager.CreateRouteAsync(createDto, userId);
                
                return CreatedAtAction(nameof(GetRouteById), new { id = route.Id }, route);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating bus route", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRoute(Guid id, [FromBody] UpdateBusRouteDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var route = await _busRouteManager.UpdateRouteAsync(id, updateDto);
                if (route == null)
                    return NotFound(new { message = "Bus route not found" });

                return Ok(route);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating bus route", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoute(Guid id)
        {
            try
            {
                var result = await _busRouteManager.DeleteRouteAsync(id);
                if (!result)
                    return BadRequest(new { message = "Cannot delete bus route. It may not exist or have active bus stops." });

                return Ok(new { message = "Bus route deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting bus route", error = ex.Message });
            }
        }

        [HttpGet("bus/{busId}")]
        public async Task<IActionResult> GetRoutesByBusId(Guid busId)
        {
            try
            {
                var routes = await _busRouteManager.GetRoutesByBusIdAsync(busId);
                return Ok(routes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving bus routes", error = ex.Message });
            }
        }

        [HttpGet("optimized")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetOptimizedRoutes()
        {
            try
            {
                var routes = await _busRouteManager.GetOptimizedRoutesAsync();
                return Ok(routes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving optimized routes", error = ex.Message });
            }
        }

        [HttpGet("{id}/profitability")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRouteProfitability(Guid id)
        {
            try
            {
                var profitability = await _busRouteManager.CalculateRouteProfitabilityAsync(id);
                return Ok(new { routeId = id, profitability });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error calculating route profitability", error = ex.Message });
            }
        }
    }
}
