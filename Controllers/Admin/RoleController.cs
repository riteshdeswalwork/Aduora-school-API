using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces.Admin;
using System.Security.Claims;

namespace sms.api.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleManager _roleManager;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IRoleManager roleManager, ILogger<RoleController> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetRoles([FromQuery] bool includeInactive = false)
        {
            try
            {
                var roles = await _roleManager.GetRolesAsync(includeInactive);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return StatusCode(500, new { message = "Error retrieving roles", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetRole(Guid id)
        {
            try
            {
                var role = await _roleManager.GetRoleByIdAsync(id);
                if (role == null)
                    return NotFound(new { message = "Role not found" });
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role {RoleId}", id);
                return StatusCode(500, new { message = "Error retrieving role", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole([FromBody] sms.api.Models.Role role)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();
                if (userId == null)
                    return BadRequest(new { message = "Invalid user context" });

                var created = await _roleManager.CreateRoleAsync(role, userId.Value);
                _logger.LogInformation("Role created: {RoleName} by user {UserId}", created.Name, userId);
                return CreatedAtAction(nameof(GetRole), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return StatusCode(500, new { message = "Error creating role", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] sms.api.Models.Role role)
        {
            try
            {
                if (id != role.Id)
                    return BadRequest(new { message = "Role ID mismatch" });
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();
                if (userId == null)
                    return BadRequest(new { message = "Invalid user context" });

                var updated = await _roleManager.UpdateRoleAsync(id, role, userId.Value);
                if (updated == null)
                    return NotFound(new { message = "Role not found" });

                _logger.LogInformation("Role updated: {RoleName} by user {UserId}", role.Name, userId);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {RoleId}", id);
                return StatusCode(500, new { message = "Error updating role", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return BadRequest(new { message = "Invalid user context" });

                var deleted = await _roleManager.DeleteRoleAsync(id, userId.Value);
                if (!deleted)
                    return NotFound(new { message = "Role not found" });

                _logger.LogInformation("Role deleted by user {UserId}", userId);
                return Ok(new { message = "Role deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {RoleId}", id);
                return StatusCode(500, new { message = "Error deleting role", error = ex.Message });
            }
        }

        [HttpGet("{id}/permissions")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetRolePermissions(Guid id)
        {
            try
            {
                var permissions = await _roleManager.GetRolePermissionsAsync(id);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role permissions for role {RoleId}", id);
                return StatusCode(500, new { message = "Error retrieving role permissions", error = ex.Message });
            }
        }

        [HttpPost("{id}/permissions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignPermissionsToRole(Guid id, [FromBody] List<Guid> permissionIds)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return BadRequest(new { message = "Invalid user context" });

                var success = await _roleManager.AssignPermissionsToRoleAsync(id, permissionIds ?? new List<Guid>(), userId.Value);
                if (!success)
                    return NotFound(new { message = "Role not found" });

                _logger.LogInformation("Permissions assigned to role {RoleId} by user {UserId}", id, userId);
                return Ok(new { message = "Permissions assigned successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning permissions to role {RoleId}", id);
                return StatusCode(500, new { message = "Error assigning permissions", error = ex.Message });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
