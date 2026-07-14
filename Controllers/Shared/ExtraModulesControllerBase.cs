using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using sms.api.Interfaces.Security;
using sms.api.Middleware;
using sms.api.Models.Security;
using System;
using System.Security.Claims;

namespace sms.api.Controllers.Shared
{
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public abstract class ExtraModulesControllerBase : ControllerBase
    {
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

        protected async Task<StaffScopeContext> GetDataScopeAsync()
        {
            var scopeService = HttpContext.RequestServices.GetRequiredService<IStaffDataScopeService>();
            return await scopeService.ResolveAsync(GetCurrentUserId(), GetCurrentAcademicYearId());
        }
    }
}
