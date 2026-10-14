using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Models;
using sms.api.Services.Data;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/biometric-user-maps")]
    public class BiometricUserMapController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISqlDataAccess _sqlDataAccess;

        public BiometricUserMapController(ApplicationDbContext context, ISqlDataAccess sqlDataAccess)
        {
            _context = context;
            _sqlDataAccess = sqlDataAccess;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? deviceId = null, [FromQuery] string? personType = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.BiometricUserMaps.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (deviceId.HasValue) query = query.Where(x => x.DeviceId == deviceId.Value);
            if (!string.IsNullOrWhiteSpace(personType)) query = query.Where(x => x.PersonType == personType);
            var items = await query.OrderBy(x => x.ExternalUserId).Take(1000).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? deviceId = null, [FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@DeviceId", deviceId);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@IncludeInactive", includeInactive);

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<BiometricUserMap>(
                "dbo.sp_BiometricUserMaps_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return Ok(new PagedResult<BiometricUserMap>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.BiometricUserMaps.FindAsync(id);
            return entity == null ? NotFound(new { message = "Mapping not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BiometricUserMap entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(entity.ExternalUserId))
                return BadRequest(new { message = "ExternalUserId (fingerprint device user id) is required" });

            var personType = string.Equals(entity.PersonType, "Student", StringComparison.OrdinalIgnoreCase) ? "Student" : "Staff";
            if (personType == "Staff" && (!entity.StaffId.HasValue || entity.StaffId == Guid.Empty))
                return BadRequest(new { message = "StaffId is required for Staff mapping" });
            if (personType == "Student" && (!entity.StudentId.HasValue || entity.StudentId == Guid.Empty))
                return BadRequest(new { message = "StudentId is required for Student mapping" });

            entity.Id = Guid.NewGuid();
            entity.ExternalUserId = entity.ExternalUserId.Trim();
            entity.PersonType = personType;
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            if (personType == "Staff") entity.StudentId = null;
            else entity.StaffId = null;

            if (string.IsNullOrWhiteSpace(entity.DisplayName))
                entity.DisplayName = await ResolveDisplayNameAsync(personType, entity.StaffId, entity.StudentId);

            _context.BiometricUserMaps.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] BiometricUserMap entity)
        {
            var existing = await _context.BiometricUserMaps.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Mapping not found" });

            var personType = string.Equals(entity.PersonType, "Student", StringComparison.OrdinalIgnoreCase) ? "Student" : "Staff";
            existing.DeviceId = entity.DeviceId;
            existing.ExternalUserId = (entity.ExternalUserId ?? existing.ExternalUserId).Trim();
            existing.PersonType = personType;
            existing.StaffId = personType == "Staff" ? entity.StaffId : null;
            existing.StudentId = personType == "Student" ? entity.StudentId : null;
            existing.DisplayName = string.IsNullOrWhiteSpace(entity.DisplayName)
                ? await ResolveDisplayNameAsync(personType, existing.StaffId, existing.StudentId)
                : entity.DisplayName.Trim();
            existing.IsActive = entity.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.BiometricUserMaps.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Mapping not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Mapping deactivated" });
        }

        private async Task<string?> ResolveDisplayNameAsync(string personType, Guid? staffId, Guid? studentId)
        {
            if (personType == "Staff" && staffId.HasValue)
            {
                var staff = await _context.Staff.AsNoTracking().Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == staffId.Value);
                if (staff?.User != null)
                    return $"{staff.User.FirstName} {staff.User.LastName}".Trim();
            }
            if (personType == "Student" && studentId.HasValue)
            {
                var student = await _context.Students.AsNoTracking().Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == studentId.Value);
                if (student?.User != null)
                    return $"{student.User.FirstName} {student.User.LastName}".Trim();
            }
            return null;
        }
    }
}
