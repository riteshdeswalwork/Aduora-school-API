using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces;
using sms.api.Interfaces.Shared;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [ApiController]
    [Route("api/analytics")]
    [Authorize]
    public class AnalyticsController : ApiControllerBase
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

        private async Task<IEnumerable<Guid>?> GetAllowedClassIdsForAnalyticsAsync()
        {
            var scope = await GetDataScopeAsync();
            return DataScopeHelper.AllowedClassIdsForQuery(scope);
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
                var allowed = await GetAllowedClassIdsForAnalyticsAsync();
                var analytics = await _analyticsManager.GetDashboardAnalyticsAsync(allowed);
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
                var allowed = await GetAllowedClassIdsForAnalyticsAsync();
                var analytics = await _analyticsManager.GetStudentAnalyticsAsync(allowed);
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
                var allowed = await GetAllowedClassIdsForAnalyticsAsync();
                var analytics = await _analyticsManager.GetAttendanceAnalyticsAsync(allowed);
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
                var allowed = await GetAllowedClassIdsForAnalyticsAsync();
                var analytics = await _analyticsManager.GetFeeAnalyticsAsync(allowed);
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
                var allowed = await GetAllowedClassIdsForAnalyticsAsync();
                var analytics = await _analyticsManager.GetExamAnalyticsAsync(allowed);
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
                var allowed = await GetAllowedClassIdsForAnalyticsAsync();
                var analytics = await _analyticsManager.GetClassAnalyticsAsync(allowed);
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
                var allowed = await GetAllowedClassIdsForAnalyticsAsync();
                var analytics = await _analyticsManager.GetDashboardAnalyticsAsync(allowed);

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
                var allowed = await GetAllowedClassIdsForAnalyticsAsync();
                var analytics = await _analyticsManager.GetDashboardAnalyticsAsync(allowed);

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
                string databaseName = _context.Database.GetDbConnection().Database ?? "—";
                double databaseSizeMb = 0;
                double dataFileMb = 0;
                double logFileMb = 0;
                double usedSpaceMb = 0;
                double unusedSpaceMb = 0;
                var fileBreakdown = new List<object>();

                try
                {
                    var conn = _context.Database.GetDbConnection();
                    if (conn.State != System.Data.ConnectionState.Open)
                        await conn.OpenAsync();

                    // Allocated size by file (data vs log)
                    await using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
SELECT
  name,
  type_desc,
  CAST(size * 8.0 / 1024 AS FLOAT) AS SizeMb
FROM sys.database_files
ORDER BY type_desc, name;";
                        await using var reader = await cmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            var name = reader.GetString(0);
                            var typeDesc = reader.GetString(1);
                            var sizeMb = reader.GetDouble(2);
                            fileBreakdown.Add(new { name, type = typeDesc, sizeMb = Math.Round(sizeMb, 2) });
                            if (string.Equals(typeDesc, "ROWS", StringComparison.OrdinalIgnoreCase))
                                dataFileMb += sizeMb;
                            else if (string.Equals(typeDesc, "LOG", StringComparison.OrdinalIgnoreCase))
                                logFileMb += sizeMb;
                            databaseSizeMb += sizeMb;
                        }
                    }

                    // Used vs unused within data file (approx)
                    await using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
SELECT
  CAST(SUM(a.total_pages) * 8.0 / 1024 AS FLOAT) AS ReservedMb,
  CAST(SUM(a.used_pages) * 8.0 / 1024 AS FLOAT) AS UsedMb
FROM sys.allocation_units a;";
                        await using var reader = await cmd.ExecuteReaderAsync();
                        if (await reader.ReadAsync())
                        {
                            var reserved = reader.IsDBNull(0) ? 0 : reader.GetDouble(0);
                            usedSpaceMb = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);
                            unusedSpaceMb = Math.Max(0, reserved - usedSpaceMb);
                        }
                    }
                }
                catch
                {
                    /* non-SQL Server or permission — keep zeros */
                }

                var tableCounts = new List<object>();
                long totalRows = 0;
                try
                {
                    var conn = _context.Database.GetDbConnection();
                    if (conn.State != System.Data.ConnectionState.Open)
                        await conn.OpenAsync();

                    // Top user tables by row count + approx size (MB)
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
SELECT TOP 25
  t.name AS TableName,
  SUM(CASE WHEN p.index_id IN (0, 1) THEN p.rows ELSE 0 END) AS [RowCnt],
  CAST(SUM(a.total_pages) * 8.0 / 1024 AS FLOAT) AS SizeMb
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.is_ms_shipped = 0
GROUP BY t.name
ORDER BY [RowCnt] DESC, SizeMb DESC;";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var tableName = reader.GetString(0);
                        var rowCount = reader.IsDBNull(1) ? 0L : Convert.ToInt64(reader.GetValue(1));
                        var sizeMb = reader.IsDBNull(2) ? 0d : Convert.ToDouble(reader.GetValue(2));
                        tableCounts.Add(new
                        {
                            tableName,
                            rowCount,
                            sizeMb = Math.Round(sizeMb, 2)
                        });
                        totalRows += rowCount;
                    }
                }
                catch
                {
                    // Fallback: sample known entity sets
                    try
                    {
                        var u = await _context.Users.CountAsync();
                        var s = await _context.Students.CountAsync();
                        var st = await _context.Staff.CountAsync();
                        var c = await _context.Classes.CountAsync();
                        var e = await _context.Exams.CountAsync();
                        var fp = await _context.FeePayments.CountAsync();
                        tableCounts.Add(new { tableName = "Users", rowCount = (long)u, sizeMb = 0d });
                        tableCounts.Add(new { tableName = "Students", rowCount = (long)s, sizeMb = 0d });
                        tableCounts.Add(new { tableName = "Staff", rowCount = (long)st, sizeMb = 0d });
                        tableCounts.Add(new { tableName = "Classes", rowCount = (long)c, sizeMb = 0d });
                        tableCounts.Add(new { tableName = "Exams", rowCount = (long)e, sizeMb = 0d });
                        tableCounts.Add(new { tableName = "FeePayments", rowCount = (long)fp, sizeMb = 0d });
                        totalRows = u + s + st + c + e + fp;
                    }
                    catch { /* ignore */ }
                }

                var result = new
                {
                    databaseName,
                    databaseSizeMb = Math.Round(databaseSizeMb, 2),
                    databaseSizeGb = Math.Round(databaseSizeMb / 1024.0, 3),
                    dataFileMb = Math.Round(dataFileMb, 2),
                    logFileMb = Math.Round(logFileMb, 2),
                    usedSpaceMb = Math.Round(usedSpaceMb, 2),
                    unusedSpaceMb = Math.Round(unusedSpaceMb, 2),
                    fileBreakdown,
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
                        databaseName,
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
