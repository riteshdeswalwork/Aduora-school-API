using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Models;

namespace sms.api.Controllers.Shared
{
    [Route("api")]
    public class BiometricFingerTemplateController : ExtraModulesControllerBase
    {
        private static readonly string[] FingerCodes =
        {
            "L1", "L2", "L3", "L4", "L5",
            "R1", "R2", "R3", "R4", "R5"
        };

        private readonly ApplicationDbContext _context;

        public BiometricFingerTemplateController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class FingerDto
        {
            public int FingerIndex { get; set; }
            public string? FingerCode { get; set; }
            public string IsoTemplate { get; set; } = string.Empty;
            public int Quality { get; set; }
        }

        public class FingerListItemDto
        {
            public Guid Id { get; set; }
            public int FingerIndex { get; set; }
            public string FingerCode { get; set; } = string.Empty;
            public int Quality { get; set; }
            public bool HasTemplate { get; set; }
            /// <summary>Omitted on list-by-person for UI; included on active gallery.</summary>
            public string? IsoTemplate { get; set; }
        }

        public class ActiveGalleryItemDto
        {
            public Guid Id { get; set; }
            public string PersonType { get; set; } = string.Empty;
            public Guid? StaffId { get; set; }
            public Guid? StudentId { get; set; }
            public string? PersonName { get; set; }
            public int FingerIndex { get; set; }
            public string FingerCode { get; set; } = string.Empty;
            public string IsoTemplate { get; set; } = string.Empty;
            public int Quality { get; set; }
        }

        [HttpGet("staff/{staffId:guid}/fingerprints")]
        public async Task<IActionResult> GetStaffFingerprints(Guid staffId)
        {
            var exists = await _context.Staff.AsNoTracking().AnyAsync(s => s.Id == staffId);
            if (!exists) return NotFound(new { message = "Staff not found" });
            return Ok(await ListForPersonAsync("Staff", staffId, null, includeTemplate: true));
        }

        [HttpPut("staff/{staffId:guid}/fingerprints")]
        public async Task<IActionResult> PutStaffFingerprints(Guid staffId, [FromBody] List<FingerDto>? fingers)
        {
            var exists = await _context.Staff.AsNoTracking().AnyAsync(s => s.Id == staffId);
            if (!exists) return NotFound(new { message = "Staff not found" });
            try
            {
                await ReplaceAsync("Staff", staffId, null, fingers);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            return Ok(await ListForPersonAsync("Staff", staffId, null, includeTemplate: true));
        }

        [HttpGet("students/{studentId:guid}/fingerprints")]
        public async Task<IActionResult> GetStudentFingerprints(Guid studentId)
        {
            var exists = await _context.Students.AsNoTracking().AnyAsync(s => s.Id == studentId);
            if (!exists) return NotFound(new { message = "Student not found" });
            return Ok(await ListForPersonAsync("Student", null, studentId, includeTemplate: true));
        }

        [HttpPut("students/{studentId:guid}/fingerprints")]
        public async Task<IActionResult> PutStudentFingerprints(Guid studentId, [FromBody] List<FingerDto>? fingers)
        {
            var exists = await _context.Students.AsNoTracking().AnyAsync(s => s.Id == studentId);
            if (!exists) return NotFound(new { message = "Student not found" });
            try
            {
                await ReplaceAsync("Student", null, studentId, fingers);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            return Ok(await ListForPersonAsync("Student", null, studentId, includeTemplate: true));
        }

        /// <summary>
        /// Active ISO gallery for browser-side MFS100 MatchISO (attendance PC only).
        /// </summary>
        [HttpGet("biometric/fingerprints/active")]
        public async Task<IActionResult> GetActiveGallery([FromQuery] string? personType = null)
        {
            var query = _context.BiometricFingerTemplates.AsNoTracking()
                .Where(t => t.IsActive && t.IsoTemplate != null && t.IsoTemplate != "");

            if (!string.IsNullOrWhiteSpace(personType))
            {
                var pt = personType.Trim();
                query = query.Where(t => t.PersonType == pt);
            }

            // Index/thumb + higher quality first → faster 1:N early-exit on the client/bridge
            var rows = await query
                .OrderBy(t => t.FingerIndex == 2 || t.FingerIndex == 7 ? 0
                    : t.FingerIndex == 1 || t.FingerIndex == 6 ? 1 : 2)
                .ThenByDescending(t => t.Quality)
                .ThenBy(t => t.PersonType)
                .ToListAsync();

            var staffIds = rows.Where(r => r.StaffId.HasValue).Select(r => r.StaffId!.Value).Distinct().ToList();
            var studentIds = rows.Where(r => r.StudentId.HasValue).Select(r => r.StudentId!.Value).Distinct().ToList();

            var staffNames = await _context.Staff.AsNoTracking()
                .Where(s => staffIds.Contains(s.Id))
                .Select(s => new { s.Id, Name = ((s.User!.FirstName ?? "") + " " + (s.User.LastName ?? "")).Trim() })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var studentNames = await _context.Students.AsNoTracking()
                .Where(s => studentIds.Contains(s.Id))
                .Select(s => new { s.Id, Name = ((s.User!.FirstName ?? "") + " " + (s.User.LastName ?? "")).Trim() })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var result = rows.Select(t => new ActiveGalleryItemDto
            {
                Id = t.Id,
                PersonType = t.PersonType,
                StaffId = t.StaffId,
                StudentId = t.StudentId,
                PersonName = t.StaffId.HasValue && staffNames.TryGetValue(t.StaffId.Value, out var sn) ? sn
                    : t.StudentId.HasValue && studentNames.TryGetValue(t.StudentId.Value, out var stn) ? stn
                    : null,
                FingerIndex = t.FingerIndex,
                FingerCode = t.FingerCode,
                IsoTemplate = t.IsoTemplate,
                Quality = t.Quality
            }).ToList();

            return Ok(result);
        }

        private async Task<List<FingerListItemDto>> ListForPersonAsync(string personType, Guid? staffId, Guid? studentId, bool includeTemplate)
        {
            var query = _context.BiometricFingerTemplates.AsNoTracking()
                .Where(t => t.IsActive && t.PersonType == personType);
            if (staffId.HasValue) query = query.Where(t => t.StaffId == staffId);
            if (studentId.HasValue) query = query.Where(t => t.StudentId == studentId);

            var existing = await query.ToListAsync();
            var byIndex = existing.ToDictionary(t => t.FingerIndex);

            var list = new List<FingerListItemDto>();
            for (var i = 1; i <= 10; i++)
            {
                if (byIndex.TryGetValue(i, out var t))
                {
                    list.Add(new FingerListItemDto
                    {
                        Id = t.Id,
                        FingerIndex = i,
                        FingerCode = string.IsNullOrWhiteSpace(t.FingerCode) ? FingerCodes[i - 1] : t.FingerCode,
                        Quality = t.Quality,
                        HasTemplate = !string.IsNullOrWhiteSpace(t.IsoTemplate),
                        IsoTemplate = includeTemplate ? t.IsoTemplate : null
                    });
                }
                else
                {
                    list.Add(new FingerListItemDto
                    {
                        Id = Guid.Empty,
                        FingerIndex = i,
                        FingerCode = FingerCodes[i - 1],
                        Quality = 0,
                        HasTemplate = false,
                        IsoTemplate = null
                    });
                }
            }
            return list;
        }

        private async Task ReplaceAsync(string personType, Guid? staffId, Guid? studentId, List<FingerDto>? fingers)
        {
            fingers ??= new List<FingerDto>();
            if (fingers.Count > 10)
                throw new InvalidOperationException("Maximum 10 fingerprints per person");

            var seen = new HashSet<int>();
            foreach (var f in fingers)
            {
                if (f.FingerIndex < 1 || f.FingerIndex > 10)
                    throw new InvalidOperationException($"Invalid FingerIndex {f.FingerIndex}");
                if (!seen.Add(f.FingerIndex))
                    throw new InvalidOperationException($"Duplicate FingerIndex {f.FingerIndex}");
                if (string.IsNullOrWhiteSpace(f.IsoTemplate))
                    throw new InvalidOperationException($"IsoTemplate required for finger {f.FingerIndex}");
            }

            var existing = await _context.BiometricFingerTemplates
                .Where(t => t.PersonType == personType
                    && (staffId == null || t.StaffId == staffId)
                    && (studentId == null || t.StudentId == studentId))
                .ToListAsync();

            foreach (var row in existing)
            {
                row.IsActive = false;
                row.UpdatedAt = DateTime.UtcNow;
            }

            foreach (var f in fingers)
            {
                var code = string.IsNullOrWhiteSpace(f.FingerCode)
                    ? FingerCodes[f.FingerIndex - 1]
                    : f.FingerCode.Trim();

                _context.BiometricFingerTemplates.Add(new BiometricFingerTemplate
                {
                    Id = Guid.NewGuid(),
                    PersonType = personType,
                    StaffId = staffId,
                    StudentId = studentId,
                    FingerIndex = f.FingerIndex,
                    FingerCode = code,
                    IsoTemplate = f.IsoTemplate.Trim(),
                    Quality = Math.Max(0, f.Quality),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
