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
    public class ExamController : ApiControllerBase
    {
        private readonly IExamManager _examManager;

        public ExamController(IExamManager examManager)
        {
            _examManager = examManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamDTO>>> GetExams()
        {
            if (User.IsInRole("Admin"))
            {
                var allExams = await _examManager.GetAllAsync();
                var examDtos = new List<ExamDTO>();
                foreach (var exam in allExams)
                {
                    var dto = await _examManager.GetExamDTOByIdAsync(exam.Id);
                    if (dto != null) examDtos.Add(dto);
                }
                return Ok(examDtos);
            }
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var exams = await _examManager.GetExamsByStaffAsync(staffId);
                return Ok(exams);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExamDTO>> GetExam(Guid id)
        {
            var exam = await _examManager.GetExamDTOByIdAsync(id);
            if (exam == null)
                return NotFound();

            // Check access permissions
            if (User.IsInRole("Admin"))
            {
                return Ok(exam);
            }
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var canAccess = await _examManager.CanStaffAccessExamAsync(id, staffId);
                if (!canAccess)
                    return Forbid();

                return Ok(exam);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpGet("class/{classId}")]
        public async Task<ActionResult<IEnumerable<ExamDTO>>> GetExamsByClass(Guid classId)
        {
            if (User.IsInRole("Admin"))
            {
                var exams = await _examManager.GetExamsByClassAsync(classId);
                return Ok(exams);
            }
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var exams = await _examManager.GetExamsByStaffAsync(staffId);
                var classExams = exams.Where(e => e.ClassId == classId);
                return Ok(classExams);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpGet("subject/{subjectId}")]
        public async Task<ActionResult<IEnumerable<ExamDTO>>> GetExamsBySubject(Guid subjectId)
        {
            if (User.IsInRole("Admin"))
            {
                var exams = await _examManager.GetExamsBySubjectAsync(subjectId);
                return Ok(exams);
            }
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var exams = await _examManager.GetExamsByStaffAsync(staffId);
                var subjectExams = exams.Where(e => e.SubjectId == subjectId);
                return Ok(subjectExams);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<ExamDTO>>> GetUpcomingExams()
        {
            if (User.IsInRole("Admin"))
            {
                var exams = await _examManager.GetUpcomingExamsAsync();
                return Ok(exams);
            }
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var allExams = await _examManager.GetExamsByStaffAsync(staffId);
                var upcomingExams = allExams.Where(e => e.IsUpcoming);
                return Ok(upcomingExams);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPost]
        public async Task<ActionResult<ExamDTO>> CreateExam(CreateExamDTO createExamDto)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);
            var exam = await _examManager.CreateExamAsync(createExamDto, userId);
            return CreatedAtAction(nameof(GetExam), new { id = exam.Id }, exam);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ExamDTO>> UpdateExam(Guid id, UpdateExamDTO updateExamDto)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var exam = await _examManager.UpdateExamAsync(id, updateExamDto);
            if (exam == null)
                return NotFound();

            return Ok(exam);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExam(Guid id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var success = await _examManager.DeleteExamAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("academic-year/{academicYear}")]
        public async Task<ActionResult<IEnumerable<ExamDTO>>> GetExamsByAcademicYear(string academicYear)
        {
            if (User.IsInRole("Admin"))
            {
                var exams = await _examManager.GetExamsByAcademicYearAsync(academicYear);
                return Ok(exams);
            }
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var allExams = await _examManager.GetExamsByStaffAsync(staffId);
                var yearExams = allExams.Where(e => e.AcademicYear == academicYear);
                return Ok(yearExams);
            }
            else
            {
                return Forbid();
            }
        }
    }
}
