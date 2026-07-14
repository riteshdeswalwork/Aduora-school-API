using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using sms.api.Interfaces.Security;
using sms.api.Middleware;
using sms.api.Models.Security;

namespace sms.api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>Academic year selected in the frontend header (from X-Academic-Year-Id). Use for list/add/edit scope.</summary>
        protected Guid? GetCurrentAcademicYearId()
        {
            if (HttpContext.Items.TryGetValue(AcademicYearHeaderMiddleware.HttpContextKey, out var obj) && obj is Guid id)
                return id;
            if (Request.Headers.TryGetValue(AcademicYearHeaderMiddleware.HeaderName, out var v) && Guid.TryParse(v.ToString().Trim(), out var parsed))
                return parsed;
            return null;
        }

        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in claims");
        }

        protected string GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            return roleClaim?.Value ?? throw new UnauthorizedAccessException("User role not found in claims");
        }

        protected string? GetCurrentUserEmail()
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email) ?? User.FindFirst("email");
            return emailClaim?.Value;
        }

        protected async Task<StaffScopeContext> GetDataScopeAsync()
        {
            var scopeService = HttpContext.RequestServices.GetRequiredService<IStaffDataScopeService>();
            return await scopeService.ResolveAsync(GetCurrentUserId(), GetCurrentAcademicYearId());
        }
    }
}
