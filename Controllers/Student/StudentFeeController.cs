using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.DTOs;
using sms.api.Interfaces;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Student
{
    [Route("api/fees")]
    [Authorize]
    public class StudentFeeController : ApiControllerBase
    {
        private readonly IStudentFeeManager _studentFeeManager;
        private readonly IStudentManager _studentManager;

        public StudentFeeController(
            IStudentFeeManager studentFeeManager,
            IStudentManager studentManager)
        {
            _studentFeeManager = studentFeeManager;
            _studentManager = studentManager;
        }

        private static bool FeeInAllowedClasses(StudentFee fee, IReadOnlyCollection<Guid> allowed)
        {
            if (fee.FeeStructure == null) return false;
            if (fee.FeeStructure.ClassId == null) return true; // common fee, applies to every class
            return allowed.Contains(fee.FeeStructure.ClassId.Value);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetAll()
        {
            if (User.IsInRole("Student"))
            {
                var student = await _studentManager.GetByUserIdAsync(GetCurrentUserId());
                if (student == null) return Forbid();

                var studentFees = await _studentFeeManager.GetStudentFeesByStudentIdAsync(student.Id);
                return Ok(studentFees);
            }

            var scope = await GetDataScopeAsync();
            if (scope.IsUnrestricted)
            {
                var all = await _studentFeeManager.GetAllAsync();
                return Ok(all);
            }

            var allowed = scope.AllowedClassIds?.ToList() ?? new List<Guid>();
            var scopedFees = new List<StudentFee>();
            foreach (var classId in allowed)
            {
                var classFees = await _studentFeeManager.GetStudentFeesByClassIdAsync(classId);
                scopedFees.AddRange(classFees);
            }
            return Ok(scopedFees.DistinctBy(f => f.Id));
        }

        [HttpGet("report")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetReport(
            [FromQuery] Guid? academicYearId,
            [FromQuery] Guid? classId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted)
            {
                if (classId.HasValue && classId.Value != Guid.Empty)
                {
                    if (!scope.CanAccessClass(classId.Value))
                        return Forbid();
                }
                else
                {
                    var allowed = scope.AllowedClassIds?.ToList() ?? new List<Guid>();
                    if (allowed.Count == 0)
                        return Ok(Array.Empty<FeeReportRowDto>());

                    var merged = new List<FeeReportRowDto>();
                    foreach (var cid in allowed)
                    {
                        var part = await _studentFeeManager.GetFeeReportAsync(academicYearId, cid, startDate, endDate);
                        merged.AddRange(part);
                    }
                    return Ok(merged);
                }
            }

            var rows = await _studentFeeManager.GetFeeReportAsync(academicYearId, classId, startDate, endDate);
            return Ok(rows);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? classId = null,
            [FromQuery] string? status = null,
            [FromQuery] Guid? academicYearId = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var yearId = academicYearId;
            if (!yearId.HasValue || yearId.Value == Guid.Empty)
                yearId = GetCurrentAcademicYearId();

            var scope = await GetDataScopeAsync();
            IEnumerable<Guid>? allowedClassIds = DataScopeHelper.AllowedClassIdsForQuery(scope);

            if (!scope.IsUnrestricted && classId.HasValue && classId.Value != Guid.Empty
                && !scope.CanAccessClass(classId.Value))
                return Forbid();

            var result = await _studentFeeManager.GetStudentFeesPagedAsync(
                pageNumber, pageSize, searchTerm, classId, status, yearId, allowedClassIds);

            var totalCount = result.TotalCount;
            var totalPages = result.TotalPages;
            var firstItemIndex = totalCount == 0 ? 0 : (pageNumber - 1) * pageSize + 1;
            var lastItemIndex = Math.Min(pageNumber * pageSize, totalCount);

            return Ok(new
            {
                items = result.Items,
                totalCount,
                pageNumber,
                pageSize,
                totalPages,
                hasPreviousPage = pageNumber > 1,
                hasNextPage = pageNumber < totalPages,
                firstItemIndex,
                lastItemIndex
            });
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

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted)
            {
                if (classId.HasValue && classId.Value != Guid.Empty)
                {
                    if (!scope.CanAccessClass(classId.Value))
                        return Forbid();
                }
                else
                {
                    var allowed = scope.AllowedClassIds?.ToList() ?? new List<Guid>();
                    if (allowed.Count == 0)
                        return Ok(Array.Empty<FeeDefaulterRowDto>());

                    var merged = new List<FeeDefaulterRowDto>();
                    foreach (var cid in allowed)
                    {
                        var part = await _studentFeeManager.GetFeeDefaultersAsync(academicYearId, cid, minDaysOverdue);
                        merged.AddRange(part);
                    }
                    return Ok(merged);
                }
            }

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

            if (User.IsInRole("Student"))
            {
                var student = await _studentManager.GetByUserIdAsync(GetCurrentUserId());
                if (student == null || student.Id != studentFee.StudentId)
                    return Forbid();
            }
            else
            {
                var scope = await GetDataScopeAsync();
                if (!scope.IsUnrestricted && !scope.CanAccessStudent(studentFee.StudentId))
                    return Forbid();
            }

            return Ok(studentFee);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetByStudentId(Guid studentId)
        {
            if (User.IsInRole("Student"))
            {
                var student = await _studentManager.GetByUserIdAsync(GetCurrentUserId());
                if (student == null || student.Id != studentId) return Forbid();
            }
            else
            {
                var scope = await GetDataScopeAsync();
                if (!scope.IsUnrestricted && !scope.CanAccessStudent(studentId))
                    return Forbid();
            }

            var studentFees = await _studentFeeManager.GetStudentFeesByStudentIdAsync(studentId);
            return Ok(studentFees);
        }

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByClassId(Guid classId)
        {
            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessClass(classId))
                return Forbid();

            var studentFees = await _studentFeeManager.GetStudentFeesByClassIdAsync(classId);
            return Ok(studentFees);
        }

        [HttpGet("outstanding")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetOutstanding()
        {
            var scope = await GetDataScopeAsync();
            var studentFees = await _studentFeeManager.GetOutstandingFeesAsync();
            if (!scope.IsUnrestricted)
            {
                var allowed = scope.AllowedClassIds?.ToHashSet() ?? new HashSet<Guid>();
                studentFees = studentFees.Where(f => FeeInAllowedClasses(f, allowed));
            }
            return Ok(studentFees);
        }

        [HttpGet("overdue")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetOverdue()
        {
            var scope = await GetDataScopeAsync();
            var studentFees = await _studentFeeManager.GetOverdueFeesAsync();
            if (!scope.IsUnrestricted)
            {
                var allowed = scope.AllowedClassIds?.ToHashSet() ?? new HashSet<Guid>();
                studentFees = studentFees.Where(f => FeeInAllowedClasses(f, allowed));
            }
            return Ok(studentFees);
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var scope = await GetDataScopeAsync();
            var studentFees = await _studentFeeManager.GetFeesByStatusAsync(status);
            if (!scope.IsUnrestricted)
            {
                var allowed = scope.AllowedClassIds?.ToHashSet() ?? new HashSet<Guid>();
                studentFees = studentFees.Where(f => FeeInAllowedClasses(f, allowed));
            }
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
            if (User.IsInRole("Student"))
            {
                var student = await _studentManager.GetByUserIdAsync(GetCurrentUserId());
                if (student == null || student.Id != studentId) return Forbid();
            }
            else
            {
                var scope = await GetDataScopeAsync();
                if (!scope.IsUnrestricted && !scope.CanAccessStudent(studentId))
                    return Forbid();
            }

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
            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessClass(classId))
                return Forbid();

            var total = await _studentFeeManager.GetTotalOutstandingByClassAsync(classId);
            return Ok(new { ClassId = classId, TotalOutstanding = total });
        }

        [HttpGet("class/{classId}/collected-total")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetTotalCollectedByClass(Guid classId)
        {
            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessClass(classId))
                return Forbid();

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

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessStudent(studentFee.StudentId))
                return Forbid();

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

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessStudent(existingStudentFee.StudentId))
                return Forbid();

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
            var existing = await _studentFeeManager.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessStudent(existing.StudentId))
                return Forbid();

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
