using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.DTOs;
using sms.api.Interfaces;
using System.Security.Claims;

namespace sms.api.Controllers.Academic
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExamResultController : ApiControllerBase
    {
        private readonly IExamResultManager _examResultManager;
        private readonly IStudentManager _studentManager;

        public ExamResultController(IExamResultManager examResultManager, IStudentManager studentManager)
        {
            _examResultManager = examResultManager;
            _studentManager = studentManager;
        }

        [HttpGet("exam/{examId}")]
        public async Task<ActionResult<IEnumerable<ExamResultDTO>>> GetResultsByExam(Guid examId)
        {
            if (User.IsInRole("Admin"))
            {
                var results = await _examResultManager.GetResultsByExamAsync(examId);
                return Ok(results);
            }
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var canAccess = await _examResultManager.CanStaffAccessExamResultsAsync(examId, staffId);
                if (!canAccess)
                    return Forbid();

                var results = await _examResultManager.GetResultsByExamAsync(examId);
                return Ok(results);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<ExamResultDTO>>> GetResultsByStudent(Guid studentId)
        {
            if (User.IsInRole("Admin"))
            {
                var results = await _examResultManager.GetResultsByStudentAsync(studentId);
                return Ok(results);
            }
            else if (User.IsInRole("Student"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                var userId = Guid.Parse(userIdClaim);
                var student = await _studentManager.GetByUserIdAsync(userId);
                if (student == null || student.Id != studentId)
                    return Forbid();

                var results = await _examResultManager.GetResultsByStudentAsync(studentId);
                return Ok(results);
            }
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var results = await _examResultManager.GetResultsByStaffAsync(staffId);
                var studentResults = results.Where(r => r.StudentId == studentId);
                return Ok(studentResults);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpGet("class/{classId}")]
        public async Task<ActionResult<IEnumerable<ExamResultDTO>>> GetResultsByClass(Guid classId)
        {
            if (User.IsInRole("Admin"))
            {
                var results = await _examResultManager.GetResultsByClassAsync(classId);
                return Ok(results);
            }
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var results = await _examResultManager.GetResultsByStaffAsync(staffId);
                var classResults = results.Where(r => r.ExamId != Guid.Empty); // Filter by class through exam
                return Ok(classResults);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExamResultDTO>> GetResult(Guid id)
        {
            var result = await _examResultManager.GetResultDTOByIdAsync(id);
            if (result == null)
                return NotFound();

            if (User.IsInRole("Admin"))
            {
                return Ok(result);
            }
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var canAccess = await _examResultManager.CanStaffAccessResultAsync(id, staffId);
                if (!canAccess)
                    return Forbid();

                return Ok(result);
            }
            else if (User.IsInRole("Student"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                var userId = Guid.Parse(userIdClaim);
                var student = await _studentManager.GetByUserIdAsync(userId);
                if (student == null || student.Id != result.StudentId)
                    return Forbid();

                return Ok(result);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpGet("student/{studentId}/exam/{examId}")]
        public async Task<ActionResult<ExamResultDTO>> GetStudentExamResult(Guid studentId, Guid examId)
        {
            var result = await _examResultManager.GetStudentExamResultAsync(examId, studentId);
            if (result == null)
                return NotFound();

            if (User.IsInRole("Admin"))
            {
                return Ok(result);
            }
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var canAccess = await _examResultManager.CanStaffAccessExamResultsAsync(examId, staffId);
                if (!canAccess)
                    return Forbid();

                return Ok(result);
            }
            else if (User.IsInRole("Student"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                var userId = Guid.Parse(userIdClaim);
                var student = await _studentManager.GetByUserIdAsync(userId);
                if (student == null || student.Id != studentId)
                    return Forbid();

                return Ok(result);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPost]
        public async Task<ActionResult<ExamResultDTO>> CreateResult(CreateExamResultDTO createResultDto)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
                return Forbid();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            // If staff, check if they can access this exam
            if (User.IsInRole("Staff"))
            {
                var staffId = userId;
                var canAccess = await _examResultManager.CanStaffAccessExamResultsAsync(createResultDto.ExamId, staffId);
                if (!canAccess)
                    return Forbid();
            }

            try
            {
                var result = await _examResultManager.CreateResultAsync(createResultDto, userId);
                return CreatedAtAction(nameof(GetResult), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<IEnumerable<ExamResultDTO>>> CreateBulkResults(BulkExamResultDTO bulkResultDto)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
                return Forbid();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            // If staff, check if they can access this exam
            if (User.IsInRole("Staff"))
            {
                var staffId = userId;
                var canAccess = await _examResultManager.CanStaffAccessExamResultsAsync(bulkResultDto.ExamId, staffId);
                if (!canAccess)
                    return Forbid();
            }

            try
            {
                var results = await _examResultManager.CreateBulkResultsAsync(bulkResultDto, userId);
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ExamResultDTO>> UpdateResult(Guid id, UpdateExamResultDTO updateResultDto)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
                return Forbid();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            // If staff, check if they can access this result
            if (User.IsInRole("Staff"))
            {
                var staffId = userId;
                var canAccess = await _examResultManager.CanStaffAccessResultAsync(id, staffId);
                if (!canAccess)
                    return Forbid();
            }

            var result = await _examResultManager.UpdateResultAsync(id, updateResultDto, userId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(Guid id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var success = await _examResultManager.DeleteResultAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("exam/{examId}/summary")]
        public async Task<ActionResult<ExamResultSummaryDTO>> GetExamSummary(Guid examId)
        {
            if (User.IsInRole("Admin"))
            {
                var summary = await _examResultManager.GetExamSummaryAsync(examId);
                if (summary == null)
                    return NotFound();

                return Ok(summary);
            }
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var canAccess = await _examResultManager.CanStaffAccessExamResultsAsync(examId, staffId);
                if (!canAccess)
                    return Forbid();

                var summary = await _examResultManager.GetExamSummaryAsync(examId);
                if (summary == null)
                    return NotFound();

                return Ok(summary);
            }
            else
            {
                return Forbid();
            }
        }
    }
}
