using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces;
using sms.api.Interfaces.Shared;
using sms.api.Managers;

namespace sms.api.Controllers.Shared
{
    [ApiController]
    [Route("api/analytics")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsManager _analyticsManager;
        private readonly ApplicationDbContext _context;
        private readonly ITrafficMetricsService _trafficMetrics;

        public AnalyticsController(IAnalyticsManager analyticsManager, ApplicationDbContext context, ITrafficMetricsService trafficMetrics)
        {
            _analyticsManager = analyticsManager;
            _context = context;
            _trafficMetrics = trafficMetrics;
        }

        /// <summary>
        /// Get comprehensive dashboard analytics
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetDashboardAnalytics()
        {
            try
            {
                var analytics = await _analyticsManager.GetDashboardAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get student analytics
        /// </summary>
        [HttpGet("students")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetStudentAnalytics()
        {
            try
            {
                var analytics = await _analyticsManager.GetStudentAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get staff analytics
        /// </summary>
        [HttpGet("staff")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStaffAnalytics()
        {
            try
            {
                var analytics = await _analyticsManager.GetStaffAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get attendance analytics
        /// </summary>
        [HttpGet("attendance")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAttendanceAnalytics()
        {
            try
            {
                var analytics = await _analyticsManager.GetAttendanceAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get fee analytics
        /// </summary>
        [HttpGet("fees")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetFeeAnalytics()
        {
            try
            {
                var analytics = await _analyticsManager.GetFeeAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get exam analytics
        /// </summary>
        [HttpGet("exams")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetExamAnalytics()
        {
            try
            {
                var analytics = await _analyticsManager.GetExamAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get class analytics
        /// </summary>
        [HttpGet("classes")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetClassAnalytics()
        {
            try
            {
                var analytics = await _analyticsManager.GetClassAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get department analytics
        /// </summary>
        [HttpGet("departments")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetDepartmentAnalytics()
        {
            try
            {
                var analytics = await _analyticsManager.GetDepartmentAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get quick stats for dashboard cards
        /// </summary>
        [HttpGet("quick-stats")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetQuickStats()
        {
            try
            {
                var analytics = await _analyticsManager.GetDashboardAnalyticsAsync();

                var quickStats = new
                {
                    TotalStudents = analytics.Students.TotalStudents,
                    TotalStaff = analytics.Staff.TotalStaff,
                    TotalClasses = analytics.Classes.TotalClasses,
                    AttendancePercentage = analytics.Attendance.OverallAttendancePercentage,
                    FeeCollectionPercentage = analytics.Fees.CollectionPercentage,
                    TotalExams = analytics.Exams.TotalExams,
                    AverageScore = analytics.Exams.AverageScore,
                    ActiveDepartments = analytics.Departments.ActiveDepartments
                };

                return Ok(quickStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get analytics summary for mobile/compact view
        /// </summary>
        [HttpGet("summary")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAnalyticsSummary()
        {
            try
            {
                var analytics = await _analyticsManager.GetDashboardAnalyticsAsync();

                var summary = new
                {
                    Overview = new
                    {
                        TotalStudents = analytics.Students.TotalStudents,
                        TotalStaff = analytics.Staff.TotalStaff,
                        TotalClasses = analytics.Classes.TotalClasses,
                        TotalDepartments = analytics.Departments.TotalDepartments
                    },
                    Performance = new
                    {
                        AttendanceRate = analytics.Attendance.OverallAttendancePercentage,
                        FeeCollectionRate = analytics.Fees.CollectionPercentage,
                        AverageExamScore = analytics.Exams.AverageScore
                    },
                    Recent = new
                    {
                        NewAdmissions = analytics.Students.NewAdmissionsThisMonth,
                        NewStaffJoining = analytics.Staff.NewJoiningThisMonth,
                        UpcomingExams = analytics.Exams.UpcomingExams
                    }
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get system analytics (database size, table counts, environment)
        /// </summary>
        [HttpGet("system")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSystemAnalytics()
        {
            try
            {
                double databaseSizeMb = 0;
                try
                {
                    var sizeResult = await _context.Database
                        .SqlQueryRaw<double>("SELECT CAST(SUM(size * 8.0 / 1024) AS FLOAT) AS Value FROM sys.master_files WHERE database_id = DB_ID()")
                        .ToListAsync();
                    if (sizeResult.Count > 0) databaseSizeMb = sizeResult[0];
                }
                catch { /* non-SQL Server or permission */ }

                var tableCounts = new List<object>();
                long totalRows = 0;
                try
                {
                    var u = await _context.Users.CountAsync();
                    var s = await _context.Students.CountAsync();
                    var st = await _context.Staff.CountAsync();
                    var c = await _context.Classes.CountAsync();
                    var e = await _context.Exams.CountAsync();
                    var fp = await _context.FeePayments.CountAsync();
                    var car = await _context.ClassAttendanceRecords.CountAsync();
                    var a = await _context.Announcements.CountAsync();
                    var cm = await _context.ChatMessages.CountAsync();
                    var v = await _context.VisitorEntries.CountAsync();
                    tableCounts.Add(new { tableName = "Users", rowCount = u });
                    tableCounts.Add(new { tableName = "Students", rowCount = s });
                    tableCounts.Add(new { tableName = "Staff", rowCount = st });
                    tableCounts.Add(new { tableName = "Classes", rowCount = c });
                    tableCounts.Add(new { tableName = "Exams", rowCount = e });
                    tableCounts.Add(new { tableName = "FeePayments", rowCount = fp });
                    tableCounts.Add(new { tableName = "ClassAttendanceRecords", rowCount = car });
                    tableCounts.Add(new { tableName = "Announcements", rowCount = a });
                    tableCounts.Add(new { tableName = "ChatMessages", rowCount = cm });
                    tableCounts.Add(new { tableName = "VisitorEntries", rowCount = v });
                    totalRows = u + s + st + c + e + fp + car + a + cm + v;
                }
                catch { /* ignore */ }

                var result = new
                {
                    databaseSizeMb = Math.Round(databaseSizeMb, 2),
                    databaseSizeGb = Math.Round(databaseSizeMb / 1024.0, 2),
                    tableCounts,
                    totalRows,
                    environment = new
                    {
                        runtimeVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                        osDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                        serverTimeUtc = DateTime.UtcNow,
                        serverTimeLocal = DateTime.Now,
                    },
                    connectionInfo = new
                    {
                        providerName = _context.Database.ProviderName,
                        canConnect = await _context.Database.CanConnectAsync(),
                    },
                    traffic = _trafficMetrics.GetSnapshot()
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
