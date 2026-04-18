using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.DTOs;
using sms.api.Interfaces;
using sms.api.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace sms.api.Controllers.Admin
{
    [Authorize]
    public class DepartmentController : ApiControllerBase
    {
        private readonly IDepartmentManager _departmentManager;

        public DepartmentController(IDepartmentManager departmentManager)
        {
            _departmentManager = departmentManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _departmentManager.GetAllAsync();
            var departmentDTOs = departments.Select(d => new DepartmentDTO
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Description = d.Description,
                HeadOfDepartment = d.HeadOfDepartment,
                PhoneNumber = d.PhoneNumber,
                Email = d.Email,
                Location = d.Location,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            });

            return Ok(departmentDTOs);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var departments = await _departmentManager.GetActiveDepartmentsAsync();
            var departmentDTOs = departments.Select(d => new DepartmentDTO
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Description = d.Description,
                HeadOfDepartment = d.HeadOfDepartment,
                PhoneNumber = d.PhoneNumber,
                Email = d.Email,
                Location = d.Location,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            });

            return Ok(departmentDTOs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var department = await _departmentManager.GetByIdAsync(id);
            if (department == null)
                return NotFound();

            var departmentDTO = new DepartmentDTO
            {
                Id = department.Id,
                Name = department.Name,
                Code = department.Code,
                Description = department.Description,
                HeadOfDepartment = department.HeadOfDepartment,
                PhoneNumber = department.PhoneNumber,
                Email = department.Email,
                Location = department.Location,
                IsActive = department.IsActive,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt
            };

            return Ok(departmentDTO);
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var department = await _departmentManager.GetByCodeAsync(code);
            if (department == null)
                return NotFound();

            var departmentDTO = new DepartmentDTO
            {
                Id = department.Id,
                Name = department.Name,
                Code = department.Code,
                Description = department.Description,
                HeadOfDepartment = department.HeadOfDepartment,
                PhoneNumber = department.PhoneNumber,
                Email = department.Email,
                Location = department.Location,
                IsActive = department.IsActive,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt
            };

            return Ok(departmentDTO);
        }

        [HttpGet("search/{name}")]
        public async Task<IActionResult> SearchByName(string name)
        {
            var departments = await _departmentManager.SearchByNameAsync(name);
            var departmentDTOs = departments.Select(d => new DepartmentDTO
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Description = d.Description,
                HeadOfDepartment = d.HeadOfDepartment,
                PhoneNumber = d.PhoneNumber,
                Email = d.Email,
                Location = d.Location,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            });

            return Ok(departmentDTOs);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDTO createDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if code is unique
            if (!await _departmentManager.IsCodeUniqueAsync(createDTO.Code))
                return BadRequest("Department code already exists");

            var department = new Department
            {
                Name = createDTO.Name,
                Code = createDTO.Code,
                Description = createDTO.Description,
                HeadOfDepartment = createDTO.HeadOfDepartment,
                PhoneNumber = createDTO.PhoneNumber,
                Email = createDTO.Email,
                Location = createDTO.Location,
                IsActive = true
            };

            var createdDepartment = await _departmentManager.CreateAsync(department);

            var departmentDTO = new DepartmentDTO
            {
                Id = createdDepartment.Id,
                Name = createdDepartment.Name,
                Code = createdDepartment.Code,
                Description = createdDepartment.Description,
                HeadOfDepartment = createdDepartment.HeadOfDepartment,
                PhoneNumber = createdDepartment.PhoneNumber,
                Email = createdDepartment.Email,
                Location = createdDepartment.Location,
                IsActive = createdDepartment.IsActive,
                CreatedAt = createdDepartment.CreatedAt,
                UpdatedAt = createdDepartment.UpdatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = departmentDTO.Id }, departmentDTO);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDTO updateDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingDepartment = await _departmentManager.GetByIdAsync(id);
            if (existingDepartment == null)
                return NotFound();

            // Check if code is unique (excluding current department)
            if (!await _departmentManager.IsCodeUniqueAsync(updateDTO.Code, id))
                return BadRequest("Department code already exists");

            existingDepartment.Name = updateDTO.Name;
            existingDepartment.Code = updateDTO.Code;
            existingDepartment.Description = updateDTO.Description;
            existingDepartment.HeadOfDepartment = updateDTO.HeadOfDepartment;
            existingDepartment.PhoneNumber = updateDTO.PhoneNumber;
            existingDepartment.Email = updateDTO.Email;
            existingDepartment.Location = updateDTO.Location;
            existingDepartment.IsActive = updateDTO.IsActive;

            var updatedDepartment = await _departmentManager.UpdateAsync(existingDepartment);

            var departmentDTO = new DepartmentDTO
            {
                Id = updatedDepartment.Id,
                Name = updatedDepartment.Name,
                Code = updatedDepartment.Code,
                Description = updatedDepartment.Description,
                HeadOfDepartment = updatedDepartment.HeadOfDepartment,
                PhoneNumber = updatedDepartment.PhoneNumber,
                Email = updatedDepartment.Email,
                Location = updatedDepartment.Location,
                IsActive = updatedDepartment.IsActive,
                CreatedAt = updatedDepartment.CreatedAt,
                UpdatedAt = updatedDepartment.UpdatedAt
            };

            return Ok(departmentDTO);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var department = await _departmentManager.GetByIdAsync(id);
            if (department == null)
                return NotFound();

            // Check if department has any associated records
            if (department.Designations.Any() || department.Staff.Any() ||
                department.Subjects.Any() || department.Courses.Any())
            {
                return BadRequest("Cannot delete department with associated records. Please remove all designations, staff, subjects, and courses first.");
            }

            await _departmentManager.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("{id}/designations")]
        public async Task<IActionResult> GetDepartmentDesignations(Guid id)
        {
            var designations = await _departmentManager.GetDepartmentDesignationsAsync(id);
            return Ok(designations);
        }

        [HttpGet("{id}/staff")]
        public async Task<IActionResult> GetDepartmentStaff(Guid id)
        {
            var staff = await _departmentManager.GetDepartmentStaffAsync(id);
            return Ok(staff);
        }



        [HttpGet("{id}/courses")]
        public async Task<IActionResult> GetDepartmentCourses(Guid id)
        {
            var courses = await _departmentManager.GetDepartmentCoursesAsync(id);
            return Ok(courses);
        }
    }
}
