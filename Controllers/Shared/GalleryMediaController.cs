using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/gallery-media")]
    public class GalleryMediaController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IGalleryMediaManager _manager;

        public GalleryMediaController(ApplicationDbContext context, IGalleryMediaManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm = null, [FromQuery] string? mediaType = null, [FromQuery] bool? isPublic = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.GalleryMedia.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(mediaType)) query = query.Where(x => x.MediaType == mediaType);
            if (isPublic.HasValue) query = query.Where(x => x.IsPublic == isPublic.Value);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(term)) ||
                    (x.Url != null && x.Url.ToLower().Contains(term)) ||
                    (x.MediaType != null && x.MediaType.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? mediaType = null, [FromQuery] string? albumCategory = null, [FromQuery] bool? isPublic = null, [FromQuery] bool includeInactive = false)
        {
            var result = await _manager.GetPagedAsync(pageNumber, pageSize, searchTerm, mediaType, albumCategory, isPublic, includeInactive);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.GalleryMedia.FindAsync(id);
            return entity == null ? NotFound(new { message = "Media not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GalleryMedia entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            entity.Id = Guid.NewGuid();
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = null;
            if (!entity.UploadedByUserId.HasValue)
                entity.UploadedByUserId = GetCurrentUserId();
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] GalleryMedia entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.GalleryMedia.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Media not found" });
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            entity.UpdatedAt = DateTime.UtcNow;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.GalleryMedia.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Media not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
