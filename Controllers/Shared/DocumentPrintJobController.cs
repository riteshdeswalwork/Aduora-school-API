using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/document-print-jobs")]
    public class DocumentPrintJobController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDocumentPrintJobManager _manager;

        public DocumentPrintJobController(ApplicationDbContext context, IDocumentPrintJobManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.DocumentPrintJobs.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.DocumentType != null && x.DocumentType.ToLower().Contains(term)) ||
                    (x.TemplateName != null && x.TemplateName.ToLower().Contains(term)) ||
                    (x.TargetReference != null && x.TargetReference.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.RequestedAt).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var result = await _manager.GetPagedAsync(pageNumber, pageSize, searchTerm, status, includeInactive);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.DocumentPrintJobs.FindAsync(id);
            return entity == null ? NotFound(new { message = "Print job not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DocumentPrintJob entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            entity.Id = Guid.NewGuid();
            entity.IsActive = true;
            if (!entity.RequestedByUserId.HasValue)
                entity.RequestedByUserId = GetCurrentUserId();
            entity.RequestedAt = DateTime.UtcNow;
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DocumentPrintJob entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.DocumentPrintJobs.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Print job not found" });
            entity.Id = id;
            entity.RequestedAt = existing.RequestedAt;
            if (!entity.RequestedByUserId.HasValue)
                entity.RequestedByUserId = existing.RequestedByUserId ?? GetCurrentUserId();
            entity.RequestedAt = existing.RequestedAt;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }
        [HttpPut("{id}/mark-printed")]
        public async Task<IActionResult> MarkPrinted(Guid id)
        {
            var existing = await _context.DocumentPrintJobs.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Document print job not found" });
            if (!existing.IsActive) return BadRequest(new { message = "Document print job is inactive" });
            existing.Status = "Printed";
            existing.PrintedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.DocumentPrintJobs.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Document print job not found" });
            existing.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
