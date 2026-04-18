using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.DTOs;
using sms.api.Interfaces;
using sms.api.Models;

namespace sms.api.Controllers.Student
{
    [Route("api/fees")]
    [Authorize]
    public class StudentFeeController : ApiControllerBase
    {
        private readonly IStudentFeeManager _studentFeeManager;
        private readonly IStudentManager _studentManager;
        private readonly IStaffManager _staffManager;
        private readonly IClassManager _classManager;
        private readonly IClassEnrollmentManager _classEnrollmentManager;

        public StudentFeeController(
            IStudentFeeManager studentFeeManager,
            IStudentManager studentManager,
            IStaffManager staffManager,
            IClassManager classManager,
            IClassEnrollmentManager classEnrollmentManager)
        {
            _studentFeeManager = studentFeeManager;
            _studentManager = studentManager;
            _staffManager = staffManager;
            _classManager = classManager;
            _classEnrollmentManager = classEnrollmentManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetAll()
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (currentUserRole == "Student")
            {
                // Students can only see their own fees
                var student = await _studentManager.GetByUserIdAsync(currentUserId);
                if (student == null) return Forbid();

                var studentFees = await _studentFeeManager.GetStudentFeesByStudentIdAsync(student.Id);
                return Ok(studentFees);
            }
            else if (currentUserRole == "Staff")
            {
                // Staff can only see fees for classes they are assigned to
                var staff = await _staffManager.GetByUserIdAsync(currentUserId);
                if (staff == null) return Forbid();

                var staffClasses = await _classManager.GetClassesByStaffIdAsync(staff.Id);
                var assignedClasses = staffClasses.Select(c => c.Id).ToList();

                var studentFees = new List<StudentFee>();
                foreach (var classId in assignedClasses)
                {
                    var classFees = await _studentFeeManager.GetStudentFeesByClassIdAsync(classId);
                    studentFees.AddRange(classFees);
                }

                return Ok(studentFees.Distinct());
            }
            else
            {
                // Admin can see all fees
                var studentFees = await _studentFeeManager.GetAllAsync();
                return Ok(studentFees);
            }
        }

        [HttpGet("report")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetReport(
            [FromQuery] Guid? academicYearId,
            [FromQuery] Guid? classId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var rows = await _studentFeeManager.GetFeeReportAsync(academicYearId, classId, startDate, endDate);
            return Ok(rows);
        }

        [HttpGet("defaulters")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetDefaulters(
            [FromQuery] Guid academicYearId,
            [FromQuery] Guid? classId,
            [FromQuery] int? minDaysOverdue)
        {
            if (academicYearId == Guid.Empty)
                return BadRequest("academicYearId is required.");
            var rows = await _studentFeeManager.GetFeeDefaultersAsync(academicYearId, classId, minDaysOverdue);
            return Ok(rows);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var studentFee = await _studentFeeManager.GetByIdAsync(id);
            if (studentFee == null)
                return NotFound();

            return Ok(studentFee);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetByStudentId(Guid studentId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (currentUserRole == "Student")
            {
                // Students can only see their own fees
                var student = await _studentManager.GetByUserIdAsync(currentUserId);
                if (student == null || student.Id != studentId) return Forbid();
            }
            else if (currentUserRole == "Staff")
            {
                // Staff can only see fees for students in their assigned classes
                var staff = await _staffManager.GetByUserIdAsync(currentUserId);
                if (staff == null) return Forbid();

                var staffClasses = await _classManager.GetClassesByStaffIdAsync(staff.Id);
                var assignedClasses = staffClasses.Select(c => c.Id).ToList();

                var studentEnrollments = await _classEnrollmentManager.GetEnrollmentsByStudentIdAsync(studentId);
                var studentClassEnrollment = studentEnrollments.FirstOrDefault();

                if (studentClassEnrollment == null || !assignedClasses.Contains(studentClassEnrollment.ClassId))
                    return Forbid();
            }

            var studentFees = await _studentFeeManager.GetStudentFeesByStudentIdAsync(studentId);
            return Ok(studentFees);
        }

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByClassId(Guid classId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (currentUserRole == "Staff")
            {
                // Staff can only see fees for their assigned classes
                var staff = await _staffManager.GetByUserIdAsync(currentUserId);
                if (staff == null) return Forbid();

                var staffClasses = await _classManager.GetClassesByStaffIdAsync(staff.Id);
                var assignedClasses = staffClasses.Select(c => c.Id).ToList();

                if (!assignedClasses.Contains(classId)) return Forbid();
            }

            var studentFees = await _studentFeeManager.GetStudentFeesByClassIdAsync(classId);
            return Ok(studentFees);
        }

        [HttpGet("outstanding")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetOutstanding()
        {
            var studentFees = await _studentFeeManager.GetOutstandingFeesAsync();
            return Ok(studentFees);
        }

        [HttpGet("overdue")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetOverdue()
        {
            var studentFees = await _studentFeeManager.GetOverdueFeesAsync();
            return Ok(studentFees);
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var studentFees = await _studentFeeManager.GetFeesByStatusAsync(status);
            return Ok(studentFees);
        }

        [HttpGet("student/{studentId}/structure/{feeStructureId}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetByStudentAndStructure(
            Guid studentId,
            Guid feeStructureId,
            [FromQuery] int? billingYear = null,
            [FromQuery] int? billingMonth = null,
            [FromQuery] Guid? feeStructureLineId = null)
        {
            if (billingYear.HasValue && billingMonth.HasValue)
            {
                var matches = await _studentFeeManager.GetStudentFeeLinesForBillingPeriodAsync(
                    studentId, feeStructureId, billingYear.Value, billingMonth.Value, feeStructureLineId);
                if (feeStructureLineId.HasValue && feeStructureLineId.Value != Guid.Empty)
                {
                    if (matches.Count == 0)
                        return NotFound();
                    return Ok(matches[0]);
                }
                if (matches.Count == 0)
                    return NotFound();
                if (matches.Count > 1)
                {
                    return Conflict(new
                    {
                        message = "Multiple ledger lines exist for this month. Pass feeStructureLineId to select a component line.",
                        studentFeeIds = matches.Select(x => x.Id).ToList()
                    });
                }
                return Ok(matches[0]);
            }

            var all = await _studentFeeManager.GetStudentFeesForStudentAndStructureAsync(studentId, feeStructureId);
            if (all.Count == 0)
                return NotFound();
            if (all.Count == 1)
                return Ok(all[0]);

            return Conflict(new
            {
                message = "Multiple ledger lines exist for this fee head. Pass billingYear and billingMonth (and optionally feeStructureLineId).",
                count = all.Count,
                studentFeeIds = all.Select(x => x.Id).ToList()
            });
        }

        [HttpGet("class/{classId}/outstanding-total")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetTotalOutstandingByClass(Guid classId)
        {
            var total = await _studentFeeManager.GetTotalOutstandingByClassAsync(classId);
            return Ok(new { ClassId = classId, TotalOutstanding = total });
        }

        [HttpGet("class/{classId}/collected-total")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetTotalCollectedByClass(Guid classId)
        {
            var total = await _studentFeeManager.GetTotalCollectedByClassAsync(classId);
            return Ok(new { ClassId = classId, TotalCollected = total });
        }

        /// <summary>
        /// Creates missing monthly StudentFee rows for all students (active enrollments) for the given month,
        /// using each class's active Monthly fee structures. Idempotent per student/structure/month.
        /// </summary>
        [HttpPost("generate-monthly-demands")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GenerateMonthlyDemands([FromBody] GenerateMonthlyDemandsRequest? body)
        {
            if (body == null || body.AcademicYearId == Guid.Empty)
                return BadRequest("AcademicYearId is required.");

            var now = DateTime.UtcNow;
            var y = body.Year ?? now.Year;
            var m = body.Month ?? now.Month;

            try
            {
                var result = await _studentFeeManager.GenerateMissingMonthlyDemandsAsync(body.AcademicYearId, y, m);
                return Ok(result);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Creates instalment StudentFee rows (Source InstallmentPlan) for Yearly/One-time structures with InstallmentCount ≥ 2.
        /// </summary>
        [HttpPost("generate-installment-demands")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GenerateInstallmentDemands([FromBody] GenerateInstallmentDemandsRequest? body)
        {
            if (body == null || body.AcademicYearId == Guid.Empty || body.FeeStructureId == Guid.Empty)
                return BadRequest("AcademicYearId and FeeStructureId are required.");

            try
            {
                var result = await _studentFeeManager.GenerateInstallmentDemandsAsync(body.AcademicYearId, body.FeeStructureId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] StudentFee studentFee)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            studentFee.Id = Guid.NewGuid();
            studentFee.CreatedAt = DateTime.UtcNow;
            studentFee.OutstandingAmount = studentFee.TotalAmount - studentFee.PaidAmount - studentFee.DiscountAmount;

            var createdStudentFee = await _studentFeeManager.CreateAsync(studentFee);
            return CreatedAtAction(nameof(GetById), new { id = createdStudentFee.Id }, createdStudentFee);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] StudentFee studentFee)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingStudentFee = await _studentFeeManager.GetByIdAsync(id);
            if (existingStudentFee == null)
                return NotFound();

            studentFee.Id = id;
            studentFee.CreatedAt = existingStudentFee.CreatedAt;
            studentFee.UpdatedAt = DateTime.UtcNow;

            var updatedStudentFee = await _studentFeeManager.UpdateAsync(studentFee);
            return Ok(updatedStudentFee);
        }

        [HttpPut("{id}/update-outstanding")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateOutstanding(Guid id)
        {
            var success = await _studentFeeManager.UpdateOutstandingAmountAsync(id);
            if (!success)
                return NotFound();

            return Ok(new { Message = "Outstanding amount updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _studentFeeManager.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
