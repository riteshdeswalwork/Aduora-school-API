using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/asset-maintenance-records")]
    public class AssetMaintenanceRecordController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAssetMaintenanceManager _manager;

        public AssetMaintenanceRecordController(ApplicationDbContext context, IAssetMaintenanceManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? assetId = null, [FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.AssetMaintenanceRecords.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (assetId.HasValue) query = query.Where(x => x.AssetId == assetId.Value);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Vendor != null && x.Vendor.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.ServiceDate).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? assetId = null, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.AssetMaintenanceRecords.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (assetId.HasValue) query = query.Where(x => x.AssetId == assetId.Value);
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status != null && x.Status.ToLower() == status.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Vendor != null && x.Vendor.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(term)) ||
                    (x.MaintenanceChecklist != null && x.MaintenanceChecklist.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.ServiceDate);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.AssetMaintenanceRecords.FindAsync(id);
            return entity == null ? NotFound(new { message = "Asset maintenance record not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AssetMaintenanceRecord entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (entity.AssetId == default) return BadRequest(new { message = "AssetId is required" });
            var assetExists = await _context.Assets.AnyAsync(a => a.Id == entity.AssetId && a.IsActive);
            if (!assetExists) return BadRequest(new { message = "Asset not found or inactive" });
            entity.Id = Guid.NewGuid();
            entity.IsActive = true;
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AssetMaintenanceRecord entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.AssetMaintenanceRecords.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Asset maintenance record not found" });
            entity.Id = id;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.AssetMaintenanceRecords.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Asset maintenance record not found" });
            existing.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
