using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;

namespace sms.api.Controllers.Fee
{
    [Authorize]
    [Route("api/fee-receipts")]
    public class FeeReceiptController : ApiControllerBase
    {
        private readonly IFeeReceiptManager _feeReceiptManager;

        public FeeReceiptController(IFeeReceiptManager feeReceiptManager)
        {
            _feeReceiptManager = feeReceiptManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var receipts = await _feeReceiptManager.GetAllAsync();
            return Ok(receipts);
        }

        [HttpPost("{id:guid}/reprint")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> RecordReprint(Guid id)
        {
            var updated = await _feeReceiptManager.RecordReprintAsync(id);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var receipt = await _feeReceiptManager.GetByIdAsync(id);
            if (receipt == null)
                return NotFound();

            return Ok(receipt);
        }

        [HttpGet("payment/{paymentId}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetByPaymentId(Guid paymentId)
        {
            var receipts = await _feeReceiptManager.GetReceiptsByPaymentIdAsync(paymentId);
            return Ok(receipts);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetByStudentId(Guid studentId)
        {
            var receipts = await _feeReceiptManager.GetReceiptsByStudentIdAsync(studentId);
            return Ok(receipts);
        }

        [HttpGet("number/{receiptNumber}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetByReceiptNumber(string receiptNumber)
        {
            var receipt = await _feeReceiptManager.GetReceiptByNumberAsync(receiptNumber);
            if (receipt == null)
                return NotFound();

            return Ok(receipt);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] FeeReceipt receipt)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            receipt.Id = Guid.NewGuid();
            receipt.CreatedAt = DateTime.UtcNow;
            receipt.IssueDate = DateTime.UtcNow;

            if (string.IsNullOrEmpty(receipt.ReceiptNumber))
            {
                receipt.ReceiptNumber = await _feeReceiptManager.GenerateReceiptNumberAsync();
            }

            var createdReceipt = await _feeReceiptManager.CreateAsync(receipt);
            return CreatedAtAction(nameof(GetById), new { id = createdReceipt.Id }, createdReceipt);
        }

        [HttpPost("generate/{paymentId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GenerateReceipt(Guid paymentId, [FromBody] GenerateReceiptRequest request)
        {
            var receipt = await _feeReceiptManager.GenerateReceiptAsync(paymentId, request.IssuedById);
            if (receipt == null)
                return BadRequest("Payment not found or receipt generation failed");

            return CreatedAtAction(nameof(GetById), new { id = receipt.Id }, receipt);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FeeReceipt receipt)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingReceipt = await _feeReceiptManager.GetByIdAsync(id);
            if (existingReceipt == null)
                return NotFound();

            receipt.Id = id;
            receipt.CreatedAt = existingReceipt.CreatedAt;
            receipt.UpdatedAt = DateTime.UtcNow;

            var updatedReceipt = await _feeReceiptManager.UpdateAsync(receipt);
            return Ok(updatedReceipt);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _feeReceiptManager.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("generate-receipt-number")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GenerateReceiptNumber()
        {
            var receiptNumber = await _feeReceiptManager.GenerateReceiptNumberAsync();
            return Ok(new { ReceiptNumber = receiptNumber });
        }
    }

    public class GenerateReceiptRequest
    {
        public Guid IssuedById { get; set; }
    }
}
