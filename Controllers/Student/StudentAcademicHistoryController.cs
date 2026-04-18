using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.DTOs;
using sms.api.Interfaces;

namespace sms.api.Controllers.Student
{
    [Authorize]
    public class StudentAcademicHistoryController : ApiControllerBase
    {
        private readonly IStudentAcademicHistoryManager _academicHistoryManager;

        public StudentAcademicHistoryController(IStudentAcademicHistoryManager academicHistoryManager)
        {
            _academicHistoryManager = academicHistoryManager;
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentAcademicHistory(Guid studentId)
        {
            try
            {
                var history = await _academicHistoryManager.GetStudentAcademicHistoryAsync(studentId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving student academic history", error = ex.Message });
            }
        }

        [HttpGet("student/{studentId}/year/{academicYearId}")]
        public async Task<IActionResult> GetStudentAcademicHistoryByYear(Guid studentId, Guid academicYearId)
        {
            try
            {
                var history = await _academicHistoryManager.GetStudentAcademicHistoryByYearAsync(studentId, academicYearId);
                if (history == null)
                    return NotFound(new { message = "Academic history not found for the specified student and year" });

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving student academic history", error = ex.Message });
            }
        }

        [HttpGet("class/{classId}/year/{academicYearId}")]
        public async Task<IActionResult> GetClassAcademicHistory(Guid classId, Guid academicYearId)
        {
            try
            {
                var history = await _academicHistoryManager.GetClassAcademicHistoryAsync(classId, academicYearId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving class academic history", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateStudentAcademicHistory([FromBody] CreateStudentAcademicHistoryDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var history = await _academicHistoryManager.CreateStudentAcademicHistoryAsync(createDto);
                return CreatedAtAction(nameof(GetStudentAcademicHistoryByYear), 
                    new { studentId = createDto.StudentId, academicYearId = createDto.AcademicYearId }, history);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating academic history", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStudentAcademicHistory(Guid id, [FromBody] UpdateStudentAcademicHistoryDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var history = await _academicHistoryManager.UpdateStudentAcademicHistoryAsync(id, updateDto);
                if (history == null)
                    return NotFound(new { message = "Academic history not found" });

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating academic history", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudentAcademicHistory(Guid id)
        {
            try
            {
                var result = await _academicHistoryManager.DeleteStudentAcademicHistoryAsync(id);
                if (!result)
                    return BadRequest(new { message = "Cannot delete academic history. It may have associated data." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting academic history", error = ex.Message });
            }
        }

        [HttpPut("student/{studentId}/year/{academicYearId}/performance")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateAcademicPerformance(Guid studentId, Guid academicYearId, [FromBody] AcademicPerformanceUpdateDTO performanceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _academicHistoryManager.UpdateAcademicPerformanceAsync(studentId, academicYearId, performanceDto);
                if (!result)
                    return NotFound(new { message = "Academic history not found" });

                return Ok(new { message = "Academic performance updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating academic performance", error = ex.Message });
            }
        }

        [HttpPut("student/{studentId}/year/{academicYearId}/attendance")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateAttendanceSummary(Guid studentId, Guid academicYearId, [FromBody] AttendanceSummaryDTO attendanceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _academicHistoryManager.UpdateAttendanceSummaryAsync(studentId, academicYearId, attendanceDto);
                if (!result)
                    return NotFound(new { message = "Academic history not found" });

                return Ok(new { message = "Attendance summary updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating attendance summary", error = ex.Message });
            }
        }

        [HttpPut("student/{studentId}/year/{academicYearId}/fees")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateFeeSummary(Guid studentId, Guid academicYearId, [FromBody] FeeSummaryDTO feeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _academicHistoryManager.UpdateFeeSummaryAsync(studentId, academicYearId, feeDto);
                if (!result)
                    return NotFound(new { message = "Academic history not found" });

                return Ok(new { message = "Fee summary updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating fee summary", error = ex.Message });
            }
        }

        [HttpPost("student/{studentId}/year/{academicYearId}/promotion")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ProcessStudentPromotion(Guid studentId, Guid academicYearId, [FromBody] StudentPromotionDTO promotionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _academicHistoryManager.ProcessStudentPromotionAsync(studentId, academicYearId, promotionDto);
                if (!result)
                    return NotFound(new { message = "Academic history not found" });

                return Ok(new { message = "Student promotion processed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing student promotion", error = ex.Message });
            }
        }

        [HttpPut("student/{studentId}/year/{academicYearId}/promotion-status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdatePromotionStatus(Guid studentId, Guid academicYearId, [FromBody] UpdatePromotionStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _academicHistoryManager.UpdatePromotionStatusAsync(studentId, academicYearId, request.PromotionStatus, request.Remarks);
                if (!result)
                    return NotFound(new { message = "Academic history not found" });

                return Ok(new { message = "Promotion status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating promotion status", error = ex.Message });
            }
        }

        [HttpGet("class/{classId}/year/{academicYearId}/promotion")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetStudentsForPromotion(Guid classId, Guid academicYearId)
        {
            try
            {
                var students = await _academicHistoryManager.GetStudentsForPromotionAsync(classId, academicYearId);
                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving students for promotion", error = ex.Message });
            }
        }

        [HttpGet("year/{academicYearId}/promoted")]
        public async Task<IActionResult> GetPromotedStudents(Guid academicYearId)
        {
            try
            {
                var students = await _academicHistoryManager.GetPromotedStudentsAsync(academicYearId);
                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving promoted students", error = ex.Message });
            }
        }

        [HttpGet("year/{academicYearId}/detained")]
        public async Task<IActionResult> GetDetainedStudents(Guid academicYearId)
        {
            try
            {
                var students = await _academicHistoryManager.GetDetainedStudentsAsync(academicYearId);
                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving detained students", error = ex.Message });
            }
        }

        [HttpGet("class/{classId}/year/{academicYearId}/performance-report")]
        public async Task<IActionResult> GetClassPerformanceReport(Guid classId, Guid academicYearId)
        {
            try
            {
                var report = await _academicHistoryManager.GetClassPerformanceReportAsync(classId, academicYearId);
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating class performance report", error = ex.Message });
            }
        }

        [HttpGet("student/{studentId}/progress-report")]
        public async Task<IActionResult> GetStudentProgressReport(Guid studentId)
        {
            try
            {
                var report = await _academicHistoryManager.GetStudentProgressReportAsync(studentId);
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating student progress report", error = ex.Message });
            }
        }

        [HttpGet("validate-history")]
        public async Task<IActionResult> ValidateAcademicHistory([FromQuery] Guid studentId, [FromQuery] Guid academicYearId)
        {
            try
            {
                var hasHistory = await _academicHistoryManager.HasAcademicHistoryAsync(studentId, academicYearId);
                var canCreate = await _academicHistoryManager.CanCreateAcademicHistoryAsync(studentId, academicYearId);
                
                return Ok(new { hasHistory, canCreate });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error validating academic history", error = ex.Message });
            }
        }
    }

    public class UpdatePromotionStatusRequest
    {
        public string PromotionStatus { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }
}
