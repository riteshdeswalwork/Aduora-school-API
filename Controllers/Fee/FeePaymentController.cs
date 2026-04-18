using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;

namespace sms.api.Controllers.Fee
{
    [Route("api/fee-payments")]
    [Authorize]
    public class FeePaymentController : ApiControllerBase
    {
        private readonly IFeePaymentManager _feePaymentManager;

        public FeePaymentController(IFeePaymentManager feePaymentManager)
        {
            _feePaymentManager = feePaymentManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _feePaymentManager.GetAllAsync();
            return Ok(payments);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Guid? academicYearId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] Guid? classId = null)
        {
            var result = await _feePaymentManager.GetPaymentsPagedAsync(pageNumber, pageSize, academicYearId, fromDate, toDate, classId);
            return Ok(result);
        }

        [HttpGet("day-book/summary")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetDayBookSummary(
            [FromQuery] Guid? academicYearId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] Guid? classId = null)
        {
            var rows = await _feePaymentManager.GetDayBookSummaryAsync(academicYearId, fromDate, toDate, classId);
            return Ok(rows);
        }

        [HttpGet("day-book/detail")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetDayBookDetail(
            [FromQuery] Guid? academicYearId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] Guid? classId = null,
            [FromQuery] Guid? receivedById = null)
        {
            var rows = await _feePaymentManager.GetDayBookDetailAsync(academicYearId, fromDate, toDate, classId, receivedById);
            return Ok(rows);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var payment = await _feePaymentManager.GetByIdAsync(id);
            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        [HttpGet("student-fee/{studentFeeId}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetByStudentFeeId(Guid studentFeeId)
        {
            var payments = await _feePaymentManager.GetPaymentsByStudentFeeIdAsync(studentFeeId);
            return Ok(payments);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetByStudentId(Guid studentId)
        {
            var payments = await _feePaymentManager.GetPaymentsByStudentIdAsync(studentId);
            return Ok(payments);
        }

        [HttpGet("date-range")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var payments = await _feePaymentManager.GetPaymentsByDateRangeAsync(fromDate, toDate);
            return Ok(payments);
        }

        [HttpGet("received-by/{receivedById}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByReceivedById(Guid receivedById)
        {
            var payments = await _feePaymentManager.GetPaymentsByReceivedByIdAsync(receivedById);
            return Ok(payments);
        }

        [HttpGet("class/{classId}/total")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetTotalByClass(Guid classId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var total = await _feePaymentManager.GetTotalPaymentsByClassAsync(classId, fromDate, toDate);
            return Ok(new { ClassId = classId, TotalPayments = total, FromDate = fromDate, ToDate = toDate });
        }

        [HttpGet("student/{studentId}/total")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetTotalByStudent(Guid studentId)
        {
            var total = await _feePaymentManager.GetTotalPaymentsByStudentAsync(studentId);
            return Ok(new { StudentId = studentId, TotalPayments = total });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] FeePayment payment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Only Admin and Staff can record payments
            if (currentUserRole == "Student")
                return Forbid("Students cannot record payments");

            payment.Id = Guid.NewGuid();
            payment.CreatedAt = DateTime.UtcNow;
            payment.ReceivedById = currentUserId;

            var createdPayment = await _feePaymentManager.ProcessPaymentAsync(payment);
            return CreatedAtAction(nameof(GetById), new { id = createdPayment.Id }, createdPayment);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FeePayment payment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingPayment = await _feePaymentManager.GetByIdAsync(id);
            if (existingPayment == null)
                return NotFound();

            payment.Id = id;
            payment.CreatedAt = existingPayment.CreatedAt;
            payment.UpdatedAt = DateTime.UtcNow;

            var updatedPayment = await _feePaymentManager.UpdateAsync(payment);
            return Ok(updatedPayment);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _feePaymentManager.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id:guid}/void")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VoidPayment(Guid id, [FromBody] VoidPaymentRequest? body)
        {
            var userId = GetCurrentUserId();
            var ok = await _feePaymentManager.VoidPaymentAsync(id, userId, body?.Reason);
            if (!ok)
                return NotFound();
            return Ok(new { Message = "Payment voided.", PaymentId = id });
        }

        [HttpPost("collect")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CollectFeePayment([FromBody] CollectFeePaymentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Only Admin and Staff can collect payments
            if (currentUserRole == "Student")
                return Forbid("Students cannot collect payments");

            try
            {
                var result = await _feePaymentManager.CollectFeePaymentAsync(request, currentUserId);
                if (result == null)
                    return BadRequest("Payment collection failed");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error collecting payment: {ex.Message}");
            }
        }

        [HttpGet("generate-receipt-number")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GenerateReceiptNumber()
        {
            var receiptNumber = await _feePaymentManager.GenerateReceiptNumberAsync();
            return Ok(new { ReceiptNumber = receiptNumber });
        }
    }

    public class CollectFeePaymentRequest
    {
        public Guid StudentFeeId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionReference { get; set; }
        public string? Remarks { get; set; }
        public bool GenerateReceipt { get; set; } = true;
    }

    public class VoidPaymentRequest
    {
        public string? Reason { get; set; }
    }
}
