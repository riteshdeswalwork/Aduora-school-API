using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.DTOs;
using sms.api.Interfaces;
using sms.api.Interfaces.Auth;
using sms.api.Interfaces.Staff;
using sms.api.Interfaces.Student;

namespace sms.api.Controllers.Academic
{
    [Route("api/classes")]
    [ApiController]
    [Authorize]
    public class ClassController : ControllerBase
    {
        private readonly IClassManager _classManager;
        private readonly IUserManager _userManager;
        private readonly IStaffManager _staffManager;
        private readonly IClassEnrollmentManager _classEnrollmentManager;
        private readonly IStudentManager _studentManager;

        public ClassController(
            IClassManager classManager,
            IUserManager userManager,
            IStaffManager staffManager,
            IClassEnrollmentManager classEnrollmentManager,
            IStudentManager studentManager)
        {
            _classManager = classManager;
            _userManager = userManager;
            _staffManager = staffManager;
            _classEnrollmentManager = classEnrollmentManager;
            _studentManager = studentManager;
        }

        // GET: api/classes?academicYearId=
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ClassDTO>>> GetClasses([FromQuery] Guid? academicYearId)
        {
            // If user is Admin, return classes (filtered by academic year when provided)
            if (User.IsInRole("Admin"))
            {
                var classes = await _classManager.GetAllClassesAsync(academicYearId);
                return Ok(classes);
            }
            // If user is Staff, return only classes assigned to them (optionally filter by year)
            else if (User.IsInRole("Staff"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                var userId = Guid.Parse(userIdClaim);

                // Get Staff ID from User ID
                var staff = await _staffManager.GetByUserIdAsync(userId);
                if (staff == null)
                    return NotFound(new { message = $"Staff record not found for user ID: {userId}" });

                var allClasses = await _classManager.GetAllClassesAsync(academicYearId);
                var staffClasses = await _classManager.GetClassesByStaffIdAsync(staff.Id);
                var staffIdSet = staffClasses.Select(c => c.Id).ToHashSet();
                var classes = allClasses.Where(c => staffIdSet.Contains(c.Id)).ToList();

                return Ok(classes);
            }
            // If user is Student, they shouldn't access this endpoint directly
            else
            {
                return Forbid();
            }
        }

        // GET: api/classes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ClassDTO>> GetClass(Guid id)
        {
            var classDto = await _classManager.GetClassByIdAsync(id);

            if (classDto == null)
            {
                return NotFound();
            }

            // If user is Admin, allow access to any class
            if (User.IsInRole("Admin"))
            {
                return Ok(classDto);
            }
            // If user is Staff, only allow access to classes assigned to them
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);

                // Check if this class is assigned to the staff member
                if (classDto.TeacherId == staffId)
                {
                    return Ok(classDto);
                }
                else
                {
                    return Forbid();
                }
            }
            // If user is Student, they shouldn't access this endpoint directly
            else
            {
                return Forbid();
            }
        }

        // POST: api/classes
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ClassDTO>> CreateClass(CreateClassDTO classDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdClass = await _classManager.CreateClassAsync(classDto);
            return CreatedAtAction(nameof(GetClass), new { id = createdClass.Id }, createdClass);
        }

        // PUT: api/classes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClass(Guid id, UpdateClassDTO classDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // If user is Admin, allow updating any class
            if (User.IsInRole("Admin"))
            {
                var updatedClass = await _classManager.UpdateClassAsync(id, classDto);

                if (updatedClass == null)
                {
                    return NotFound();
                }

                return Ok(updatedClass);
            }
            // If user is Staff, only allow updating classes assigned to them
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);

                // Get the class to check if it's assigned to this staff
                var existingClass = await _classManager.GetClassByIdAsync(id);
                if (existingClass == null)
                    return NotFound();

                // Only allow staff to update their own classes
                if (existingClass.TeacherId != staffId)
                    return Forbid();

                // Staff can only update certain fields, not reassign the class
                // Preserve the original teacher ID
                classDto.TeacherId = existingClass.TeacherId;

                var updatedClass = await _classManager.UpdateClassAsync(id, classDto);
                return Ok(updatedClass);
            }
            else
            {
                return Forbid();
            }
        }

        // DELETE: api/classes/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteClass(Guid id)
        {
            var result = await _classManager.DeleteClassAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // GET: api/classes/staff/{staffId}
        [HttpGet("staff/{staffId}")]
        public async Task<ActionResult<IEnumerable<ClassDTO>>> GetClassesByStaffId(Guid staffId)
        {
            // If user is Admin, allow access to any staff's classes
            if (User.IsInRole("Admin"))
            {
                var classes = await _classManager.GetClassesByStaffIdAsync(staffId);
                return Ok(classes);
            }
            // If user is Staff, only allow access to their own classes
            else if (User.IsInRole("Staff"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                var userId = Guid.Parse(userIdClaim);

                // Only allow staff to view their own classes
                if (staffId != userId)
                    return Forbid();

                var classes = await _classManager.GetClassesByStaffIdAsync(staffId);
                return Ok(classes);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpGet("debug-staff-mapping")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DebugStaffMapping()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return BadRequest("User ID not found in token");

            var userId = Guid.Parse(userIdClaim);

            // Get current user info
            var user = await _userManager.GetByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            // Get staff record
            var staff = await _staffManager.GetByUserIdAsync(userId);

            // Get classes assigned to this staff
            var staffId = staff?.Id ?? Guid.Empty;
            var classesAssignedToStaffList = await _classManager.GetClassesByStaffIdAsync(staffId);
            var classesAssignedToStaff = classesAssignedToStaffList.Select(c => new { c.Id, c.Name, c.TeacherId, c.CurrentEnrollment }).ToList();

            // Get all classes with their teacher assignments
            var allClassesList = await _classManager.GetAllClassesAsync();
            var allClasses = allClassesList.Select(c => new { c.Id, c.Name, c.TeacherId, c.CurrentEnrollment }).ToList();

            // Get class enrollments
            var enrollments = await _classEnrollmentManager.GetAllEnrollmentsAsync();
            var classEnrollments = enrollments.Select(e => new { ClassId = e.ClassId, StudentId = e.StudentId, EnrollmentDate = e.EnrollmentDate }).ToList();

            // Get all students
            var students = await _studentManager.GetAllAsync();
            var allStudents = students.Select(s => new { s.Id, FullName = s.User?.FullName }).ToList();

            return Ok(new
            {
                CurrentUser = new { user.Id, user.Username, user.Role, user.FullName },
                StaffRecord = staff != null ? new { staff.Id, staff.UserId, staff.EmployeeId } : null,
                ClassesAssignedToThisStaff = classesAssignedToStaff,
                AllClassesInSystem = allClasses,
                ClassEnrollments = classEnrollments,
                AllStudents = allStudents,
                Message = staff == null ? "No staff record found for this user" : "Staff record found"
            });
        }
    }
}
