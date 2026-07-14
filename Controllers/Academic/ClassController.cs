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
using sms.api.Interfaces.Security;

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
        private readonly IStaffDataScopeService _scopeService;

        public ClassController(
            IClassManager classManager,
            IUserManager userManager,
            IStaffManager staffManager,
            IClassEnrollmentManager classEnrollmentManager,
            IStudentManager studentManager,
            IStaffDataScopeService scopeService)
        {
            _classManager = classManager;
            _userManager = userManager;
            _staffManager = staffManager;
            _classEnrollmentManager = classEnrollmentManager;
            _studentManager = studentManager;
            _scopeService = scopeService;
        }

        private Guid? GetAcademicYearIdFromHeader()
        {
            if (Request.Headers.TryGetValue("X-Academic-Year-Id", out var v) && Guid.TryParse(v.ToString().Trim(), out var parsed))
                return parsed;
            return null;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("User ID not found");
            return userId;
        }

        // GET: api/classes?academicYearId=
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<IEnumerable<ClassDTO>>> GetClasses([FromQuery] Guid? academicYearId)
        {
            var yearId = academicYearId ?? GetAcademicYearIdFromHeader();
            var scope = await _scopeService.ResolveAsync(GetCurrentUserId(), yearId);

            if (scope.IsUnrestricted)
            {
                var classes = await _classManager.GetAllClassesAsync(yearId);
                return Ok(classes);
            }

            if (scope.EffectiveScope == sms.api.Constants.DataScopeType.AssignedClasses)
            {
                var allClasses = await _classManager.GetAllClassesAsync(yearId);
                var allowed = scope.AllowedClassIds?.ToHashSet() ?? new HashSet<Guid>();
                return Ok(allClasses.Where(c => allowed.Contains(c.Id)).ToList());
            }

            return Forbid();
        }

        // GET: api/classes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ClassDTO>> GetClass(Guid id)
        {
            var classDto = await _classManager.GetClassByIdAsync(id);
            if (classDto == null)
                return NotFound();

            var scope = await _scopeService.ResolveAsync(GetCurrentUserId(), GetAcademicYearIdFromHeader());
            if (!scope.IsUnrestricted && !scope.CanAccessClass(id))
                return Forbid();

            return Ok(classDto);
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

            // Scope check for update
            var scope = await _scopeService.ResolveAsync(GetCurrentUserId(), GetAcademicYearIdFromHeader());
            var existingClass = await _classManager.GetClassByIdAsync(id);
            if (existingClass == null)
                return NotFound();

            if (!scope.IsUnrestricted)
            {
                if (!scope.CanAccessClass(id))
                    return Forbid();
                classDto.TeacherId = existingClass.TeacherId;
            }

            var updatedClass = await _classManager.UpdateClassAsync(id, classDto);
            if (updatedClass == null)
                return NotFound();
            return Ok(updatedClass);
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
            var scope = await _scopeService.ResolveAsync(GetCurrentUserId(), GetAcademicYearIdFromHeader());
            if (!scope.IsUnrestricted)
            {
                if (!scope.StaffId.HasValue || scope.StaffId.Value != staffId)
                    return Forbid();
            }

            var classes = await _classManager.GetClassesByStaffIdAsync(staffId);
            return Ok(classes);
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
