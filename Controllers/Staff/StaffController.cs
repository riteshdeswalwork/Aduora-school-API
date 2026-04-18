using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Controllers.Staff
{
    [Route("api/staff")]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffController : ApiControllerBase
    {
        private readonly IStaffManager _staffManager;
        private readonly IClassManager _classManager;

        public StaffController(IStaffManager staffManager, IClassManager classManager)
        {
            _staffManager = staffManager;
            _classManager = classManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] bool timetableTeachers = false)
        {
            var staffList = await _staffManager.GetAllAsync();

            if (timetableTeachers)
            {
                staffList = staffList.Where(s =>
                    s.User != null &&
                    s.User.IsActive &&
                    string.Equals(s.User.Role, "Teacher", StringComparison.OrdinalIgnoreCase));
            }

            // Map the staff data to match the frontend model structure
            var mappedStaff = staffList.Select(s => new
            {
                s.Id,
                s.UserId,
                s.DesignationId,
                s.EmployeeId,
                DateOfJoining = s.DateOfJoining,
                s.Address,
                s.PhoneNumber,
                s.Qualification,
                s.Salary,
                s.CreatedAt,
                s.UpdatedAt,
                User = new
                {
                    s.User.Id,
                    s.User.FirstName,
                    s.User.LastName,
                    s.User.Email,
                    s.User.Username,
                    s.User.Role,
                    s.User.IsActive,
                    s.User.ProfilePicture,
                    FullName = $"{s.User.FirstName} {s.User.LastName}"
                },
                Designation = s.Designation != null ? new
                {
                    s.Designation.Id,
                    s.Designation.Name,
                    s.Designation.Description,
                    DepartmentName = s.Designation.Department != null ? s.Designation.Department.Name : "Not assigned"
                } : null
            });

            return Ok(mappedStaff);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var (items, totalCount) = await _staffManager.GetPagedAsync(pageNumber, pageSize, searchTerm);
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
            var firstItemIndex = totalCount == 0 ? 0 : (pageNumber - 1) * pageSize + 1;
            var lastItemIndex = Math.Min(pageNumber * pageSize, totalCount);

            var mappedItems = items.Select(s => new
            {
                s.Id,
                s.UserId,
                s.DesignationId,
                s.EmployeeId,
                DateOfJoining = s.DateOfJoining,
                s.Address,
                s.PhoneNumber,
                s.Qualification,
                s.Salary,
                s.CreatedAt,
                s.UpdatedAt,
                User = new
                {
                    s.User.Id,
                    s.User.FirstName,
                    s.User.LastName,
                    s.User.Email,
                    s.User.Username,
                    s.User.Role,
                    s.User.IsActive,
                    s.User.ProfilePicture,
                    FullName = $"{s.User.FirstName} {s.User.LastName}"
                },
                Designation = s.Designation != null ? new
                {
                    s.Designation.Id,
                    s.Designation.Name,
                    s.Designation.Description,
                    DepartmentName = s.Designation.Department != null ? s.Designation.Department.Name : "Not assigned"
                } : null
            });

            return Ok(new
            {
                items = mappedItems,
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var staff = await _staffManager.GetByIdAsync(id);
            if (staff == null)
                return NotFound();

            // If not admin, only allow access to own profile
            if (User.IsInRole("Staff") && id.ToString() != User.FindFirst("nameid")?.Value)
                return Forbid();

            // Map the staff data to match the frontend model structure
            var mappedStaff = new
            {
                staff.Id,
                staff.UserId,
                staff.DesignationId,
                staff.EmployeeId,
                DateOfJoining = staff.DateOfJoining,
                staff.Address,
                staff.PhoneNumber,
                staff.Qualification,
                staff.Salary,
                staff.CreatedAt,
                staff.UpdatedAt,
                User = new
                {
                    staff.User.Id,
                    staff.User.FirstName,
                    staff.User.LastName,
                    staff.User.Email,
                    staff.User.Username,
                    staff.User.Role,
                    staff.User.IsActive,
                    staff.User.ProfilePicture,
                    FullName = $"{staff.User.FirstName} {staff.User.LastName}"
                },
                Designation = staff.Designation != null ? new
                {
                    staff.Designation.Id,
                    staff.Designation.Name,
                    staff.Designation.Description,
                    DepartmentName = staff.Designation.Department != null ? staff.Designation.Department.Name : "Not assigned",
                    Department = staff.Designation.Department != null ? new
                    {
                        staff.Designation.Department.Id,
                        staff.Designation.Department.Name,
                        staff.Designation.Department.Code
                    } : null
                } : null
            };

            return Ok(mappedStaff);
        }

        [HttpGet("department/{departmentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByDepartment(Guid departmentId)
        {
            var staffList = await _staffManager.GetStaffByDepartmentAsync(departmentId);

            // Map the staff data to match the frontend model structure
            var mappedStaff = staffList.Select(s => new
            {
                s.Id,
                s.UserId,
                s.DesignationId,
                s.EmployeeId,
                DateOfJoining = s.DateOfJoining,
                s.Address,
                s.PhoneNumber,
                s.Qualification,
                s.Salary,
                s.CreatedAt,
                s.UpdatedAt,
                User = new
                {
                    s.User.Id,
                    s.User.FirstName,
                    s.User.LastName,
                    s.User.Email,
                    s.User.Username,
                    s.User.Role,
                    s.User.IsActive,
                    s.User.ProfilePicture,
                    FullName = $"{s.User.FirstName} {s.User.LastName}"
                },
                Designation = s.Designation != null ? new
                {
                    s.Designation.Id,
                    s.Designation.Name,
                    s.Designation.Description
                } : null
            });

            return Ok(mappedStaff);
        }

        [HttpGet("{id}/courses")]
        public async Task<IActionResult> GetCourses(Guid id)
        {
            // If not admin, only allow access to own courses
            if (User.IsInRole("Staff") && id.ToString() != User.FindFirst("nameid")?.Value)
                return Forbid();

            var courses = await _staffManager.GetCoursesByStaffIdAsync(id);
            return Ok(courses);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateStaffDTO staffDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Create user object
            var user = new User
            {
                FirstName = staffDto.FirstName,
                LastName = staffDto.LastName,
                Email = staffDto.Email,
                Username = staffDto.Username,
                PasswordHash = staffDto.Password, // Will be hashed in the manager
                PhoneNumber = staffDto.PhoneNumber,
                Address = staffDto.Address,
                ProfilePicture = staffDto.ProfilePicture
            };

            // Map DTO to entity
            var staff = new Models.Staff
            {
                Id = Guid.NewGuid(),
                DesignationId = staffDto.DesignationId,
                EmployeeId = staffDto.EmployeeId,
                DateOfJoining = staffDto.DateOfJoining,
                Address = staffDto.Address,
                PhoneNumber = staffDto.PhoneNumber,
                Qualification = staffDto.Qualification,
                Salary = staffDto.Salary,
                User = user,
                RoleId = staffDto.RoleId
            };

            try
            {
                var createdStaff = await _staffManager.CreateAsync(staff);

                // Don't return the password hash in the response
                if (createdStaff.User != null)
                {
                    createdStaff.User.PasswordHash = "[HIDDEN]";
                }

                return CreatedAtAction(nameof(GetById), new { id = createdStaff.Id }, createdStaff);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStaffDTO staffDto)
        {
            if (id != staffDto.Id)
                return BadRequest();

            // If not admin, only allow updating own profile
            if (User.IsInRole("Staff") && id.ToString() != User.FindFirst("nameid")?.Value)
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingStaff = await _staffManager.GetByIdAsync(id);
            if (existingStaff == null)
                return NotFound();

            // Update the existing staff with new data
            existingStaff.DesignationId = staffDto.DesignationId;
            existingStaff.EmployeeId = staffDto.EmployeeId;
            existingStaff.DateOfJoining = staffDto.DateOfJoining;
            existingStaff.Address = staffDto.Address;
            existingStaff.PhoneNumber = staffDto.PhoneNumber;
            existingStaff.Qualification = staffDto.Qualification;
            existingStaff.Salary = staffDto.Salary;
            existingStaff.UpdatedAt = DateTime.UtcNow;
            existingStaff.RoleId = staffDto.RoleId;

            // Update user information
            if (existingStaff.User != null)
            {
                existingStaff.User.FirstName = staffDto.FirstName;
                existingStaff.User.LastName = staffDto.LastName;
                existingStaff.User.Email = staffDto.Email;
                existingStaff.User.Username = staffDto.Username;
                existingStaff.User.PhoneNumber = staffDto.PhoneNumber;
                existingStaff.User.Address = staffDto.Address;
                existingStaff.User.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(staffDto.ProfilePicture))
                {
                    existingStaff.User.ProfilePicture = staffDto.ProfilePicture;
                }
            }

            try
            {
                var updatedStaff = await _staffManager.UpdateAsync(existingStaff);
                return Ok(updatedStaff);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var staff = await _staffManager.GetByIdAsync(id);
                if (staff == null)
                    return NotFound(new { message = "Staff member not found" });

                // Check if trying to delete own account
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == staff.UserId.ToString())
                    return BadRequest(new { message = "Cannot delete your own account" });

                // Delete staff - cascade delete will handle all related data
                var result = await _staffManager.DeleteAsync(id);
                if (!result)
                    return BadRequest(new { message = "Failed to delete staff member" });

                return Ok(new { message = $"Staff member '{staff.User?.FullName}' and all associated data deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting staff member", error = ex.Message });
            }
        }

        [HttpGet("dashboard")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetDashboardData()
        {
            var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffIdClaim) || !Guid.TryParse(staffIdClaim, out var staffUserId))
                return BadRequest("Invalid staff ID");

            var dashboardData = await _staffManager.GetStaffDashboardDataAsync(staffUserId);
            return Ok(dashboardData);
        }

        [HttpGet("my-classes")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetMyClasses()
        {
            var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffIdClaim) || !Guid.TryParse(staffIdClaim, out var staffUserId))
                return BadRequest("Invalid staff ID");

            // Get staff record first
            var staff = await _staffManager.GetByUserIdAsync(staffUserId);
            if (staff == null)
                return NotFound("Staff not found");

            // Get classes where this staff is the teacher
            var classList = await _classManager.GetClassesByTeacherIdAsync(staff.Id);
            var classes = classList.Select(c => new
            {
                c.Id,
                c.Name,
                c.GradeLevel,
                c.AcademicYear,
                c.Section,
                c.RoomNumber,
                c.MaxCapacity,
                c.CurrentEnrollment,
                c.CreatedAt,
                c.UpdatedAt
            }).ToList();

            return Ok(classes);
        }

        [HttpGet("my-students")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetMyStudents()
        {
            var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffIdClaim) || !Guid.TryParse(staffIdClaim, out var staffUserId))
                return BadRequest("Invalid staff ID");

            var students = await _staffManager.GetStaffStudentsAsync(staffUserId);

            // Convert to StudentListDTO to avoid serialization issues
            var studentDTOs = students.Select(s => new sms.api.Models.DTOs.StudentListDTO
            {
                Id = s.Id,
                FullName = s.User?.FullName ?? "",
                RollNumber = s.RollNumber,
                RegistrationNumber = s.RegistrationNumber,
                ClassName = s.Class?.Name ?? "",
                Status = s.Status,
                ProfilePicture = null
            }).ToList();

            return Ok(studentDTOs);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetCurrentStaff()
        {
            var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffIdClaim) || !Guid.TryParse(staffIdClaim, out var staffUserId))
                return BadRequest("Invalid staff ID");

            var staff = await _staffManager.GetByUserIdAsync(staffUserId);
            if (staff == null)
                return NotFound("Staff record not found");

            // Map the staff data to match the frontend model structure
            var mappedStaff = new
            {
                staff.Id,
                staff.UserId,
                staff.DesignationId,
                staff.EmployeeId,
                DateOfJoining = staff.DateOfJoining,
                staff.Address,
                staff.PhoneNumber,
                staff.Qualification,
                staff.Salary,
                staff.CreatedAt,
                staff.UpdatedAt,
                User = new
                {
                    staff.User.Id,
                    staff.User.FirstName,
                    staff.User.LastName,
                    staff.User.Email,
                    staff.User.Username,
                    staff.User.Role,
                    staff.User.IsActive,
                    staff.User.ProfilePicture,
                    FullName = $"{staff.User.FirstName} {staff.User.LastName}"
                },
                Designation = staff.Designation != null ? new
                {
                    staff.Designation.Id,
                    staff.Designation.Name,
                    staff.Designation.Description,
                    DepartmentName = staff.Designation.Department != null ? staff.Designation.Department.Name : "Not assigned"
                } : null
            };

            return Ok(mappedStaff);
        }
    }
}
