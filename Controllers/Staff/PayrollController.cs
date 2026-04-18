using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces.Staff;
using sms.api.Models.DTOs;

namespace sms.api.Controllers.Staff
{
    [Route("api/payroll")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollManager _payroll;

        public PayrollController(IPayrollManager payroll)
        {
            _payroll = payroll;
        }

        private Guid? CurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var g) ? g : null;
        }

        [HttpGet("components")]
        public async Task<IActionResult> GetComponents()
        {
            var list = await _payroll.GetComponentsAsync();
            return Ok(list);
        }

        [HttpPost("components/seed-defaults")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedDefaults()
        {
            var n = await _payroll.SeedDefaultComponentsAsync();
            return Ok(new { added = n, message = n == 0 ? "Defaults already exist" : $"Seeded {n} components" });
        }

        [HttpGet("components/{id:guid}")]
        public async Task<IActionResult> GetComponent(Guid id)
        {
            var c = await _payroll.GetComponentByIdAsync(id);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpPost("components")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateComponent([FromBody] PayrollComponentWriteDto dto)
        {
            try
            {
                var c = await _payroll.CreateComponentAsync(dto);
                return CreatedAtAction(nameof(GetComponent), new { id = c.Id }, c);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("components/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateComponent(Guid id, [FromBody] PayrollComponentWriteDto dto)
        {
            var c = await _payroll.UpdateComponentAsync(id, dto);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpDelete("components/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteComponent(Guid id)
        {
            var ok = await _payroll.DeleteComponentAsync(id);
            return ok ? NoContent() : NotFound();
        }

        [HttpGet("periods")]
        public async Task<IActionResult> GetPeriods([FromQuery] Guid academicYearId)
        {
            if (academicYearId == Guid.Empty) return BadRequest(new { message = "academicYearId is required" });
            var list = await _payroll.GetPeriodsAsync(academicYearId);
            return Ok(list);
        }

        [HttpGet("periods/{id:guid}")]
        public async Task<IActionResult> GetPeriod(Guid id)
        {
            var p = await _payroll.GetPeriodByIdAsync(id);
            return p == null ? NotFound() : Ok(p);
        }

        [HttpPost("periods")]
        public async Task<IActionResult> CreatePeriod([FromBody] PayrollPeriodCreateDto dto)
        {
            try
            {
                var p = await _payroll.CreatePeriodAsync(dto, CurrentUserId());
                return Ok(p);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("periods/{id:guid}/recalculate")]
        public async Task<IActionResult> Recalculate(Guid id)
        {
            try
            {
                var r = await _payroll.RecalculatePeriodAsync(id);
                return Ok(r);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("periods/{id:guid}/finalize")]
        public async Task<IActionResult> Finalize(Guid id)
        {
            var ok = await _payroll.FinalizePeriodAsync(id);
            return ok ? Ok(new { message = "Finalized" }) : BadRequest(new { message = "Period must be in Calculated status" });
        }

        [HttpPost("periods/{id:guid}/mark-paid")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkPaid(Guid id)
        {
            var ok = await _payroll.MarkPeriodPaidAsync(id);
            return ok ? Ok(new { message = "Marked paid" }) : BadRequest(new { message = "Period must be Finalized" });
        }

        [HttpPost("periods/{id:guid}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Lock(Guid id)
        {
            var ok = await _payroll.LockPeriodAsync(id);
            return ok ? Ok(new { message = "Locked" }) : BadRequest(new { message = "Period must be Paid" });
        }

        [HttpPost("periods/{id:guid}/reopen")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reopen(Guid id)
        {
            var ok = await _payroll.ReopenPeriodAsync(id);
            return ok ? Ok(new { message = "Reopened to Calculated" }) : BadRequest(new { message = "Period must be Finalized or Paid" });
        }

        [HttpGet("periods/{id:guid}/slips")]
        public async Task<IActionResult> GetSlips(Guid id)
        {
            var list = await _payroll.GetSlipsForPeriodAsync(id);
            return Ok(list);
        }

        [HttpGet("slips/{id:guid}")]
        public async Task<IActionResult> GetSlip(Guid id)
        {
            var s = await _payroll.GetSlipByIdAsync(id);
            return s == null ? NotFound() : Ok(s);
        }
    }
}
