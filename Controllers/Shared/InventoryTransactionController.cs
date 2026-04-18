using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/inventory-transactions")]
    public class InventoryTransactionController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryTransactionManager _manager;

        public InventoryTransactionController(ApplicationDbContext context, IInventoryTransactionManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? inventoryItemId = null, [FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.InventoryTransactions.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (inventoryItemId.HasValue) query = query.Where(x => x.InventoryItemId == inventoryItemId.Value);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.TransactionType != null && x.TransactionType.ToLower().Contains(term)) ||
                    (x.Remarks != null && x.Remarks.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.TransactionDate).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? inventoryItemId = null, [FromQuery] string? transactionType = null, [FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.InventoryTransactions.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (inventoryItemId.HasValue) query = query.Where(x => x.InventoryItemId == inventoryItemId.Value);
            if (!string.IsNullOrWhiteSpace(transactionType))
            {
                var t = transactionType.Trim().ToUpper();
                if (t == "IN" || t == "OUT") query = query.Where(x => x.TransactionType == t);
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.TransactionType != null && x.TransactionType.ToLower().Contains(term)) ||
                    (x.Remarks != null && x.Remarks.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.TransactionDate);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.InventoryTransactions.FindAsync(id);
            return entity == null ? NotFound(new { message = "Inventory transaction not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InventoryTransaction entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (entity.InventoryItemId == default) return BadRequest(new { message = "InventoryItemId is required" });

            var item = await _context.InventoryItems.FirstOrDefaultAsync(x => x.Id == entity.InventoryItemId);
            if (item == null) return NotFound(new { message = "Inventory item not found" });
            if (!item.IsActive) return BadRequest(new { message = "Inventory item is inactive" });

            var type = (entity.TransactionType ?? "IN").Trim().ToUpper();
            if (type != "IN" && type != "OUT") return BadRequest(new { message = "TransactionType must be IN or OUT" });
            if (entity.Quantity <= 0) return BadRequest(new { message = "Quantity must be greater than 0" });

            if (!entity.PerformedByUserId.HasValue)
                entity.PerformedByUserId = GetCurrentUserId();

            entity.TransactionType = type;
            entity.Id = Guid.NewGuid();
            entity.IsActive = true;
            if (entity.TransactionDate == default) entity.TransactionDate = DateTime.UtcNow;

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                if (type == "OUT" && item.Quantity < entity.Quantity)
                    return BadRequest(new { message = "Insufficient stock" });

                item.Quantity = type == "IN" ? item.Quantity + entity.Quantity : item.Quantity - entity.Quantity;
                item.UpdatedAt = DateTime.UtcNow;

                _context.InventoryTransactions.Add(entity);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InventoryTransaction entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.InventoryTransactions.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Inventory transaction not found" });
            return BadRequest(new { message = "Updating inventory transactions is not supported. Create a new adjustment transaction instead." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return BadRequest(new { message = "Deleting inventory transactions is not supported. Create a reversal transaction instead." });
        }
    }
}
