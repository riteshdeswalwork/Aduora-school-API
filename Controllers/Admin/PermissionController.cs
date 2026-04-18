using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces.Admin;
using System.Security.Claims;

namespace sms.api.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionManager _permissionManager;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(IPermissionManager permissionManager, ILogger<PermissionController> logger)
        {
            _permissionManager = permissionManager;
            _logger = logger;
        }

        [HttpGet("tree")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetPermissionTree([FromQuery] bool includeInactive = false)
        {
            try
            {
                var tree = await _permissionManager.GetPermissionTreeAsync(includeInactive);
                return Ok(tree);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permission tree");
                return StatusCode(500, new { message = "Error retrieving permission tree", error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetPermissions([FromQuery] bool includeInactive = false, [FromQuery] string? module = null)
        {
            try
            {
                var permissions = await _permissionManager.GetPermissionsAsync(includeInactive, module);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions");
                return StatusCode(500, new { message = "Error retrieving permissions", error = ex.Message });
            }
        }

        [HttpGet("modules")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetModules()
        {
            try
            {
                var modules = await _permissionManager.GetModulesAsync();
                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permission modules");
                return StatusCode(500, new { message = "Error retrieving modules", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetPermission(Guid id)
        {
            try
            {
                var permission = await _permissionManager.GetPermissionByIdAsync(id);
                if (permission == null)
                    return NotFound(new { message = "Permission not found" });
                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permission {PermissionId}", id);
                return StatusCode(500, new { message = "Error retrieving permission", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePermission([FromBody] sms.api.Models.Permission permission)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();

                var created = await _permissionManager.CreatePermissionAsync(permission, userId ?? Guid.Empty);
                _logger.LogInformation("Permission created: {PermissionCode} by user {UserId}", created.Code, userId);
                return CreatedAtAction(nameof(GetPermission), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission");
                return StatusCode(500, new { message = "Error creating permission", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] sms.api.Models.Permission permission)
        {
            try
            {
                if (id != permission.Id)
                    return BadRequest(new { message = "Permission ID mismatch" });
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();

                var updated = await _permissionManager.UpdatePermissionAsync(id, permission, userId ?? Guid.Empty);
                if (updated == null)
                    return NotFound(new { message = "Permission not found" });

                _logger.LogInformation("Permission updated: {PermissionCode} by user {UserId}", permission.Code, userId);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission {PermissionId}", id);
                return StatusCode(500, new { message = "Error updating permission", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePermission(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var deleted = await _permissionManager.DeletePermissionAsync(id, userId ?? Guid.Empty);
                if (!deleted)
                    return NotFound(new { message = "Permission not found" });

                _logger.LogInformation("Permission deleted by user {UserId}", userId);
                return Ok(new { message = "Permission deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting permission {PermissionId}", id);
                return StatusCode(500, new { message = "Error deleting permission", error = ex.Message });
            }
        }

        [HttpPost("seed-default")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedDefaultPermissions()
        {
            try
            {
                var userId = GetCurrentUserId();
                await _permissionManager.SeedDefaultPermissionsAsync(userId);
                _logger.LogInformation("Default permissions seeded by user {UserId}", userId);
                return Ok(new { message = "Default permissions seeded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default permissions");
                return StatusCode(500, new { message = "Error seeding default permissions", error = ex.Message });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
