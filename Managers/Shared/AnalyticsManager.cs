using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models.DTOs;

namespace sms.api.Managers.Shared
{

    public class AnalyticsManager : IAnalyticsManager
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardAnalyticsDTO> GetDashboardAnalyticsAsync(IEnumerable<Guid>? allowedClassIds = null)
        {
            var analytics = new DashboardAnalyticsDTO
            {
                Students = await GetStudentAnalyticsAsync(allowedClassIds),
                Staff = await GetStaffAnalyticsAsync(),
                Attendance = await GetAttendanceAnalyticsAsync(allowedClassIds),
                Fees = await GetFeeAnalyticsAsync(allowedClassIds),
                Exams = await GetExamAnalyticsAsync(allowedClassIds),
                Classes = await GetClassAnalyticsAsync(allowedClassIds),
                Departments = await GetDepartmentAnalyticsAsync()
            };

            return analytics;
        }

        public async Task<StudentAnalyticsDTO> GetStudentAnalyticsAsync(IEnumerable<Guid>? allowedClassIds = null)
        {
            var allowed = allowedClassIds?.ToHashSet();
            HashSet<Guid>? allowedStudentIds = null;
            if (allowed != null)
            {
                allowedStudentIds = (await _context.ClassEnrollments
                    .AsNoTracking()
                    .Where(ce => allowed.Contains(ce.ClassId))
                    .Select(ce => ce.StudentId)
                    .Distinct()
                    .ToListAsync()).ToHashSet();
            }

            var studentsQuery = _context.Students.AsNoTracking().Include(s => s.User).AsQueryable();
            if (allowedStudentIds != null)
                studentsQuery = studentsQuery.Where(s => allowedStudentIds.Contains(s.Id));

            var students = await studentsQuery.ToListAsync();
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var analytics = new StudentAnalyticsDTO
            {
                TotalStudents = students.Count,
                ActiveStudents = students.Count(s => s.IsActive),
                InactiveStudents = students.Count(s => !s.IsActive),
                NewAdmissionsThisMonth = students.Count(s => s.AdmissionDate.Month == currentMonth && s.AdmissionDate.Year == currentYear)
            };

            var classWiseQuery = _context.ClassEnrollments
                .AsNoTracking()
                .Include(ce => ce.Class)
                .AsQueryable();
            if (allowed != null)
                classWiseQuery = classWiseQuery.Where(ce => allowed.Contains(ce.ClassId));

            var classWiseData = await classWiseQuery
                .GroupBy(ce => ce.Class.Name)
                .Select(g => new ClassWiseStudentCountDTO
                {
                    ClassName = g.Key,
                    StudentCount = g.Count(),
                    Color = GetRandomColor()
                })
                .ToListAsync();
            analytics.ClassWiseDistribution = classWiseData;

            var genderData = students
                .GroupBy(s => s.Gender)
                .Select(g => new GenderWiseCountDTO
                {
                    Gender = g.Key,
                    Count = g.Count(),
                    Color = g.Key == "Male" ? "#3498db" : g.Key == "Female" ? "#e74c3c" : "#95a5a6"
                })
                .ToList();
            analytics.GenderDistribution = genderData;

            var monthlyAdmissions = new List<MonthlyAdmissionDTO>();
            for (int i = 11; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var count = students.Count(s => s.AdmissionDate.Month == date.Month && s.AdmissionDate.Year == date.Year);
                monthlyAdmissions.Add(new MonthlyAdmissionDTO
                {
                    Month = date.ToString("MMM yyyy"),
                    Count = count
                });
            }
            analytics.MonthlyAdmissions = monthlyAdmissions;

            return analytics;
        }

        public async Task<StaffAnalyticsDTO> GetStaffAnalyticsAsync()
        {
            var staff = await _context.Staff
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Designation)
                    .ThenInclude(d => d.Department)
                .ToListAsync();

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var analytics = new StaffAnalyticsDTO
            {
                TotalStaff = staff.Count,
                ActiveStaff = staff.Count(s => s.IsActive),
                InactiveStaff = staff.Count(s => !s.IsActive),
                NewJoiningThisMonth = staff.Count(s => s.DateOfJoining.Month == currentMonth && s.DateOfJoining.Year == currentYear)
            };

            var departmentData = staff
                .Where(s => s.Designation?.Department != null)
                .GroupBy(s => s.Designation!.Department!.Name)
                .Select(g => new DepartmentWiseStaffCountDTO
                {
                    DepartmentName = g.Key,
                    StaffCount = g.Count(),
                    Color = GetRandomColor()
                })
                .ToList();
            analytics.DepartmentWiseDistribution = departmentData;

            var designationData = staff
                .Where(s => s.Designation != null)
                .GroupBy(s => s.Designation!.Name)
                .Select(g => new DesignationWiseCountDTO
                {
                    Designation = g.Key,
                    Count = g.Count(),
                    Color = GetRandomColor()
                })
                .ToList();
            analytics.DesignationDistribution = designationData;

            var salaryRanges = new[]
            {
                new { Range = "Below 30K", Min = 0m, Max = 30000m },
                new { Range = "30K - 50K", Min = 30000m, Max = 50000m },
                new { Range = "50K - 75K", Min = 50000m, Max = 75000m },
                new { Range = "75K - 1L", Min = 75000m, Max = 100000m },
                new { Range = "Above 1L", Min = 100000m, Max = decimal.MaxValue }
            };

            var salaryData = salaryRanges.Select(range => new SalaryRangeCountDTO
            {
                SalaryRange = range.Range,
                Count = staff.Count(s => s.Salary >= range.Min && s.Salary < range.Max),
                Color = GetRandomColor()
            }).ToList();
            analytics.SalaryDistribution = salaryData;

            return analytics;
        }

        public async Task<AttendanceAnalyticsDTO> GetAttendanceAnalyticsAsync(IEnumerable<Guid>? allowedClassIds = null)
        {
            var analytics = new AttendanceAnalyticsDTO();
            var allowed = allowedClassIds?.ToHashSet();

            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var attendanceQuery = _context.ClassAttendanceRecords
                .AsNoTracking()
                .Include(ar => ar.ClassAttendance)
                .Where(ar => ar.ClassAttendance.Date >= thirtyDaysAgo);

            if (allowed != null)
                attendanceQuery = attendanceQuery.Where(ar => allowed.Contains(ar.ClassAttendance.ClassId));

            var attendanceRecords = await attendanceQuery.ToListAsync();

            if (attendanceRecords.Any())
            {
                var totalRecords = attendanceRecords.Count;
                var presentRecords = attendanceRecords.Count(ar => ar.IsPresent);
                analytics.OverallAttendancePercentage = Math.Round((double)presentRecords / totalRecords * 100, 2);
                analytics.StudentAttendancePercentage = analytics.OverallAttendancePercentage;
                analytics.StaffAttendancePercentage = 85.5;
            }

            var weeklyData = new List<DailyAttendanceDTO>();
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i);
                var dayRecords = attendanceRecords.Where(ar => ar.ClassAttendance.Date.Date == date.Date);
                var percentage = dayRecords.Any() ?
                    Math.Round((double)dayRecords.Count(ar => ar.IsPresent) / dayRecords.Count() * 100, 2) : 0;

                weeklyData.Add(new DailyAttendanceDTO
                {
                    Day = date.ToString("ddd"),
                    AttendancePercentage = percentage
                });
            }
            analytics.WeeklyAttendance = weeklyData;

            return analytics;
        }

        public async Task<FeeAnalyticsDTO> GetFeeAnalyticsAsync(IEnumerable<Guid>? allowedClassIds = null)
        {
            var allowed = allowedClassIds?.ToHashSet();
            var feesQuery = _context.StudentFees
                .AsNoTracking()
                .Include(sf => sf.FeeStructure)
                .AsQueryable();

            if (allowed != null)
                feesQuery = feesQuery.Where(sf => sf.FeeStructure != null && allowed.Contains(sf.FeeStructure.ClassId));

            var studentFees = await feesQuery.ToListAsync();

            var analytics = new FeeAnalyticsDTO();

            if (studentFees.Count > 0)
            {
                analytics.TotalFeeAmount = studentFees.Sum(sf => sf.TotalAmount);
                analytics.TotalFeePending = studentFees.Sum(sf => sf.OutstandingAmount);

                var feeIds = studentFees.Select(sf => sf.Id).ToHashSet();
                analytics.TotalFeeCollected = await _context.FeePayments
                    .AsNoTracking()
                    .Where(p => !p.IsVoided && feeIds.Contains(p.StudentFeeId))
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                analytics.CollectionPercentage = analytics.TotalFeeAmount > 0
                    ? (double)Math.Round(100m * analytics.TotalFeeCollected / analytics.TotalFeeAmount, 2)
                    : 0;
            }

            var monthlyCollection = new List<MonthlyFeeCollectionDTO>();
            for (var i = 11; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var y = date.Year;
                var mo = date.Month;

                decimal collected;
                if (allowed != null)
                {
                    var feeIds = studentFees.Select(sf => sf.Id).ToHashSet();
                    collected = await _context.FeePayments
                        .AsNoTracking()
                        .Where(p => !p.IsVoided && feeIds.Contains(p.StudentFeeId) && p.PaymentDate.Year == y && p.PaymentDate.Month == mo)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0;
                }
                else
                {
                    collected = await _context.FeePayments
                        .AsNoTracking()
                        .Where(p => !p.IsVoided && p.PaymentDate.Year == y && p.PaymentDate.Month == mo)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0;
                }

                var inMonth = studentFees.Where(sf =>
                    (sf.BillingPeriodYear == y && sf.BillingPeriodMonth == mo)
                    || (sf.BillingPeriodYear == null && sf.DueDate.Year == y && sf.DueDate.Month == mo)).ToList();
                var billed = inMonth.Sum(sf => sf.TotalAmount);
                var pending = inMonth.Sum(sf => sf.OutstandingAmount);

                monthlyCollection.Add(new MonthlyFeeCollectionDTO
                {
                    Month = date.ToString("MMM yyyy"),
                    Collected = collected,
                    Pending = pending,
                    Billed = billed
                });
            }
            analytics.MonthlyCollection = monthlyCollection;

            return analytics;
        }

        public async Task<ExamAnalyticsDTO> GetExamAnalyticsAsync(IEnumerable<Guid>? allowedClassIds = null)
        {
            var allowed = allowedClassIds?.ToHashSet();
            var examsQuery = _context.Exams
                .AsNoTracking()
                .Include(e => e.ExamResults)
                .Include(e => e.Subject)
                .AsQueryable();

            // Exams may not have ClassId; filter results by enrolled students when scoped
            var exams = await examsQuery.ToListAsync();

            if (allowed != null)
            {
                var allowedStudentIds = (await _context.ClassEnrollments
                    .AsNoTracking()
                    .Where(ce => allowed.Contains(ce.ClassId))
                    .Select(ce => ce.StudentId)
                    .Distinct()
                    .ToListAsync()).ToHashSet();

                foreach (var exam in exams)
                {
                    exam.ExamResults = exam.ExamResults
                        .Where(r => allowedStudentIds.Contains(r.StudentId))
                        .ToList();
                }
            }

            var analytics = new ExamAnalyticsDTO
            {
                TotalExams = exams.Count,
                CompletedExams = exams.Count(e => e.ExamDate < DateTime.Now),
                UpcomingExams = exams.Count(e => e.ExamDate >= DateTime.Now)
            };

            var allResults = exams.SelectMany(e => e.ExamResults).ToList();
            if (allResults.Any())
            {
                analytics.AverageScore = Math.Round(allResults.Average(r => r.MarksObtained), 2);
            }

            var subjectPerformance = exams
                .Where(e => e.ExamResults.Any())
                .GroupBy(e => e.Subject.Name)
                .Select(g => new SubjectWisePerformanceDTO
                {
                    SubjectName = g.Key,
                    AverageScore = Math.Round(g.SelectMany(e => e.ExamResults).Average(r => r.MarksObtained), 2),
                    TotalStudents = g.SelectMany(e => e.ExamResults).Count()
                })
                .ToList();
            analytics.SubjectWisePerformance = subjectPerformance;

            return analytics;
        }

        public async Task<ClassAnalyticsDTO> GetClassAnalyticsAsync(IEnumerable<Guid>? allowedClassIds = null)
        {
            var allowed = allowedClassIds?.ToHashSet();
            var classesQuery = _context.Classes.AsNoTracking().Include(c => c.Enrollments).AsQueryable();
            if (allowed != null)
                classesQuery = classesQuery.Where(c => allowed.Contains(c.Id));

            var classes = await classesQuery.ToListAsync();

            var analytics = new ClassAnalyticsDTO
            {
                TotalClasses = classes.Count,
                ActiveClasses = classes.Count(c => c.IsActive),
                AverageStudentsPerClass = classes.Any() ? Math.Round((double)classes.Sum(c => c.Enrollments.Count) / classes.Count, 2) : 0
            };

            var capacityData = classes.Select(c => new ClassCapacityUtilizationDTO
            {
                ClassName = c.Name,
                Capacity = c.Capacity,
                CurrentStudents = c.Enrollments.Count,
                UtilizationPercentage = c.Capacity > 0 ? Math.Round((double)c.Enrollments.Count / c.Capacity * 100, 2) : 0
            }).ToList();
            analytics.ClassCapacityUtilization = capacityData;

            return analytics;
        }

        public async Task<DepartmentAnalyticsDTO> GetDepartmentAnalyticsAsync()
        {
            var departments = await _context.Departments
                .AsNoTracking()
                .Include(d => d.Staff)
                .Include(d => d.Subjects)
                .ToListAsync();

            var analytics = new DepartmentAnalyticsDTO
            {
                TotalDepartments = departments.Count,
                ActiveDepartments = departments.Count(d => d.IsActive)
            };

            var departmentStaffData = departments.Select(d => new DepartmentWiseStaffCountDTO
            {
                DepartmentName = d.Name,
                StaffCount = d.Staff.Count,
                Color = GetRandomColor()
            }).ToList();
            analytics.DepartmentWiseStaff = departmentStaffData;

            var departmentSubjectData = departments.Select(d => new DepartmentWiseSubjectCountDTO
            {
                DepartmentName = d.Name,
                SubjectCount = d.Subjects.Count,
                Color = GetRandomColor()
            }).ToList();
            analytics.DepartmentWiseSubjects = departmentSubjectData;

            return analytics;
        }

        private string GetRandomColor()
        {
            var colors = new[]
            {
                "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF",
                "#FF9F40", "#FF6384", "#C9CBCF", "#4BC0C0", "#FF6384"
            };
            var random = new Random();
            return colors[random.Next(colors.Length)];
        }
    }
}
