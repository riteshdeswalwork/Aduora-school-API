using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Interfaces.Academic;
using sms.api.Interfaces.Staff;
using sms.api.Models;
using sms.api.Models.DTOs;
using sms.api.Models.Enums;

namespace sms.api.Controllers.Student
{
    [Route("api/students")]
    [Authorize]
    public class StudentController : ApiControllerBase
    {
        private readonly IStudentManager _studentManager;
        private readonly IClassEnrollmentManager _classEnrollmentManager;
        private readonly IAcademicYearManager _academicYearManager;
        private readonly IDocumentManager _documentManager;
        private readonly IStaffManager _staffManager;
        private readonly IClassManager _classManager;

        public StudentController(
            IStudentManager studentManager,
            IClassEnrollmentManager classEnrollmentManager,
            IAcademicYearManager academicYearManager,
            IDocumentManager documentManager,
            IStaffManager staffManager,
            IClassManager classManager)
        {
            _studentManager = studentManager;
            _classEnrollmentManager = classEnrollmentManager;
            _academicYearManager = academicYearManager;
            _documentManager = documentManager;
            _staffManager = staffManager;
            _classManager = classManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<Models.Student> studentList;

            // If user is Admin, return all students
            if (User.IsInRole("Admin"))
            {
                studentList = await _studentManager.GetAllAsync();
            }
            // If user is Staff, return only students in their assigned classes
            else if (User.IsInRole("Staff"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                var userId = Guid.Parse(userIdClaim);

                // Get Staff ID from User ID
                var staff = await _staffManager.GetByUserIdAsync(userId);
                if (staff == null)
                    return NotFound(new { message = "Staff record not found for this user" });

                var staffId = staff.Id;

                // Get classes assigned to this staff
                var staffClasses = await _classManager.GetClassesByStaffIdAsync(staffId);

                // Get all students enrolled in these classes
                var allStudents = await _studentManager.GetAllAsync();
                var studentIds = new HashSet<Guid>();

                foreach (var classItem in staffClasses)
                {
                    var enrollments = await _classEnrollmentManager.GetEnrollmentsByClassIdAsync(classItem.Id);
                    foreach (var enrollment in enrollments)
                    {
                        studentIds.Add(enrollment.StudentId);
                    }
                }

                studentList = allStudents.Where(s => studentIds.Contains(s.Id)).ToList();
            }
            else
            {
                return Forbid();
            }

            // Map to simplified DTO for list display
            var mappedStudents = studentList.Select(s => new StudentListDTO
            {
                Id = s.Id,
                FullName = $"{s.User?.FirstName} {s.User?.LastName}".Trim(),
                RollNumber = s.RollNumber,
                RegistrationNumber = s.RegistrationNumber,
                ClassName = s.Class?.Name ?? s.CurrentGrade ?? "No Class",
                Status = s.Status,
                ProfilePicture = s.User?.ProfilePhotoData != null && s.User.ProfilePhotoData.Length > 0
                    ? Convert.ToBase64String(s.User.ProfilePhotoData)
                    : null
            });

            return Ok(mappedStudents);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] Guid? academicYearId = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            IEnumerable<Models.Student> studentList;
            int totalCount;

            if (User.IsInRole("Admin"))
            {
                var (items, count) = await _studentManager.GetPagedAsync(pageNumber, pageSize, searchTerm, academicYearId);
                studentList = items;
                totalCount = count;
            }
            else if (User.IsInRole("Staff"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                var userId = Guid.Parse(userIdClaim);
                var staff = await _staffManager.GetByUserIdAsync(userId);
                if (staff == null)
                    return NotFound(new { message = "Staff record not found for this user" });

                var staffClasses = await _classManager.GetClassesByStaffIdAsync(staff.Id);
                var studentIds = new HashSet<Guid>();
                foreach (var classItem in staffClasses)
                {
                    var enrollments = await _classEnrollmentManager.GetEnrollmentsByClassIdAsync(classItem.Id);
                    foreach (var enrollment in enrollments)
                        studentIds.Add(enrollment.StudentId);
                }

                var (items, count) = await _studentManager.GetPagedAsync(pageNumber, pageSize, searchTerm, academicYearId, studentIds);
                studentList = items;
                totalCount = count;
            }
            else
            {
                return Forbid();
            }

            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
            var firstItemIndex = totalCount == 0 ? 0 : (pageNumber - 1) * pageSize + 1;
            var lastItemIndex = Math.Min(pageNumber * pageSize, totalCount);

            var mappedItems = studentList.Select(s => new StudentListDTO
            {
                Id = s.Id,
                FullName = $"{s.User?.FirstName} {s.User?.LastName}".Trim(),
                RollNumber = s.RollNumber,
                RegistrationNumber = s.RegistrationNumber,
                ClassName = s.Class?.Name ?? s.CurrentGrade ?? "No Class",
                Status = s.Status,
                ProfilePicture = s.User?.ProfilePhotoData != null && s.User.ProfilePhotoData.Length > 0
                    ? Convert.ToBase64String(s.User.ProfilePhotoData)
                    : null
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

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            try
            {
                Console.WriteLine($"Getting student by UserId: {userId}");

                // Authorization check - only allow access to own data for students
                if (User.IsInRole("Student"))
                {
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (currentUserId != userId.ToString())
                        return Forbid();
                }

                var student = await _studentManager.GetByUserIdAsync(userId);
                if (student == null)
                {
                    Console.WriteLine($"Student not found for UserId: {userId}");
                    return NotFound();
                }

                // Map the student data similar to GetById method
                var mappedStudent = new
                {
                    // Student basic info
                    student.Id,
                    student.UserId,
                    student.RegistrationNumber,
                    student.RollNumber,
                    student.DateOfBirth,
                    student.Gender,
                    student.CurrentGrade,
                    student.EnrollmentDate,
                    student.CreatedAt,
                    student.UpdatedAt,

                    // Personal Details
                    student.BloodGroup,
                    student.AadhaarNumber,
                    student.Religion,
                    student.Category,

                    // Parent/Guardian Details
                    student.FatherName,
                    student.MotherName,
                    student.GuardianName,
                    student.GuardianContact,
                    student.ParentName,
                    student.ParentPhone,
                    student.AnnualIncome,

                    // Address Details
                    PermanentAddress = student.Address,
                    student.CurrentAddress,

                    // School Details
                    student.AdmissionNumber,
                    student.AdmissionDate,
                    student.PreviousSchoolName,
                    student.MonthlyFee,
                    student.Status,
                    student.StatusChangedAt,
                    student.StatusChangedBy,

                    // User properties
                    User = new
                    {
                        student.User?.Id,
                        student.User?.FirstName,
                        student.User?.LastName,
                        student.User?.Email,
                        student.User?.Username,
                        student.User?.PhoneNumber,
                        student.User?.Address,
                        student.User?.Role,
                        student.User?.CreatedAt,
                        student.User?.UpdatedAt,
                        student.User?.IsActive,
                        ProfilePhotoData = student.User?.ProfilePhotoData,
                        ProfilePhotoContentType = student.User?.ProfilePhotoContentType,
                        ProfilePhotoFileName = student.User?.ProfilePhotoFileName,
                        FullName = $"{student.User?.FirstName} {student.User?.LastName}".Trim()
                    },

                    // Class information
                    Class = student.Class != null ? new
                    {
                        student.Class.Id,
                        student.Class.Name,
                        student.Class.GradeLevel,
                        student.Class.Section,
                        student.Class.AcademicYear,
                        student.Class.RoomNumber,
                        student.Class.MaxCapacity,
                        student.Class.CurrentEnrollment
                    } : null
                };

                return Ok(mappedStudent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting student by UserId: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving student", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                Console.WriteLine($"Getting student by ID: {id}");
                var student = await _studentManager.GetByIdAsync(id);
                if (student == null)
                {
                    Console.WriteLine($"Student not found: {id}");
                    return NotFound();
                }

            // If user is Admin, allow access to any student
            if (User.IsInRole("Admin"))
            {
                // Continue with normal flow
            }
            // If user is Staff, only allow access to students in their assigned classes
            else if (User.IsInRole("Staff"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                var userId = Guid.Parse(userIdClaim);

                // Get Staff ID from User ID
                var staff = await _staffManager.GetByUserIdAsync(userId);
                if (staff == null)
                    return NotFound(new { message = "Staff record not found for this user" });

                var staffId = staff.Id;

                // Get classes assigned to this staff
                var staffClasses = await _classManager.GetClassesByStaffIdAsync(staffId);
                var classIds = staffClasses.Select(c => c.Id).ToList();

                // Check if student is enrolled in any of these classes
                var studentEnrollments = await _classEnrollmentManager.GetEnrollmentsByStudentIdAsync(id);
                var isStudentInStaffClass = studentEnrollments.Any(e => classIds.Contains(e.ClassId));

                if (!isStudentInStaffClass)
                    return Forbid();
            }
            // If user is Student, only allow access to own profile
            else if (User.IsInRole("Student"))
            {
                if (id.ToString() != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                    return Forbid();
            }
            else
            {
                return Forbid();
            }

            // Get student's enrolled subjects first - simplified approach
            List<object> enrolledSubjects;
            try
            {
                Console.WriteLine($"Getting enrolled subjects for student: {student.Id}");

                var studentSubjects = await _studentManager.GetStudentSubjectsWithSubjectAsync(student.Id, null);
                enrolledSubjects = studentSubjects.Where(ss => ss.Subject != null).Select(ss => new
                {
                    ss.Subject!.Id,
                    ss.Subject.Name,
                    ss.Subject.Code,
                    ss.Subject.Description,
                    ss.IsOptional,
                    ss.AcademicYear,
                    ss.Subject.StaffId,
                    ss.Subject.CreatedAt,
                    ss.Subject.UpdatedAt,
                    ss.Subject.IsActive,
                    IsCompulsory = false
                }).Cast<object>().ToList();

                Console.WriteLine($"Found {enrolledSubjects.Count} enrolled subjects");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting enrolled subjects: {ex.Message}");
                enrolledSubjects = new List<object>();
            }

            // Map the student data to include ALL properties for the frontend
            var mappedStudent = new
            {
                // Student basic info
                student.Id,
                student.UserId, // IMPORTANT: Include UserId for document upload
                student.RegistrationNumber,
                student.RollNumber,
                student.DateOfBirth,
                student.Gender,
                student.CurrentGrade,
                student.EnrollmentDate,
                student.CreatedAt,
                student.UpdatedAt,

                // Personal Details
                student.BloodGroup,
                student.AadhaarNumber,
                student.Religion,
                student.Category,

                // Parent/Guardian Details
                student.FatherName,
                student.MotherName,
                student.GuardianName,
                student.GuardianContact,
                student.ParentName,
                student.ParentPhone,
                student.AnnualIncome,

                // Address Details
                PermanentAddress = student.Address, // Map Address to PermanentAddress
                student.CurrentAddress,

                // School Details
                student.AdmissionNumber,
                student.PreviousSchoolName,
                student.ClassId,
                AdmissionDate = student.AdmissionDate,
                DateOfAdmission = student.AdmissionDate, // Map for frontend compatibility
                student.MonthlyFee,

                // Student Status
                student.Status,
                student.StatusChangedAt,
                student.StatusChangeReason,
                student.StatusChangedBy,

                // User properties
                User = new
                {
                    student.User?.Id,
                    student.User?.FirstName,
                    student.User?.LastName,
                    student.User?.Email,
                    student.User?.Username,
                    student.User?.PhoneNumber,
                    student.User?.Address,
                    student.User?.Role,
                    student.User?.CreatedAt,
                    student.User?.UpdatedAt,
                    student.User?.IsActive,
                    ProfilePhotoData = student.User?.ProfilePhotoData,
                    ProfilePhotoContentType = student.User?.ProfilePhotoContentType,
                    ProfilePhotoFileName = student.User?.ProfilePhotoFileName,
                    FullName = $"{student.User?.FirstName} {student.User?.LastName}".Trim()
                },

                // Class information
                Class = student.Class != null ? new
                {
                    student.Class.Id,
                    student.Class.Name,
                    student.Class.Section,
                    student.Class.GradeLevel,
                    student.Class.RoomNumber,
                    student.Class.MaxCapacity,
                    student.Class.MonthlyFee,
                    student.Class.YearlyFee,
                    student.Class.MinimumSubjects,
                    // Include subjects for this class
                    Subjects = student.Class.ClassSubjects?.Select(cs => new
                    {
                        cs.Subject.Id,
                        cs.Subject.Name,
                        cs.Subject.Code,
                        cs.Subject.Description,
                        cs.Subject.StaffId,
                        cs.Subject.CreatedAt,
                        cs.Subject.UpdatedAt,
                        cs.Subject.IsActive,
                        cs.IsCompulsory,
                        Staff = cs.Subject.Staff != null ? new
                        {
                            cs.Subject.Staff.Id,
                            cs.Subject.Staff.Designation,
                            FirstName = cs.Subject.Staff.User?.FirstName,
                            LastName = cs.Subject.Staff.User?.LastName,
                            FullName = $"{cs.Subject.Staff.User?.FirstName} {cs.Subject.Staff.User?.LastName}".Trim()
                        } : null
                    }).ToList()
                } : null,

                // Include student's actual enrolled subjects (not just class subjects)
                Subjects = enrolledSubjects,

                // Computed properties for backward compatibility
                FullName = $"{student.User?.FirstName} {student.User?.LastName}".Trim(),
                PhoneNumber = student.User?.PhoneNumber ?? student.PhoneNumber,
                Email = student.User?.Email
            };

                Console.WriteLine($"Successfully mapped student data for: {id}");
                return Ok(mappedStudent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetById for student {id}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error retrieving student", error = ex.Message });
            }
        }

        [HttpGet("grade/{grade}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByGrade(string grade)
        {
            var studentList = await _studentManager.GetStudentsByGradeAsync(grade);

            // Map to simplified DTO for list display
            var mappedStudents = studentList.Select(s => new StudentListDTO
            {
                Id = s.Id,
                FullName = $"{s.User?.FirstName} {s.User?.LastName}".Trim(),
                RollNumber = s.RollNumber,
                RegistrationNumber = s.RegistrationNumber,
                ClassName = s.Class?.Name ?? s.CurrentGrade ?? "No Class",
                Status = s.Status,
                ProfilePicture = s.User?.ProfilePhotoData != null && s.User.ProfilePhotoData.Length > 0
                    ? Convert.ToBase64String(s.User.ProfilePhotoData)
                    : null
            });

            return Ok(mappedStudents);
        }

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByClass(Guid classId)
        {
            try
            {
                // Get class enrollments for the specified class
                var classEnrollmentManager = HttpContext.RequestServices.GetService(typeof(IClassEnrollmentManager)) as IClassEnrollmentManager;
                if (classEnrollmentManager == null)
                    return StatusCode(500, "Service unavailable");

                var enrollments = await classEnrollmentManager.GetEnrollmentsByClassIdAsync(classId);

                // Get students from enrollments
                var studentIds = enrollments.Where(e => e.IsActive).Select(e => e.StudentId).ToList();
                var students = new List<Models.Student>();

                foreach (var studentId in studentIds)
                {
                    var student = await _studentManager.GetByIdAsync(studentId);
                    if (student != null)
                    {
                        students.Add(student);
                    }
                }

                // Map to simplified DTO for list display
                var mappedStudents = students.Select(s => new StudentListDTO
                {
                    Id = s.Id,
                    FullName = $"{s.User?.FirstName} {s.User?.LastName}".Trim(),
                    RollNumber = s.RollNumber,
                    RegistrationNumber = s.RegistrationNumber,
                    ClassName = s.Class?.Name ?? s.CurrentGrade ?? "No Class",
                    Status = s.Status,
                    ProfilePicture = s.User?.ProfilePhotoData != null && s.User.ProfilePhotoData.Length > 0
                        ? Convert.ToBase64String(s.User.ProfilePhotoData)
                        : null
                });

                return Ok(mappedStudents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving students: {ex.Message}");
            }
        }



        [HttpGet("{id}/attendance")]
        public async Task<IActionResult> GetAttendance(Guid id, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            // If student, only allow access to own attendance
            if (User.IsInRole("Student") && id.ToString() != User.FindFirst("nameid")?.Value)
                return Forbid();

            var attendance = await _studentManager.GetAttendanceByStudentIdAsync(id, fromDate, toDate);
            return Ok(attendance);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateStudentDTO studentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Split full name into first and last name
            var nameParts = studentDto.FullName.Split(' ', 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            // Handle profile photo if provided (base64 data from frontend)
            byte[]? profilePhotoData = null;
            string? profilePhotoContentType = "image/jpeg";
            string? profilePhotoFileName = "profile.jpg";

            // Check if profile photo data is provided in the DTO
            if (!string.IsNullOrEmpty(studentDto.ProfilePhotoData))
            {
                try
                {
                    // Remove data URL prefix if present (data:image/jpeg;base64,)
                    var base64Data = studentDto.ProfilePhotoData;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }

                    profilePhotoData = Convert.FromBase64String(base64Data);

                    // Validate file size (max 5MB)
                    if (profilePhotoData.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest("Profile photo size cannot exceed 5MB");
                    }
                }
                catch (Exception)
                {
                    return BadRequest("Invalid profile photo data");
                }
            }

            // Create user object
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = studentDto.Email,
                Username = studentDto.Username,
                PasswordHash = studentDto.Password, // Will be hashed in the manager
                Role = "Student", // Set role explicitly
                PhoneNumber = studentDto.PhoneNumber,
                Address = studentDto.PermanentAddress,
                ProfilePhotoData = profilePhotoData,
                ProfilePhotoContentType = profilePhotoContentType,
                ProfilePhotoFileName = profilePhotoFileName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Map DTO to entity
            var student = new Models.Student
            {
                Id = Guid.NewGuid(),
                RegistrationNumber = studentDto.AdmissionNumber, // Use AdmissionNumber as RegistrationNumber
                RollNumber = studentDto.RollNumber,
                DateOfBirth = studentDto.DateOfBirth,
                Gender = studentDto.Gender,
                // Personal Details
                BloodGroup = studentDto.BloodGroup,
                AadhaarNumber = studentDto.AadhaarNumber,
                Religion = studentDto.Religion,
                Category = studentDto.Category,
                // Parent/Guardian Details
                FatherName = studentDto.FatherName,
                MotherName = studentDto.MotherName,
                GuardianName = studentDto.GuardianName,
                GuardianContact = studentDto.GuardianContact,
                ParentName = studentDto.FatherName, // Map FatherName to ParentName for backward compatibility
                ParentPhone = studentDto.GuardianContact, // Use GuardianContact as ParentPhone
                AnnualIncome = studentDto.AnnualIncome,
                // Address Details
                Address = studentDto.PermanentAddress, // Permanent Address
                CurrentAddress = studentDto.CurrentAddress,
                PhoneNumber = studentDto.PhoneNumber,
                // School Details
                AdmissionNumber = studentDto.AdmissionNumber,
                PreviousSchoolName = studentDto.PreviousSchoolName,
                ClassId = studentDto.ClassId,
                // 🚌 Bus Transportation
                BusStopId = studentDto.BusStopId,
                AdmissionDate = studentDto.DateOfAdmission,
                EnrollmentDate = studentDto.DateOfAdmission, // Set EnrollmentDate same as AdmissionDate
                MonthlyFee = studentDto.MonthlyFee,
                CurrentGrade = "", // Will be set based on class
                Status = StudentStatus.Active,
                User = user,
                RoleId = studentDto.RoleId
            };

            try
            {
                var createdStudent = await _studentManager.CreateAsync(student);

                // Create class enrollment for student count tracking
                if (studentDto.ClassId.HasValue)
                {
                    // Get current academic year
                    var currentAcademicYear = await _academicYearManager.GetCurrentAcademicYearAsync();
                    if (currentAcademicYear == null)
                    {
                        return BadRequest(new { message = "No current academic year found. Please create and set a current academic year first." });
                    }

                    var classEnrollment = new ClassEnrollment
                    {
                        Id = Guid.NewGuid(),
                        StudentId = createdStudent.Id,
                        ClassId = studentDto.ClassId.Value,
                        AcademicYearId = currentAcademicYear.Id,
                        EnrollmentDate = DateTime.UtcNow,
                        AcademicYear = currentAcademicYear.YearCode,
                        IsActive = true,
                        Status = "Active"
                    };
                    await _classEnrollmentManager.CreateAsync(classEnrollment);
                }

                // Handle subject enrollments if provided
                if (studentDto.SelectedSubjectIds != null && studentDto.SelectedSubjectIds.Any())
                {
                    Console.WriteLine($"Creating subject enrollments for {studentDto.SelectedSubjectIds.Count} subjects");
                    Console.WriteLine($"Optional subjects: {studentDto.OptionalSubjectIds?.Count ?? 0}");
                    await UpdateStudentSubjects(createdStudent.Id, studentDto.SelectedSubjectIds, studentDto.OptionalSubjectIds);
                    Console.WriteLine("Subject enrollments created successfully");
                }

                // Handle document uploads if provided
                if (studentDto.Documents != null && studentDto.Documents.Any())
                {
                    Console.WriteLine($"Creating documents for student: {studentDto.Documents.Count} documents");

                    foreach (var docDto in studentDto.Documents)
                    {
                        try
                        {
                            var document = new Models.Document
                            {
                                Id = Guid.NewGuid(),
                                UserId = createdStudent.UserId,
                                DocumentType = docDto.DocumentType,
                                Description = docDto.Description,
                                FileName = docDto.FileName,
                                ContentType = docDto.ContentType,
                                DocumentData = docDto.DocumentData,
                                FileSize = docDto.DocumentData.Length,
                                CreatedAt = DateTime.UtcNow,
                                IsVerified = false,
                                VerifiedAt = null,
                                VerifiedBy = null
                            };

                            await _documentManager.CreateAsync(document);
                            Console.WriteLine($"Document created: {docDto.DocumentType} - {docDto.FileName}");
                        }
                        catch (Exception docEx)
                        {
                            Console.WriteLine($"Error creating document {docDto.DocumentType}: {docEx.Message}");
                            // Continue with other documents even if one fails
                        }
                    }

                    Console.WriteLine("Document creation completed");
                }

                // Don't return the password hash in the response
                if (createdStudent.User != null)
                {
                    createdStudent.User.PasswordHash = "[HIDDEN]";
                }

                return CreatedAtAction(nameof(GetById), new { id = createdStudent.Id }, createdStudent);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the full exception details
                Console.WriteLine($"Error creating student: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }

                return StatusCode(500, new { message = "Error creating student", error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] dynamic updateData)
        {
            try
            {
                Console.WriteLine($"=== UPDATE STUDENT API CALLED ===");
                Console.WriteLine($"Student ID: {id}");
                Console.WriteLine($"Update Data: {updateData}");
                Console.WriteLine($"User Role: {User.FindFirst("role")?.Value}");
                Console.WriteLine($"User ID: {User.FindFirst("nameid")?.Value}");

                // If student, only allow updating own profile
                if (User.IsInRole("Student") && id.ToString() != User.FindFirst("nameid")?.Value)
                    return Forbid();

                var existingStudent = await _studentManager.GetByIdAsync(id);
                if (existingStudent == null)
                {
                    Console.WriteLine($"Student not found: {id}");
                    return NotFound();
                }

                // Parse the dynamic data
                var jsonElement = (JsonElement)updateData;
                static string GetStr(JsonElement el) => el.ValueKind == JsonValueKind.Null || el.ValueKind == JsonValueKind.Undefined ? "" : (el.GetString() ?? "");
                static DateTime? GetDateTimeSafe(JsonElement el) => el.ValueKind == JsonValueKind.Null || el.ValueKind == JsonValueKind.Undefined ? null : (el.TryGetDateTime(out var dt) ? dt : (DateTime?)null);
                static decimal GetDecimalSafe(JsonElement el) => el.ValueKind == JsonValueKind.Null || el.ValueKind == JsonValueKind.Undefined ? 0 : (el.TryGetDecimal(out var d) ? d : 0);

                // Update student properties
                if (jsonElement.TryGetProperty("fullName", out var fullNameProp))
                {
                    var fullName = GetStr(fullNameProp);
                    var nameParts = (fullName ?? "").Split(' ', 2);
                    existingStudent.User.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
                    existingStudent.User.LastName = nameParts.Length > 1 ? nameParts[1] : "";
                }

                if (jsonElement.TryGetProperty("fatherName", out var fatherNameProp))
                    existingStudent.FatherName = GetStr(fatherNameProp);

                if (jsonElement.TryGetProperty("motherName", out var motherNameProp))
                    existingStudent.MotherName = GetStr(motherNameProp);

                if (jsonElement.TryGetProperty("dateOfBirth", out var dobProp) && GetDateTimeSafe(dobProp) is { } dob)
                    existingStudent.DateOfBirth = dob;

                if (jsonElement.TryGetProperty("gender", out var genderProp))
                    existingStudent.Gender = GetStr(genderProp);

                if (jsonElement.TryGetProperty("bloodGroup", out var bloodGroupProp))
                    existingStudent.BloodGroup = GetStr(bloodGroupProp);

                if (jsonElement.TryGetProperty("aadhaarNumber", out var aadhaarProp))
                    existingStudent.AadhaarNumber = GetStr(aadhaarProp);

                if (jsonElement.TryGetProperty("religion", out var religionProp))
                    existingStudent.Religion = GetStr(religionProp);

                if (jsonElement.TryGetProperty("category", out var categoryProp))
                    existingStudent.Category = GetStr(categoryProp);

                if (jsonElement.TryGetProperty("permanentAddress", out var permAddressProp))
                    existingStudent.Address = GetStr(permAddressProp);

                if (jsonElement.TryGetProperty("currentAddress", out var currAddressProp))
                    existingStudent.CurrentAddress = GetStr(currAddressProp);

                if (jsonElement.TryGetProperty("phoneNumber", out var phoneProp))
                    existingStudent.User.PhoneNumber = GetStr(phoneProp);

                if (jsonElement.TryGetProperty("email", out var emailProp))
                    existingStudent.User.Email = GetStr(emailProp);

                if (jsonElement.TryGetProperty("admissionNumber", out var admissionProp))
                    existingStudent.AdmissionNumber = GetStr(admissionProp);

                if (jsonElement.TryGetProperty("dateOfAdmission", out var admissionDateProp) && GetDateTimeSafe(admissionDateProp) is { } admDate)
                    existingStudent.AdmissionDate = admDate;

                if (jsonElement.TryGetProperty("classId", out var classIdProp))
                {
                    if (classIdProp.ValueKind == JsonValueKind.Null || classIdProp.ValueKind == JsonValueKind.Undefined)
                        existingStudent.ClassId = null;
                    else if (classIdProp.TryGetGuid(out var classId))
                        existingStudent.ClassId = classId;
                }

                if (jsonElement.TryGetProperty("busStopId", out var busStopIdProp))
                {
                    if (busStopIdProp.ValueKind == JsonValueKind.Null || busStopIdProp.ValueKind == JsonValueKind.Undefined)
                        existingStudent.BusStopId = null;
                    else if (busStopIdProp.TryGetGuid(out var busStopId))
                        existingStudent.BusStopId = busStopId;
                }

                if (jsonElement.TryGetProperty("rollNumber", out var rollProp))
                    existingStudent.RollNumber = GetStr(rollProp);

                if (jsonElement.TryGetProperty("previousSchoolName", out var prevSchoolProp))
                    existingStudent.PreviousSchoolName = GetStr(prevSchoolProp);

                if (jsonElement.TryGetProperty("guardianName", out var guardianNameProp))
                    existingStudent.GuardianName = GetStr(guardianNameProp);

                if (jsonElement.TryGetProperty("guardianContact", out var guardianContactProp))
                    existingStudent.GuardianContact = GetStr(guardianContactProp);

                if (jsonElement.TryGetProperty("annualIncome", out var incomeProp))
                    existingStudent.AnnualIncome = GetDecimalSafe(incomeProp);

                if (jsonElement.TryGetProperty("username", out var usernameProp))
                    existingStudent.User.Username = GetStr(usernameProp);

                if (jsonElement.TryGetProperty("password", out var passwordProp))
                {
                    var password = GetStr(passwordProp);
                    if (!string.IsNullOrEmpty(password) && password != "********")
                        existingStudent.User.PasswordHash = password;
                }

                if (jsonElement.TryGetProperty("monthlyFee", out var feeProp))
                    existingStudent.MonthlyFee = GetDecimalSafe(feeProp);

                // Handle subject updates
                if (jsonElement.TryGetProperty("selectedSubjectIds", out var subjectsProp))
                {
                    var selectedSubjectIds = new List<Guid>();
                    var optionalSubjectIds = new List<Guid>();

                    Console.WriteLine("=== API SUBJECT UPDATE DEBUG ===");
                    Console.WriteLine($"Subject property found: {subjectsProp.ValueKind}");

                    if (subjectsProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var subjectElement in subjectsProp.EnumerateArray())
                        {
                            if (subjectElement.TryGetGuid(out var subjectId))
                            {
                                selectedSubjectIds.Add(subjectId);
                                Console.WriteLine($"Added subject ID: {subjectId}");
                            }
                        }
                    }

                    // Handle optional subjects
                    if (jsonElement.TryGetProperty("optionalSubjectIds", out var optionalProp))
                    {
                        Console.WriteLine($"Optional subjects property found: {optionalProp.ValueKind}");

                        if (optionalProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var optionalElement in optionalProp.EnumerateArray())
                            {
                                if (optionalElement.TryGetGuid(out var optionalId))
                                {
                                    optionalSubjectIds.Add(optionalId);
                                    Console.WriteLine($"Added optional subject ID: {optionalId}");
                                }
                            }
                        }
                    }

                    Console.WriteLine($"Total selected subjects: {selectedSubjectIds.Count}");
                    Console.WriteLine($"Total optional subjects: {optionalSubjectIds.Count}");
                    Console.WriteLine("=== END API SUBJECT DEBUG ===");

                    // Update student's subject enrollments
                    try
                    {
                        Console.WriteLine($"Updating subjects for student: {existingStudent.Id}");
                        await UpdateStudentSubjects(existingStudent.Id, selectedSubjectIds, optionalSubjectIds);
                        Console.WriteLine("Subject update completed successfully");
                    }
                    catch (Exception subjectEx)
                    {
                        Console.WriteLine($"Error updating subjects: {subjectEx.Message}");
                        Console.WriteLine($"Subject error stack trace: {subjectEx.StackTrace}");
                        throw; // Re-throw to be caught by outer catch
                    }
                }

                var updatedStudent = await _studentManager.UpdateAsync(existingStudent);
                return Ok(updatedStudent);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== UPDATE STUDENT ERROR ===");
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"=== END UPDATE STUDENT ERROR ===");
                return StatusCode(500, new { message = "Error updating student", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var student = await _studentManager.GetByIdAsync(id);
                if (student == null)
                    return NotFound(new { message = "Student not found" });

                // Delete student - cascade delete will handle all related data
                var result = await _studentManager.DeleteAsync(id);
                if (!result)
                    return BadRequest(new { message = "Failed to delete student" });

                return Ok(new { message = $"Student '{student.User?.FullName}' and all associated data deleted successfully" });
            }
            catch (Exception ex)
            {
                // Check if it's a foreign key constraint error
                if (ex.Message.Contains("FOREIGN KEY constraint") || ex.Message.Contains("foreign key"))
                {
                    return BadRequest(new {
                        message = "Cannot delete student",
                        error = "This student has associated records (attendance, fees, etc.) that must be removed first.",
                        suggestion = "Please remove all related records before deleting the student, or contact administrator for help."
                    });
                }

                return StatusCode(500, new { message = "Error deleting student", error = ex.Message });
            }
        }

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateStudent(Guid id)
        {
            try
            {
                var student = await _studentManager.GetByIdAsync(id);
                if (student == null)
                    return NotFound(new { message = "Student not found" });

                // Change status to Inactive instead of deleting
                student.Status = StudentStatus.Inactive;
                student.StatusChangedAt = DateTime.UtcNow;
                student.StatusChangeReason = "Deactivated by administrator";

                var result = await _studentManager.UpdateAsync(student);
                if (result == null)
                    return BadRequest(new { message = "Failed to deactivate student" });

                return Ok(new { message = $"Student '{student.User?.FullName}' has been deactivated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error deactivating student", error = ex.Message });
            }
        }

        [HttpPost("{id}/profile-photo")]
        public async Task<IActionResult> UploadProfilePhoto(Guid id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest("Only image files (JPEG, PNG, GIF) are allowed");
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("File size cannot exceed 5MB");
                }

                var student = await _studentManager.GetByIdAsync(id);
                if (student == null)
                {
                    return NotFound("Student not found");
                }

                // Check authorization
                if (User.IsInRole("Student") && id.ToString() != User.FindFirst("nameid")?.Value)
                    return Forbid();

                // Convert file to byte array and update user profile photo
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    student.User.ProfilePhotoData = memoryStream.ToArray();
                    student.User.ProfilePhotoContentType = file.ContentType;
                    student.User.ProfilePhotoFileName = file.FileName;
                    student.User.UpdatedAt = DateTime.UtcNow;
                }

                await _studentManager.UpdateAsync(student);

                return Ok(new {
                    message = "Profile photo uploaded successfully",
                    fileName = file.FileName,
                    contentType = file.ContentType,
                    size = file.Length
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error uploading profile photo", error = ex.Message });
            }
        }

        [HttpGet("{id}/profile-photo")]
        public async Task<IActionResult> GetProfilePhoto(Guid id)
        {
            try
            {
                var student = await _studentManager.GetByIdAsync(id);
                if (student == null)
                {
                    return NotFound("Student not found");
                }

                // Check authorization
                if (User.IsInRole("Student") && id.ToString() != User.FindFirst("nameid")?.Value)
                    return Forbid();

                if (student.User?.ProfilePhotoData == null || student.User.ProfilePhotoData.Length == 0)
                {
                    return NotFound("No profile photo found");
                }

                return File(student.User.ProfilePhotoData, student.User.ProfilePhotoContentType ?? "image/jpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving profile photo", error = ex.Message });
            }
        }

        [HttpDelete("{id}/profile-photo")]
        public async Task<IActionResult> DeleteProfilePhoto(Guid id)
        {
            try
            {
                var student = await _studentManager.GetByIdAsync(id);
                if (student == null)
                {
                    return NotFound("Student not found");
                }

                // Check authorization
                if (User.IsInRole("Student") && id.ToString() != User.FindFirst("nameid")?.Value)
                    return Forbid();

                student.User.ProfilePhotoData = null;
                student.User.ProfilePhotoContentType = null;
                student.User.ProfilePhotoFileName = null;
                student.User.UpdatedAt = DateTime.UtcNow;

                await _studentManager.UpdateAsync(student);

                return Ok(new { message = "Profile photo deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting profile photo", error = ex.Message });
            }
        }

        private async Task UpdateStudentSubjects(Guid studentId, List<Guid> selectedSubjectIds, List<Guid>? optionalSubjectIds = null)
        {
            var currentAcademicYear = await _academicYearManager.GetCurrentAcademicYearAsync();
            var academicYearCode = currentAcademicYear?.YearCode ?? DateTime.Now.Year.ToString();
            await _studentManager.SetStudentSubjectsAsync(studentId, academicYearCode, selectedSubjectIds, optionalSubjectIds);
        }

        private async Task<List<object>> GetStudentEnrolledSubjects(Guid studentId)
        {
            try
            {
                var studentSubjects = await _studentManager.GetStudentSubjectsWithSubjectAsync(studentId, null);
                var enrolledSubjects = studentSubjects.Where(ss => ss.Subject != null).Select(ss => new
                {
                    ss.Subject!.Id,
                    ss.Subject.Name,
                    ss.Subject.Code,
                    ss.Subject.Description,
                    ss.Subject.StaffId,
                    ss.Subject.CreatedAt,
                    ss.Subject.UpdatedAt,
                    ss.Subject.IsActive,
                    ss.IsOptional,
                    ss.AcademicYear,
                    IsCompulsory = false,
                    Staff = ss.Subject.Staff != null ? new
                    {
                        ss.Subject.Staff.Id,
                        ss.Subject.Staff.Designation,
                        FirstName = ss.Subject.Staff.User != null ? ss.Subject.Staff.User.FirstName : null,
                        LastName = ss.Subject.Staff.User != null ? ss.Subject.Staff.User.LastName : null,
                        FullName = ss.Subject.Staff.User != null ? (ss.Subject.Staff.User.FirstName + " " + ss.Subject.Staff.User.LastName).Trim() : ""
                    } : null
                }).Cast<object>().ToList();
                return await Task.FromResult(enrolledSubjects);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting student enrolled subjects: {ex.Message}");
                return new List<object>();
            }
        }

        [HttpGet("{id}/subjects/year/{academicYearId}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetStudentSubjectsByAcademicYear(Guid id, Guid academicYearId)
        {
            try
            {
                // If student, only allow access to own data
                if (User.IsInRole("Student") && id.ToString() != User.FindFirst("nameid")?.Value)
                    return Forbid();

                var subjects = await GetStudentEnrolledSubjectsByAcademicYear(id, academicYearId);
                return Ok(subjects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving student subjects for academic year", error = ex.Message });
            }
        }

        [HttpGet("{id}/academic-info/year/{academicYearId}")]
        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> GetStudentAcademicInfoByYear(Guid id, Guid academicYearId)
        {
            try
            {
                // If student, only allow access to own data
                if (User.IsInRole("Student") && id.ToString() != User.FindFirst("nameid")?.Value)
                    return Forbid();

                var academicInfo = await GetStudentAcademicInfoByAcademicYear(id, academicYearId);
                return Ok(academicInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving student academic information for academic year", error = ex.Message });
            }
        }

        private async Task<List<object>> GetStudentEnrolledSubjectsByAcademicYear(Guid studentId, Guid academicYearId)
        {
            try
            {
                var academicYear = await _academicYearManager.GetAcademicYearByIdAsync(academicYearId);
                if (academicYear == null)
                    return new List<object>();

                var studentSubjects = await _studentManager.GetStudentSubjectsWithSubjectAsync(studentId, academicYear.YearCode);
                var enrolledSubjects = studentSubjects.Where(ss => ss.Subject != null).Select(ss => new
                {
                    ss.Subject!.Id,
                    ss.Subject.Name,
                    ss.Subject.Code,
                    ss.Subject.Description,
                    ss.Subject.StaffId,
                    ss.Subject.CreatedAt,
                    ss.Subject.UpdatedAt,
                    ss.Subject.IsActive,
                    ss.AcademicYear,
                    ss.IsOptional,
                    IsCompulsory = false,
                    Staff = ss.Subject.Staff != null ? new
                    {
                        ss.Subject.Staff.Id,
                        ss.Subject.Staff.Designation,
                        FirstName = ss.Subject.Staff.User != null ? ss.Subject.Staff.User.FirstName : null,
                        LastName = ss.Subject.Staff.User != null ? ss.Subject.Staff.User.LastName : null,
                        FullName = ss.Subject.Staff.User != null ? (ss.Subject.Staff.User.FirstName + " " + ss.Subject.Staff.User.LastName).Trim() : ""
                    } : null
                }).Cast<object>().ToList();
                return await Task.FromResult(enrolledSubjects);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting student enrolled subjects by academic year: {ex.Message}");
                return new List<object>();
            }
        }

        private async Task<object?> GetStudentAcademicInfoByAcademicYear(Guid studentId, Guid academicYearId)
        {
            return await _studentManager.GetStudentAcademicInfoByYearAsync(studentId, academicYearId);
        }

        [HttpPut("complete-update/{id}")]
        public async Task<IActionResult> CompleteUpdate(Guid id, [FromBody] CompleteStudentUpdateDTO updateData)
        {
            try
            {
                Console.WriteLine($"=== COMPLETE STUDENT UPDATE API CALLED ===");
                Console.WriteLine($"Student ID: {id}");
                Console.WriteLine($"Update Data: {System.Text.Json.JsonSerializer.Serialize(updateData)}");

                // If student, only allow updating own profile
                if (User.IsInRole("Student") && id.ToString() != User.FindFirst("nameid")?.Value)
                    return Forbid();

                var existingStudent = await _studentManager.GetByIdAsync(id);
                if (existingStudent == null)
                {
                    Console.WriteLine($"Student not found: {id}");
                    return NotFound();
                }

                // Update basic student information
                if (!string.IsNullOrEmpty(updateData.RollNumber))
                    existingStudent.RollNumber = updateData.RollNumber;

                if (!string.IsNullOrEmpty(updateData.RegistrationNumber))
                    existingStudent.RegistrationNumber = updateData.RegistrationNumber;

                if (!string.IsNullOrEmpty(updateData.AdmissionNumber))
                    existingStudent.AdmissionNumber = updateData.AdmissionNumber;

                if (updateData.DateOfBirth.HasValue)
                    existingStudent.DateOfBirth = updateData.DateOfBirth.Value;

                if (!string.IsNullOrEmpty(updateData.Gender))
                    existingStudent.Gender = updateData.Gender;

                if (updateData.ClassId.HasValue)
                    existingStudent.ClassId = updateData.ClassId.Value;

                if (!string.IsNullOrEmpty(updateData.CurrentGrade))
                    existingStudent.CurrentGrade = updateData.CurrentGrade;

                if (updateData.Status.HasValue)
                    existingStudent.Status = updateData.Status.Value;

                existingStudent.UpdatedAt = DateTime.UtcNow;

                // Update user information if provided
                if (existingStudent.User != null && updateData.UserInfo != null)
                {
                    if (!string.IsNullOrEmpty(updateData.UserInfo.FirstName))
                        existingStudent.User.FirstName = updateData.UserInfo.FirstName;

                    if (!string.IsNullOrEmpty(updateData.UserInfo.LastName))
                        existingStudent.User.LastName = updateData.UserInfo.LastName;

                    if (!string.IsNullOrEmpty(updateData.UserInfo.Email))
                        existingStudent.User.Email = updateData.UserInfo.Email;

                    if (!string.IsNullOrEmpty(updateData.UserInfo.PhoneNumber))
                        existingStudent.User.PhoneNumber = updateData.UserInfo.PhoneNumber;

                    if (!string.IsNullOrEmpty(updateData.UserInfo.Address))
                        existingStudent.User.Address = updateData.UserInfo.Address;

                    // Note: User model doesn't have DateOfBirth and Gender fields
                    // These are handled in Student model directly

                    existingStudent.User.UpdatedAt = DateTime.UtcNow;
                }

                // Update student in database
                await _studentManager.UpdateAsync(existingStudent);

                // Update subjects if provided
                if (updateData.SubjectIds != null && updateData.SubjectIds.Any())
                {
                    await UpdateStudentSubjectsSimple(id, updateData.SubjectIds, updateData.AcademicYear);
                }

                // Update academic year data if provided
                if (!string.IsNullOrEmpty(updateData.AcademicYear))
                {
                    await UpdateStudentAcademicInfo(id, updateData.AcademicYear, updateData.CurrentGrade);
                }

                Console.WriteLine($"Student updated successfully: {id}");
                return Ok(new { message = "Student updated successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== COMPLETE UPDATE STUDENT ERROR ===");
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"=== END COMPLETE UPDATE STUDENT ERROR ===");
                return StatusCode(500, new { message = "Error updating student", error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        private async Task UpdateStudentSubjectsSimple(Guid studentId, List<Guid> subjectIds, string academicYear, List<Guid>? optionalSubjectIds = null)
        {
            await _studentManager.SetStudentSubjectsAsync(studentId, academicYear ?? DateTime.Now.Year.ToString(), subjectIds, optionalSubjectIds);
        }

        private async Task UpdateStudentAcademicInfo(Guid studentId, string academicYear, string currentGrade)
        {
            try
            {
                Console.WriteLine($"Updating academic info for student: {studentId}, year: {academicYear}");

                // Here you can add logic to update academic year specific information
                // For now, we'll just log it
                Console.WriteLine($"Academic year: {academicYear}, Grade: {currentGrade}");

                // You can extend this method to handle academic year specific data
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating academic info: {ex.Message}");
                throw;
            }
        }
    }
}
