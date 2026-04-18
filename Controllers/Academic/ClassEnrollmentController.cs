using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;

namespace sms.api.Controllers.Academic
{
    [Authorize]
    public class ClassEnrollmentController : ApiControllerBase
    {
        private readonly IClassEnrollmentManager _classEnrollmentManager;

        public ClassEnrollmentController(IClassEnrollmentManager classEnrollmentManager)
        {
            _classEnrollmentManager = classEnrollmentManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var enrollments = await _classEnrollmentManager.GetAllAsync();
            return Ok(enrollments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var enrollment = await _classEnrollmentManager.GetByIdAsync(id);
            if (enrollment == null)
                return NotFound();

            // If student, only allow access to own enrollments
            if (User.IsInRole("Student") && enrollment.StudentId.ToString() != User.FindFirst("nameid")?.Value)
                return Forbid();

            return Ok(enrollment);
        }

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByClass(Guid classId)
        {
            var enrollments = await _classEnrollmentManager.GetEnrollmentsByClassIdAsync(classId);
            return Ok(enrollments);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(Guid studentId)
        {
            // If student, only allow access to own enrollments
            if (User.IsInRole("Student") && studentId.ToString() != User.FindFirst("nameid")?.Value)
                return Forbid();

            var enrollments = await _classEnrollmentManager.GetEnrollmentsByStudentIdAsync(studentId);
            return Ok(enrollments);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] ClassEnrollment enrollment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            enrollment.Id = Guid.NewGuid();
            var createdEnrollment = await _classEnrollmentManager.CreateAsync(enrollment);
            return CreatedAtAction(nameof(GetById), new { id = createdEnrollment.Id }, createdEnrollment);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateEnrollmentStatusRequest request)
        {
            var result = await _classEnrollmentManager.UpdateClassEnrollmentAsync(id, request.IsActive);
            if (!result)
                return NotFound();

            return Ok(new { message = "Enrollment status updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _classEnrollmentManager.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }

    public class UpdateEnrollmentStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
