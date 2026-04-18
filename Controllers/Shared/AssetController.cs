using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/assets")]
    public class AssetController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAssetManager _manager;

        public AssetController(ApplicationDbContext context, IAssetManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.Assets.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(term)) ||
                    (x.Category != null && x.Category.ToLower().Contains(term)) ||
                    (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(term)) ||
                    (x.Location != null && x.Location.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.Assets.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status != null && x.Status.ToLower() == status.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(term)) ||
                    (x.Category != null && x.Category.ToLower().Contains(term)) ||
                    (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(term)) ||
                    (x.Location != null && x.Location.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.CreatedAt);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.Assets.FindAsync(id);
            return entity == null ? NotFound(new { message = "Asset not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Asset entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            entity.Id = Guid.NewGuid();
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = null;
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Asset entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.Assets.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Asset not found" });
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            entity.UpdatedAt = DateTime.UtcNow;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.Assets.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Asset not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
