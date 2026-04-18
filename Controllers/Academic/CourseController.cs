using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;

namespace sms.api.Controllers.Academic
{
    [Authorize]
    public class CourseController : ApiControllerBase
    {
        private readonly ICourseManager _courseManager;

        public CourseController(ICourseManager courseManager)
        {
            _courseManager = courseManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _courseManager.GetAllAsync();
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var course = await _courseManager.GetByIdAsync(id);
            if (course == null)
                return NotFound();

            return Ok(course);
        }

        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetByDepartment(Guid departmentId)
        {
            var courses = await _courseManager.GetCoursesByDepartmentAsync(departmentId);
            return Ok(courses);
        }

        [HttpGet("{id}/students")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetStudents(Guid id)
        {
            var students = await _courseManager.GetStudentsByCourseIdAsync(id);
            return Ok(students);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Course course)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            course.Id = Guid.NewGuid();
            var createdCourse = await _courseManager.CreateAsync(course);
            return CreatedAtAction(nameof(GetById), new { id = createdCourse.Id }, createdCourse);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Course course)
        {
            if (id != course.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCourse = await _courseManager.GetByIdAsync(id);
            if (existingCourse == null)
                return NotFound();

            // If staff, only allow updating courses assigned to them
            if (User.IsInRole("Staff") && existingCourse.StaffId.ToString() != User.FindFirst("nameid")?.Value)
                return Forbid();

            var updatedCourse = await _courseManager.UpdateAsync(course);
            return Ok(updatedCourse);
        }

        [HttpPost("{id}/assign-staff/{staffId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignStaff(Guid id, Guid staffId)
        {
            var result = await _courseManager.AssignStaffToCourseAsync(id, staffId);
            if (!result)
                return BadRequest(new { message = "Failed to assign staff to course" });

            return Ok(new { message = "Staff assigned to course successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _courseManager.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
