using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Controllers.Academic
{
    [Route("api/subjects")]
    [Authorize]
    public class SubjectController : ApiControllerBase
    {
        private readonly ISubjectManager _subjectManager;

        public SubjectController(ISubjectManager subjectManager)
        {
            _subjectManager = subjectManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<Subject> subjects;

            // If user is Admin, return all subjects
            if (User.IsInRole("Admin"))
            {
                subjects = await _subjectManager.GetAllAsync();
            }
            // If user is Staff, return only subjects assigned to them
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                subjects = await _subjectManager.GetSubjectsByStaffIdAsync(staffId);
            }
            // If user is Student, they shouldn't access this endpoint directly
            else
            {
                return Forbid();
            }

            // Map the subjects data to include staff user properties
            var mappedSubjects = subjects.Select(s => new
            {
                s.Id,
                s.Name,
                s.Code,
                s.Description,
                s.StaffId,
                s.CreatedAt,
                s.UpdatedAt,
                s.IsActive,
                Staff = s.Staff != null ? new
                {
                    s.Staff.Id,
                    s.Staff.Designation,
                    FirstName = s.Staff.User?.FirstName,
                    LastName = s.Staff.User?.LastName,
                    FullName = $"{s.Staff.User?.FirstName} {s.Staff.User?.LastName}"
                } : null
            });

            return Ok(mappedSubjects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var subject = await _subjectManager.GetByIdAsync(id);
            if (subject == null)
                return NotFound();

            // If user is Admin, allow access to any subject
            if (User.IsInRole("Admin"))
            {
                // Continue with normal flow
            }
            // If user is Staff, only allow access to subjects assigned to them
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);

                // Check if this subject is assigned to the staff member
                if (subject.StaffId != staffId)
                    return Forbid();
            }
            // If user is Student, they shouldn't access this endpoint directly
            else
            {
                return Forbid();
            }

            // Map the subject data to include staff user properties
            var mappedSubject = new
            {
                subject.Id,
                subject.Name,
                subject.Code,
                subject.Description,
                subject.StaffId,
                subject.CreatedAt,
                subject.UpdatedAt,
                subject.IsActive,
                Staff = subject.Staff != null ? new
                {
                    subject.Staff.Id,
                    subject.Staff.Designation,
                    FirstName = subject.Staff.User?.FirstName,
                    LastName = subject.Staff.User?.LastName,
                    FullName = $"{subject.Staff.User?.FirstName} {subject.Staff.User?.LastName}"
                } : null
            };

            return Ok(mappedSubject);
        }



        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetByStaffId(Guid staffId)
        {
            var subjects = await _subjectManager.GetSubjectsByStaffIdAsync(staffId);

            // Map the subjects data to include staff user properties
            var mappedSubjects = subjects.Select(s => new
            {
                s.Id,
                s.Name,
                s.Code,
                s.Description,
                s.StaffId,
                s.CreatedAt,
                s.UpdatedAt,
                s.IsActive,
                Staff = s.Staff != null ? new
                {
                    s.Staff.Id,
                    s.Staff.Designation,
                    FirstName = s.Staff.User?.FirstName,
                    LastName = s.Staff.User?.LastName,
                    FullName = $"{s.Staff.User?.FirstName} {s.Staff.User?.LastName}"
                } : null
            });

            return Ok(mappedSubjects);
        }

        [HttpGet("class/{classId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByClassId(Guid classId)
        {
            try
            {
                // Get class manager to fetch class subjects
                var classManager = HttpContext.RequestServices.GetService(typeof(IClassManager)) as IClassManager;
                if (classManager == null)
                    return StatusCode(500, "Service unavailable");

                var classEntity = await classManager.GetClassByIdAsync(classId);
                if (classEntity == null)
                    return NotFound("Class not found");

                // Return the subjects directly from the class entity
                // The ClassManager already maps them properly with IsCompulsory status
                return Ok(classEntity.Subjects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving class subjects", error = ex.Message });
            }
        }



        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSubjectDTO subjectDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = subjectDto.Name,
                Code = subjectDto.Code,
                Description = subjectDto.Description,
                StaffId = subjectDto.StaffId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdSubject = await _subjectManager.CreateAsync(subject);
            return CreatedAtAction(nameof(GetById), new { id = createdSubject.Id }, createdSubject);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubjectDTO subjectDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subject = await _subjectManager.GetByIdAsync(id);
            if (subject == null)
                return NotFound();

            // If user is Admin, allow updating any subject
            if (User.IsInRole("Admin"))
            {
                subject.Name = subjectDto.Name;
                subject.Code = subjectDto.Code;
                subject.Description = subjectDto.Description;
                subject.StaffId = subjectDto.StaffId;
                subject.IsActive = subjectDto.IsActive;
                subject.UpdatedAt = DateTime.UtcNow;
            }
            // If user is Staff, only allow updating subjects assigned to them
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);

                // Check if this subject is assigned to the staff member
                if (subject.StaffId != staffId)
                    return Forbid();

                // Staff can only update certain fields, not reassign the subject
                subject.Name = subjectDto.Name;
                subject.Code = subjectDto.Code;
                subject.Description = subjectDto.Description;
                // Preserve original values for these fields
                subject.StaffId = subject.StaffId;
                subject.IsActive = subjectDto.IsActive;
                subject.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                return Forbid();
            }

            var updatedSubject = await _subjectManager.UpdateAsync(subject);
            return Ok(updatedSubject);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _subjectManager.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
