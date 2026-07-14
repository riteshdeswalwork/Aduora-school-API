using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Controllers.Shared;
using sms.api.Data;
using sms.api.Interfaces.Fee;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Fee
{
    [Route("api/online-payment-transactions")]
    [Authorize]
    public class OnlinePaymentTransactionController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IOnlinePaymentTransactionManager _manager;

        public OnlinePaymentTransactionController(ApplicationDbContext context, IOnlinePaymentTransactionManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll([FromQuery] Guid? academicYearId = null, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] string? settlementStatus = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.OnlinePaymentTransactions.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            query = ApplySettlementStatusFilter(query, settlementStatus);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.TransactionReference != null && x.TransactionReference.ToLower().Contains(term)) ||
                    (x.ReceiptNumber != null && x.ReceiptNumber.ToLower().Contains(term)) ||
                    (x.MerchantRefId != null && x.MerchantRefId.ToLower().Contains(term)) ||
                    (x.Gateway != null && x.Gateway.ToLower().Contains(term)) ||
                    (x.PaymentFor != null && x.PaymentFor.ToLower().Contains(term)) ||
                    (x.LedgerCategory != null && x.LedgerCategory.ToLower().Contains(term)) ||
                    (x.SettlementStatus != null && x.SettlementStatus.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetPaged([FromQuery] Guid? academicYearId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] string? settlementStatus = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.OnlinePaymentTransactions.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            query = ApplySettlementStatusFilter(query, settlementStatus);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.TransactionReference != null && x.TransactionReference.ToLower().Contains(term)) ||
                    (x.ReceiptNumber != null && x.ReceiptNumber.ToLower().Contains(term)) ||
                    (x.MerchantRefId != null && x.MerchantRefId.ToLower().Contains(term)) ||
                    (x.Gateway != null && x.Gateway.ToLower().Contains(term)) ||
                    (x.PaymentFor != null && x.PaymentFor.ToLower().Contains(term)) ||
                    (x.LedgerCategory != null && x.LedgerCategory.ToLower().Contains(term)) ||
                    (x.SettlementStatus != null && x.SettlementStatus.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.CreatedAt);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.OnlinePaymentTransactions.FindAsync(id);
            return entity == null ? NotFound(new { message = "Online payment transaction not found" }) : Ok(entity);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] OnlinePaymentTransaction entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            entity.Id = Guid.NewGuid();
            if (entity.AcademicYearId == default)
            {
                var yearId = GetCurrentAcademicYearId();
                if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });
                entity.AcademicYearId = yearId.Value;
            }
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = null;
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OnlinePaymentTransaction entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.OnlinePaymentTransactions.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Online payment transaction not found" });
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            entity.UpdatedAt = DateTime.UtcNow;
            if (entity.AcademicYearId == default) entity.AcademicYearId = existing.AcademicYearId;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.OnlinePaymentTransactions.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Online payment transaction not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }

        private static IQueryable<OnlinePaymentTransaction> ApplySettlementStatusFilter(IQueryable<OnlinePaymentTransaction> query, string? settlementStatus)
        {
            if (string.IsNullOrWhiteSpace(settlementStatus)) return query;
            if (settlementStatus.Equals("NotSettled", StringComparison.OrdinalIgnoreCase))
                return query.Where(x => x.SettlementStatus != "Settled");
            return query.Where(x => x.SettlementStatus == settlementStatus);
        }
    }
}
