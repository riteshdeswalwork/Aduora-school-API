using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using sms.api.Data;
using sms.api.Interfaces.Student;
using sms.api.Models;

namespace sms.api.Services
{
    /// <summary>
    /// On the 1st of each month (UTC), creates missing monthly fee demands for the previous calendar month
    /// for every active academic year that contains that month.
    /// </summary>
    public class MonthlyFeeDemandBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonthlyFeeDemandBackgroundService> _logger;

        public MonthlyFeeDemandBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<MonthlyFeeDemandBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MonthlyFeeDemandBackgroundService started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var nextMidnightUtc = now.Date.AddDays(1);
                    var delay = nextMidnightUtc - now;
                    if (delay < TimeSpan.FromSeconds(10))
                        delay = TimeSpan.FromSeconds(10);
                    await Task.Delay(delay, stoppingToken);
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    var istDate = GetIndiaCalendarDate(DateTime.UtcNow);
                    if (istDate.Day != 1)
                        continue;

                    var prev = istDate.AddMonths(-1);
                    var y = prev.Year;
                    var month = prev.Month;

                    using var scope = _scopeFactory.CreateScope();
                    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var feeMgr = scope.ServiceProvider.GetRequiredService<IStudentFeeManager>();

                    var years = await ctx.Set<AcademicYear>().AsNoTracking()
                        .Where(a => a.IsActive)
                        .ToListAsync(stoppingToken);

                    foreach (var ay in years)
                    {
                        var first = new DateTime(y, month, 1);
                        var last = new DateTime(y, month, DateTime.DaysInMonth(y, month));
                        if (last.Date < ay.StartDate.Date || first.Date > ay.EndDate.Date)
                            continue;

                        try
                        {
                            var result = await feeMgr.GenerateMissingMonthlyDemandsAsync(ay.Id, y, month);
                            _logger.LogInformation(
                                "Auto monthly demands: AY {YearCode} {Y}-{M}: created {C}, skipped {S}",
                                ay.YearCode, y, month, result.Created, result.SkippedExisting);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Auto monthly demands failed for academic year {Id}", ay.Id);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MonthlyFeeDemandBackgroundService loop error");
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        private static DateTime GetIndiaCalendarDate(DateTime utcNow)
        {
            try
            {
                var tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "India Standard Time"
                    : "Asia/Kolkata";
                var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                return TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz).Date;
            }
            catch
            {
                return utcNow.Date;
            }
        }
    }
}
