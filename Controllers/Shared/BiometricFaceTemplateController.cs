using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Models;

namespace sms.api.Controllers.Shared
{
    [Route("api")]
    public class BiometricFaceTemplateController : ExtraModulesControllerBase
    {
        public const string DefaultModelVersion = "face-api-ssd-mobilenet-v1";

        private readonly ApplicationDbContext _context;

        public BiometricFaceTemplateController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class FaceDto
        {
            public int SlotIndex { get; set; }
            public string EmbeddingJson { get; set; } = "[]";
            public string? ModelVersion { get; set; }
            public int Quality { get; set; }
            public string? ThumbnailBase64 { get; set; }
        }

        public class FaceListItemDto
        {
            public Guid Id { get; set; }
            public int SlotIndex { get; set; }
            public int Quality { get; set; }
            public string ModelVersion { get; set; } = DefaultModelVersion;
            public bool HasTemplate { get; set; }
            public string? EmbeddingJson { get; set; }
            public string? ThumbnailBase64 { get; set; }
        }

        public class ActiveFaceGalleryItemDto
        {
            public Guid Id { get; set; }
            public string PersonType { get; set; } = string.Empty;
            public Guid? StaffId { get; set; }
            public Guid? StudentId { get; set; }
            public string? PersonName { get; set; }
            public int SlotIndex { get; set; }
            public string EmbeddingJson { get; set; } = "[]";
            public string ModelVersion { get; set; } = DefaultModelVersion;
            public int Quality { get; set; }
        }

        [HttpGet("staff/{staffId:guid}/faces")]
        public async Task<IActionResult> GetStaffFaces(Guid staffId)
        {
            var exists = await _context.Staff.AsNoTracking().AnyAsync(s => s.Id == staffId);
            if (!exists) return NotFound(new { message = "Staff not found" });
            return Ok(await ListForPersonAsync("Staff", staffId, null, includeEmbedding: true));
        }

        [HttpPut("staff/{staffId:guid}/faces")]
        public async Task<IActionResult> PutStaffFaces(Guid staffId, [FromBody] List<FaceDto>? faces)
        {
            var exists = await _context.Staff.AsNoTracking().AnyAsync(s => s.Id == staffId);
            if (!exists) return NotFound(new { message = "Staff not found" });
            try
            {
                await ReplaceAsync("Staff", staffId, null, faces);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            return Ok(await ListForPersonAsync("Staff", staffId, null, includeEmbedding: true));
        }

        [HttpGet("students/{studentId:guid}/faces")]
        public async Task<IActionResult> GetStudentFaces(Guid studentId)
        {
            var exists = await _context.Students.AsNoTracking().AnyAsync(s => s.Id == studentId);
            if (!exists) return NotFound(new { message = "Student not found" });
            return Ok(await ListForPersonAsync("Student", null, studentId, includeEmbedding: true));
        }

        [HttpPut("students/{studentId:guid}/faces")]
        public async Task<IActionResult> PutStudentFaces(Guid studentId, [FromBody] List<FaceDto>? faces)
        {
            var exists = await _context.Students.AsNoTracking().AnyAsync(s => s.Id == studentId);
            if (!exists) return NotFound(new { message = "Student not found" });
            try
            {
                await ReplaceAsync("Student", null, studentId, faces);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            return Ok(await ListForPersonAsync("Student", null, studentId, includeEmbedding: true));
        }

        /// <summary>
        /// Active face embedding gallery for browser-side 1:N cosine match.
        /// </summary>
        [HttpGet("biometric/faces/active")]
        public async Task<IActionResult> GetActiveGallery([FromQuery] string? personType = null)
        {
            var query = _context.BiometricFaceTemplates.AsNoTracking()
                .Where(t => t.IsActive && t.EmbeddingJson != null && t.EmbeddingJson != "" && t.EmbeddingJson != "[]");

            if (!string.IsNullOrWhiteSpace(personType))
            {
                var pt = personType.Trim();
                query = query.Where(t => t.PersonType == pt);
            }

            var rows = await query
                .OrderByDescending(t => t.Quality)
                .ThenBy(t => t.SlotIndex)
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

            var result = rows.Select(t => new ActiveFaceGalleryItemDto
            {
                Id = t.Id,
                PersonType = t.PersonType,
                StaffId = t.StaffId,
                StudentId = t.StudentId,
                PersonName = t.StaffId.HasValue && staffNames.TryGetValue(t.StaffId.Value, out var sn) ? sn
                    : t.StudentId.HasValue && studentNames.TryGetValue(t.StudentId.Value, out var stn) ? stn
                    : null,
                SlotIndex = t.SlotIndex,
                EmbeddingJson = t.EmbeddingJson,
                ModelVersion = t.ModelVersion,
                Quality = t.Quality
            }).ToList();

            return Ok(result);
        }

        private async Task<List<FaceListItemDto>> ListForPersonAsync(string personType, Guid? staffId, Guid? studentId, bool includeEmbedding)
        {
            var query = _context.BiometricFaceTemplates.AsNoTracking()
                .Where(t => t.IsActive && t.PersonType == personType);
            if (staffId.HasValue) query = query.Where(t => t.StaffId == staffId);
            if (studentId.HasValue) query = query.Where(t => t.StudentId == studentId);

            var existing = await query.ToListAsync();
            var bySlot = existing.ToDictionary(t => t.SlotIndex);

            var list = new List<FaceListItemDto>();
            for (var i = 1; i <= 3; i++)
            {
                if (bySlot.TryGetValue(i, out var t))
                {
                    list.Add(new FaceListItemDto
                    {
                        Id = t.Id,
                        SlotIndex = i,
                        Quality = t.Quality,
                        ModelVersion = t.ModelVersion,
                        HasTemplate = !string.IsNullOrWhiteSpace(t.EmbeddingJson) && t.EmbeddingJson != "[]",
                        EmbeddingJson = includeEmbedding ? t.EmbeddingJson : null,
                        ThumbnailBase64 = t.ThumbnailBase64
                    });
                }
                else
                {
                    list.Add(new FaceListItemDto
                    {
                        Id = Guid.Empty,
                        SlotIndex = i,
                        Quality = 0,
                        ModelVersion = DefaultModelVersion,
                        HasTemplate = false,
                        EmbeddingJson = null,
                        ThumbnailBase64 = null
                    });
                }
            }
            return list;
        }

        private async Task ReplaceAsync(string personType, Guid? staffId, Guid? studentId, List<FaceDto>? faces)
        {
            faces ??= new List<FaceDto>();
            if (faces.Count > 3)
                throw new InvalidOperationException("Maximum 3 face samples per person");

            var seen = new HashSet<int>();
            foreach (var f in faces)
            {
                if (f.SlotIndex < 1 || f.SlotIndex > 3)
                    throw new InvalidOperationException($"Invalid SlotIndex {f.SlotIndex}");
                if (!seen.Add(f.SlotIndex))
                    throw new InvalidOperationException($"Duplicate SlotIndex {f.SlotIndex}");
                if (string.IsNullOrWhiteSpace(f.EmbeddingJson) || f.EmbeddingJson == "[]")
                    throw new InvalidOperationException($"EmbeddingJson required for slot {f.SlotIndex}");
            }

            var existing = await _context.BiometricFaceTemplates
                .Where(t => t.PersonType == personType
                    && (staffId == null || t.StaffId == staffId)
                    && (studentId == null || t.StudentId == studentId))
                .ToListAsync();

            _context.BiometricFaceTemplates.RemoveRange(existing);

            var now = DateTime.UtcNow;
            foreach (var f in faces)
            {
                _context.BiometricFaceTemplates.Add(new BiometricFaceTemplate
                {
                    Id = Guid.NewGuid(),
                    PersonType = personType,
                    StaffId = staffId,
                    StudentId = studentId,
                    SlotIndex = f.SlotIndex,
                    EmbeddingJson = f.EmbeddingJson.Trim(),
                    ModelVersion = string.IsNullOrWhiteSpace(f.ModelVersion) ? DefaultModelVersion : f.ModelVersion.Trim(),
                    Quality = f.Quality,
                    ThumbnailBase64 = string.IsNullOrWhiteSpace(f.ThumbnailBase64) ? null : f.ThumbnailBase64,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
