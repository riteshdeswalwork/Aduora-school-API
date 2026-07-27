using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Services;

/// <summary>
/// Polls host + traffic metrics and alerts admins on overload (with cooldown).
/// Triggers: RAM / CPU / Disk, error rate, avg response time (and optional active requests).
/// </summary>
public class SystemHealthAlertBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITrafficMetricsService _metrics;
    private readonly IHostResourceMetricsService _hostMetrics;
    private readonly NotificationSettings _settings;
    private readonly IHostEnvironment _env;
    private readonly ILogger<SystemHealthAlertBackgroundService> _logger;
    private DateTime _lastAlertUtc = DateTime.MinValue;

    public SystemHealthAlertBackgroundService(
        IServiceScopeFactory scopeFactory,
        ITrafficMetricsService metrics,
        IHostResourceMetricsService hostMetrics,
        IOptions<NotificationSettings> settings,
        IHostEnvironment env,
        ILogger<SystemHealthAlertBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _metrics = metrics;
        _hostMetrics = hostMetrics;
        _settings = settings.Value;
        _env = env;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndAlertAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System health alert check failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task CheckAndAlertAsync(CancellationToken ct)
    {
        var cooldown = TimeSpan.FromMinutes(Math.Max(1, _settings.SystemAlertCooldownMinutes));
        if (DateTime.UtcNow - _lastAlertUtc < cooldown)
            return;

        var m = _metrics.GetHealthMetrics();
        var host = await _hostMetrics.SampleAsync(ct);
        var reasons = new List<string>();

        // Host resources — do not require min request count
        if (_settings.SystemAlertCpuPercent > 0
            && host.CpuPercent is double cpu
            && cpu >= _settings.SystemAlertCpuPercent)
        {
            reasons.Add($"CPU {cpu:0.#}% (threshold {_settings.SystemAlertCpuPercent}%)");
        }

        if (_settings.SystemAlertRamPercent > 0
            && host.RamPercent is double ram
            && ram >= _settings.SystemAlertRamPercent)
        {
            reasons.Add($"RAM {ram:0.#}% (threshold {_settings.SystemAlertRamPercent}%)");
        }

        if (_settings.SystemAlertDiskPercent > 0
            && host.DiskPercent is double disk
            && disk >= _settings.SystemAlertDiskPercent)
        {
            var diskLabel = string.IsNullOrWhiteSpace(host.DiskRoot) ? "Disk" : $"Disk {host.DiskRoot}";
            reasons.Add($"{diskLabel} used {disk:0.#}% (threshold {_settings.SystemAlertDiskPercent}%)");
        }

        // Traffic-based — require enough samples
        var enoughTraffic = m.TotalRequests >= _settings.SystemAlertMinRequests;
        if (enoughTraffic)
        {
            if (m.ErrorRatePercent >= _settings.SystemAlertErrorRatePercent)
                reasons.Add($"Error rate {m.ErrorRatePercent}% (threshold {_settings.SystemAlertErrorRatePercent}%)");

            if (_settings.SystemAlertAvgResponseMs > 0
                && m.AverageResponseMs >= _settings.SystemAlertAvgResponseMs)
            {
                reasons.Add($"Avg response {m.AverageResponseMs:0} ms (threshold {_settings.SystemAlertAvgResponseMs} ms / {(_settings.SystemAlertAvgResponseMs / 1000.0):0.##}s)");
            }

            if (_settings.SystemAlertActiveRequestsThreshold > 0
                && m.ActiveRequests >= _settings.SystemAlertActiveRequestsThreshold)
            {
                reasons.Add($"Active requests {m.ActiveRequests} (threshold {_settings.SystemAlertActiveRequestsThreshold})");
            }

            if (m.Server5xx > 0 && m.ErrorRatePercent >= _settings.SystemAlertErrorRatePercent)
                reasons.Add($"5xx count {m.Server5xx}");
        }

        if (reasons.Count == 0)
            return;

        var detectedAtLocal = DateTime.Now;
        var detectedAtUtc = DateTime.UtcNow;
        var body = BuildPlainBody(m, host, reasons, detectedAtLocal, detectedAtUtc);
        var html = BuildHtmlEmail(m, host, reasons, detectedAtLocal, detectedAtUtc);
        var subject = BuildSubject(m, host, reasons);

        using var scope = _scopeFactory.CreateScope();
        var orch = scope.ServiceProvider.GetRequiredService<INotificationOrchestrator>();

        await orch.NotifyAsync(new NotificationRequest
        {
            UserIds = new List<Guid>(),
            IncludeAdmins = true,
            Title = "System overload / elevated errors",
            Body = body,
            SmsBody = Truncate($"ALERT: {string.Join("; ", reasons)}", 160),
            EmailSubject = subject,
            EmailHtmlBody = html,
            Type = "system.overload",
            Category = "system",
            Severity = "error",
            Channels = NotificationChannels.All,
            IdempotencyKey = $"system.overload:{DateTime.UtcNow:yyyyMMddHHmm}",
            Data = new Dictionary<string, string>
            {
                ["type"] = "system.overload",
                ["errorRate"] = m.ErrorRatePercent.ToString("0.##"),
                ["avgResponseMs"] = m.AverageResponseMs.ToString("0.##"),
                ["activeRequests"] = m.ActiveRequests.ToString(),
                ["cpuPercent"] = host.CpuPercent?.ToString("0.#") ?? "",
                ["ramPercent"] = host.RamPercent?.ToString("0.#") ?? "",
                ["diskPercent"] = host.DiskPercent?.ToString("0.#") ?? "",
                ["environment"] = _env.EnvironmentName
            }
        }, ct);

        _lastAlertUtc = DateTime.UtcNow;
        _logger.LogWarning("System health alert dispatched: {Reasons}", string.Join("; ", reasons));
    }

    private string BuildSubject(TrafficHealthMetrics m, HostResourceSnapshot host, List<string> reasons)
    {
        var parts = new List<string>();
        if (host.CpuPercent is double c && _settings.SystemAlertCpuPercent > 0 && c >= _settings.SystemAlertCpuPercent)
            parts.Add($"CPU {c:0.#}%");
        if (host.RamPercent is double r && _settings.SystemAlertRamPercent > 0 && r >= _settings.SystemAlertRamPercent)
            parts.Add($"RAM {r:0.#}%");
        if (host.DiskPercent is double d && _settings.SystemAlertDiskPercent > 0 && d >= _settings.SystemAlertDiskPercent)
            parts.Add($"Disk {d:0.#}%");
        if (m.ErrorRatePercent >= _settings.SystemAlertErrorRatePercent)
            parts.Add($"errors {m.ErrorRatePercent:0.##}%");
        if (_settings.SystemAlertAvgResponseMs > 0 && m.AverageResponseMs >= _settings.SystemAlertAvgResponseMs)
            parts.Add($"avg {m.AverageResponseMs:0}ms");
        if (parts.Count == 0)
            parts.Add(reasons[0]);
        return $"[{_env.EnvironmentName}] System overload — {string.Join(", ", parts.Take(3))}";
    }

    private string BuildPlainBody(
        TrafficHealthMetrics m,
        HostResourceSnapshot host,
        List<string> reasons,
        DateTime local,
        DateTime utc)
    {
        var sb = new StringBuilder();
        sb.AppendLine("School API health alert — system overload.");
        sb.AppendLine();
        sb.AppendLine("Why triggered:");
        foreach (var r in reasons)
            sb.AppendLine($"  • {r}");
        sb.AppendLine();
        sb.AppendLine("Host resources:");
        sb.AppendLine($"  • CPU: {(host.CpuPercent is double cp ? $"{cp:0.#}%" : "n/a")} (threshold {_settings.SystemAlertCpuPercent}%)");
        sb.AppendLine($"  • RAM: {(host.RamPercent is double rp ? $"{rp:0.#}% ({host.RamUsedMb} / {host.RamTotalMb} MB)" : "n/a")} (threshold {_settings.SystemAlertRamPercent}%)");
        sb.AppendLine($"  • Disk: {(host.DiskPercent is double dp ? $"{dp:0.#}% used on {host.DiskRoot} (free {host.DiskFreeGb} GB / {host.DiskTotalGb} GB)" : "n/a")} (threshold {_settings.SystemAlertDiskPercent}%)");
        sb.AppendLine($"  • Metrics source: {host.Source}");
        sb.AppendLine();
        sb.AppendLine("Traffic snapshot:");
        sb.AppendLine($"  • Error rate: {m.ErrorRatePercent}% (threshold {_settings.SystemAlertErrorRatePercent}%)");
        sb.AppendLine($"  • Avg response: {m.AverageResponseMs:0.##} ms (threshold {_settings.SystemAlertAvgResponseMs} ms)");
        sb.AppendLine($"  • Active requests: {m.ActiveRequests} (threshold {_settings.SystemAlertActiveRequestsThreshold})");
        sb.AppendLine($"  • Requests/min: {m.RequestsPerMinute} | last 5m: {m.RequestsLast5Minutes}");
        sb.AppendLine($"  • Total requests (since start): {m.TotalRequests} (min for traffic alerts: {_settings.SystemAlertMinRequests})");
        sb.AppendLine($"  • HTTP 2xx: {m.Success2xx} | 4xx: {m.Client4xx} | 5xx: {m.Server5xx} | other: {m.OtherStatus}");
        sb.AppendLine($"  • API uptime: {FormatUptime(m.UptimeSeconds)} (started {m.StartedAtUtc:yyyy-MM-dd HH:mm} UTC)");
        sb.AppendLine();
        if (m.TopRoutes.Count > 0)
        {
            sb.AppendLine("Top routes by traffic:");
            foreach (var route in m.TopRoutes.Take(5))
                sb.AppendLine($"  • {route.Route} — {route.Requests} req");
            sb.AppendLine();
        }
        sb.AppendLine("Environment:");
        sb.AppendLine($"  • Name: {_env.EnvironmentName}");
        sb.AppendLine($"  • Machine: {Environment.MachineName}");
        sb.AppendLine($"  • Detected: {local:yyyy-MM-dd HH:mm:ss} local / {utc:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"  • Cooldown: {_settings.SystemAlertCooldownMinutes} minutes");
        sb.AppendLine();
        sb.AppendLine("Suggested checks:");
        sb.AppendLine("  1. Check Task Manager / server resources (CPU, RAM, disk).");
        sb.AppendLine("  2. Open Analytics → System for live API traffic charts.");
        sb.AppendLine("  3. Inspect API logs for 5xx / slow endpoints.");
        sb.AppendLine("  4. Verify database health and heavy jobs (fees, attendance, notifications).");
        return sb.ToString().TrimEnd();
    }

    private string BuildHtmlEmail(
        TrafficHealthMetrics m,
        HostResourceSnapshot host,
        List<string> reasons,
        DateTime local,
        DateTime utc)
    {
        string E(string? s) => WebUtility.HtmlEncode(s ?? "");

        var reasonItems = string.Join("", reasons.Select(r =>
            $"<li style=\"margin:4px 0;\">{E(r)}</li>"));

        var routeRows = m.TopRoutes.Count == 0
            ? "<tr><td colspan=\"2\" style=\"padding:8px;color:#64748b;\">No route data yet</td></tr>"
            : string.Join("", m.TopRoutes.Select(r =>
                $"<tr><td style=\"padding:8px 10px;border-bottom:1px solid #e2e8f0;font-family:Consolas,monospace;font-size:13px;\">{E(r.Route)}</td>" +
                $"<td style=\"padding:8px 10px;border-bottom:1px solid #e2e8f0;text-align:right;font-weight:600;\">{r.Requests}</td></tr>"));

        var cpuText = host.CpuPercent is double cp ? $"{cp:0.#}%" : "n/a";
        var ramText = host.RamPercent is double rp ? $"{rp:0.#}%" : "n/a";
        var diskText = host.DiskPercent is double dp ? $"{dp:0.#}%" : "n/a";
        var ramSub = host.RamUsedMb is long used && host.RamTotalMb is long total
            ? $"{used} / {total} MB"
            : $"Threshold {_settings.SystemAlertRamPercent}%";
        var diskSub = !string.IsNullOrWhiteSpace(host.DiskRoot)
            ? $"{host.DiskRoot} free {host.DiskFreeGb} GB"
            : $"Threshold {_settings.SystemAlertDiskPercent}%";

        return $@"
<div style=""font-family:Segoe UI,Arial,sans-serif;max-width:640px;margin:0 auto;color:#0f172a;background:#f8fafc;padding:16px;"">
  <div style=""background:#fff;border:1px solid #e2e8f0;border-radius:12px;overflow:hidden;"">
    <div style=""background:#b91c1c;color:#fff;padding:16px 20px;"">
      <div style=""font-size:12px;letter-spacing:0.04em;text-transform:uppercase;opacity:0.9;"">Aduora School API</div>
      <h1 style=""margin:6px 0 0;font-size:20px;font-weight:700;"">System overload</h1>
      <p style=""margin:8px 0 0;font-size:13px;opacity:0.95;"">{E(_env.EnvironmentName)} · {E(Environment.MachineName)}</p>
    </div>

    <div style=""padding:20px;"">
      <p style=""margin:0 0 12px;font-size:14px;line-height:1.5;"">
        Automatic health check detected host resource pressure and/or elevated API errors / latency.
      </p>

      <h2 style=""margin:0 0 8px;font-size:14px;color:#b91c1c;"">Why this alert fired</h2>
      <ul style=""margin:0 0 16px;padding-left:18px;font-size:14px;"">{reasonItems}</ul>

      <h2 style=""margin:0 0 10px;font-size:14px;"">Host resources</h2>
      <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""border-collapse:collapse;margin-bottom:16px;"">
        <tr>
          {MetricCell("CPU", cpuText, $"Threshold {_settings.SystemAlertCpuPercent}%")}
          {MetricCell("RAM", ramText, ramSub)}
        </tr>
        <tr>
          {MetricCell("Disk used", diskText, diskSub)}
          {MetricCell("Avg response", $"{m.AverageResponseMs:0} ms", $"Threshold {_settings.SystemAlertAvgResponseMs} ms")}
        </tr>
      </table>

      <h2 style=""margin:0 0 10px;font-size:14px;"">API traffic</h2>
      <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""border-collapse:collapse;margin-bottom:16px;"">
        <tr>
          {MetricCell("Error rate", $"{m.ErrorRatePercent:0.##}%", $"Threshold {_settings.SystemAlertErrorRatePercent}%")}
          {MetricCell("Active req", m.ActiveRequests.ToString(), $"Threshold {_settings.SystemAlertActiveRequestsThreshold}")}
        </tr>
        <tr>
          {MetricCell("Req / min", m.RequestsPerMinute.ToString(), $"Last 5m: {m.RequestsLast5Minutes}")}
          {MetricCell("Total req", m.TotalRequests.ToString(), $"Min for traffic alerts: {_settings.SystemAlertMinRequests}")}
        </tr>
        <tr>
          {MetricCell("HTTP 5xx", m.Server5xx.ToString(), $"4xx: {m.Client4xx}")}
          {MetricCell("Uptime", FormatUptime(m.UptimeSeconds), $"Started {m.StartedAtUtc:yyyy-MM-dd HH:mm} UTC")}
        </tr>
      </table>

      <h2 style=""margin:0 0 8px;font-size:14px;"">Status mix</h2>
      <p style=""margin:0 0 16px;font-size:13px;color:#334155;"">
        <span style=""color:#059669;font-weight:600;"">2xx {m.Success2xx}</span> ·
        <span style=""color:#d97706;font-weight:600;"">4xx {m.Client4xx}</span> ·
        <span style=""color:#dc2626;font-weight:600;"">5xx {m.Server5xx}</span> ·
        <span style=""color:#64748b;font-weight:600;"">other {m.OtherStatus}</span>
      </p>

      <h2 style=""margin:0 0 8px;font-size:14px;"">Top routes by traffic</h2>
      <table width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""border:1px solid #e2e8f0;border-radius:8px;border-collapse:collapse;margin-bottom:16px;"">
        <thead>
          <tr style=""background:#f1f5f9;text-align:left;font-size:12px;color:#475569;"">
            <th style=""padding:8px 10px;"">Route</th>
            <th style=""padding:8px 10px;text-align:right;"">Requests</th>
          </tr>
        </thead>
        <tbody>{routeRows}</tbody>
      </table>

      <h2 style=""margin:0 0 8px;font-size:14px;"">Alert context</h2>
      <table style=""font-size:13px;color:#334155;margin-bottom:16px;"">
        <tr><td style=""padding:3px 12px 3px 0;color:#64748b;"">Detected (local)</td><td>{E(local.ToString("yyyy-MM-dd HH:mm:ss"))}</td></tr>
        <tr><td style=""padding:3px 12px 3px 0;color:#64748b;"">Detected (UTC)</td><td>{E(utc.ToString("yyyy-MM-dd HH:mm:ss"))}</td></tr>
        <tr><td style=""padding:3px 12px 3px 0;color:#64748b;"">Environment</td><td>{E(_env.EnvironmentName)}</td></tr>
        <tr><td style=""padding:3px 12px 3px 0;color:#64748b;"">Machine</td><td>{E(Environment.MachineName)}</td></tr>
        <tr><td style=""padding:3px 12px 3px 0;color:#64748b;"">Host metrics</td><td>{E(host.Source)}</td></tr>
        <tr><td style=""padding:3px 12px 3px 0;color:#64748b;"">Cooldown</td><td>{_settings.SystemAlertCooldownMinutes} minutes</td></tr>
      </table>

      <h2 style=""margin:0 0 8px;font-size:14px;"">What to check next</h2>
      <ol style=""margin:0;padding-left:18px;font-size:13px;line-height:1.55;color:#334155;"">
        <li>Check server CPU, RAM, and disk free space.</li>
        <li>Open <strong>Analytics → System</strong> for live API charts.</li>
        <li>Review API logs for 5xx / slow endpoints.</li>
        <li>Check database load and heavy background jobs.</li>
      </ol>
    </div>

    <div style=""padding:12px 20px;background:#f8fafc;border-top:1px solid #e2e8f0;font-size:11px;color:#94a3b8;"">
      Automatic alert from Aduora School Management System health monitor.
    </div>
  </div>
</div>".Trim();
    }

    private static string MetricCell(string label, string value, string sub)
    {
        return $@"
<td style=""width:50%;padding:6px;vertical-align:top;"">
  <div style=""border:1px solid #e2e8f0;border-radius:10px;padding:12px;background:#f8fafc;"">
    <div style=""font-size:11px;color:#64748b;text-transform:uppercase;letter-spacing:0.03em;"">{WebUtility.HtmlEncode(label)}</div>
    <div style=""font-size:20px;font-weight:700;margin-top:4px;color:#0f172a;"">{WebUtility.HtmlEncode(value)}</div>
    <div style=""font-size:11px;color:#94a3b8;margin-top:4px;"">{WebUtility.HtmlEncode(sub)}</div>
  </div>
</td>";
    }

    private static string FormatUptime(long secs)
    {
        if (secs < 0) secs = 0;
        var days = secs / 86400;
        var hours = (secs % 86400) / 3600;
        var mins = (secs % 3600) / 60;
        if (days > 0) return $"{days}d {hours}h {mins}m";
        if (hours > 0) return $"{hours}h {mins}m";
        return $"{mins}m";
    }

    private static string Truncate(string s, int max) =>
        string.IsNullOrEmpty(s) ? "" : (s.Length <= max ? s : s[..(max - 1)] + "…");
}
