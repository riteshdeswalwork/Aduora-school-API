using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;

namespace sms.api.Controllers.Fee
{
    [Authorize]
    public class FeeDiscountController : ApiControllerBase
    {
        private readonly IFeeDiscountManager _feeDiscountManager;

        public FeeDiscountController(IFeeDiscountManager feeDiscountManager)
        {
            _feeDiscountManager = feeDiscountManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var discounts = await _feeDiscountManager.GetAllAsync();
            return Ok(discounts);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var discount = await _feeDiscountManager.GetByIdAsync(id);
            if (discount == null)
                return NotFound();

            return Ok(discount);
        }

        [HttpGet("student-fee/{studentFeeId}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetByStudentFeeId(Guid studentFeeId)
        {
            var discounts = await _feeDiscountManager.GetDiscountsByStudentFeeIdAsync(studentFeeId);
            return Ok(discounts);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetByStudentId(Guid studentId)
        {
            var discounts = await _feeDiscountManager.GetDiscountsByStudentIdAsync(studentId);
            return Ok(discounts);
        }

        [HttpGet("type/{discountType}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByType(string discountType)
        {
            var discounts = await _feeDiscountManager.GetDiscountsByTypeAsync(discountType);
            return Ok(discounts);
        }

        [HttpGet("approver/{approverId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByApproverId(Guid approverId)
        {
            var discounts = await _feeDiscountManager.GetDiscountsByApproverIdAsync(approverId);
            return Ok(discounts);
        }

        [HttpGet("student/{studentId}/total")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetTotalByStudent(Guid studentId)
        {
            var total = await _feeDiscountManager.GetTotalDiscountByStudentAsync(studentId);
            return Ok(new { StudentId = studentId, TotalDiscount = total });
        }

        [HttpGet("class/{classId}/total")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetTotalByClass(Guid classId)
        {
            var total = await _feeDiscountManager.GetTotalDiscountByClassAsync(classId);
            return Ok(new { ClassId = classId, TotalDiscount = total });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] FeeDiscount discount)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            discount.Id = Guid.NewGuid();
            discount.CreatedAt = DateTime.UtcNow;
            discount.ApprovedDate = DateTime.UtcNow;
            if (discount.ApprovedById == Guid.Empty)
                discount.ApprovedById = GetCurrentUserId();

            var createdDiscount = await _feeDiscountManager.CreateAsync(discount);
            return CreatedAtAction(nameof(GetById), new { id = createdDiscount.Id }, createdDiscount);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FeeDiscount discount)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingDiscount = await _feeDiscountManager.GetByIdAsync(id);
            if (existingDiscount == null)
                return NotFound();

            discount.Id = id;
            discount.CreatedAt = existingDiscount.CreatedAt;
            discount.UpdatedAt = DateTime.UtcNow;

            var updatedDiscount = await _feeDiscountManager.UpdateAsync(discount);
            return Ok(updatedDiscount);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _feeDiscountManager.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
