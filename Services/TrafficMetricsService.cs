using System.Collections.Concurrent;
using sms.api.Interfaces.Shared;

namespace sms.api.Services;

public class TrafficMetricsService : ITrafficMetricsService
{
    private readonly DateTime _startedAtUtc = DateTime.UtcNow;
    private readonly ConcurrentQueue<DateTime> _recentRequests = new();
    private readonly ConcurrentDictionary<string, long> _pathCounters = new(StringComparer.OrdinalIgnoreCase);

    private long _totalRequests;
    private long _activeRequests;
    private long _success2xx;
    private long _client4xx;
    private long _server5xx;
    private long _otherStatus;
    private long _totalDurationMs;

    public void MarkRequestStarted()
    {
        Interlocked.Increment(ref _activeRequests);
    }

    public void MarkRequestCompleted(string path, int statusCode, double durationMs)
    {
        Interlocked.Decrement(ref _activeRequests);
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Add(ref _totalDurationMs, (long)Math.Round(Math.Max(0, durationMs)));

        if (statusCode >= 200 && statusCode < 300)
            Interlocked.Increment(ref _success2xx);
        else if (statusCode >= 400 && statusCode < 500)
            Interlocked.Increment(ref _client4xx);
        else if (statusCode >= 500)
            Interlocked.Increment(ref _server5xx);
        else
            Interlocked.Increment(ref _otherStatus);

        var normalizedPath = NormalizePath(path);
        _pathCounters.AddOrUpdate(normalizedPath, 1, (_, current) => current + 1);

        _recentRequests.Enqueue(DateTime.UtcNow);
        PruneRecentRequests();
    }

    public object GetSnapshot()
    {
        PruneRecentRequests();

        var now = DateTime.UtcNow;
        var recent = _recentRequests.ToArray();
        var perMinute = recent.Count(t => t >= now.AddMinutes(-1));
        var per5Minutes = recent.Count(t => t >= now.AddMinutes(-5));
        var totalRequests = Interlocked.Read(ref _totalRequests);
        var avgResponseMs = totalRequests > 0 ? Math.Round((double)Interlocked.Read(ref _totalDurationMs) / totalRequests, 2) : 0;
        var success2xx = Interlocked.Read(ref _success2xx);
        var client4xx = Interlocked.Read(ref _client4xx);
        var server5xx = Interlocked.Read(ref _server5xx);
        var otherStatus = Interlocked.Read(ref _otherStatus);
        var errorRate = totalRequests > 0 ? Math.Round(((double)(client4xx + server5xx) / totalRequests) * 100, 2) : 0;

        return new
        {
            startedAtUtc = _startedAtUtc,
            uptimeSeconds = (long)(now - _startedAtUtc).TotalSeconds,
            activeRequests = Interlocked.Read(ref _activeRequests),
            totalRequests,
            requestsPerMinute = perMinute,
            requestsLast5Minutes = per5Minutes,
            averageResponseMs = avgResponseMs,
            errorRatePercent = errorRate,
            statusBreakdown = new
            {
                success2xx,
                client4xx,
                server5xx,
                other = otherStatus
            },
            topRoutes = _pathCounters
                .OrderByDescending(kv => kv.Value)
                .Take(8)
                .Select(kv => new { route = kv.Key, requests = kv.Value })
                .ToList(),
            minuteTrend = BuildMinuteTrend(now, recent)
        };
    }

    private void PruneRecentRequests()
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-10);
        while (_recentRequests.TryPeek(out var head) && head < cutoff)
            _recentRequests.TryDequeue(out _);
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var normalized = path.Trim();
        if (!normalized.StartsWith('/'))
            normalized = "/" + normalized;

        return normalized.Length > 80 ? normalized[..80] + "..." : normalized;
    }

    private static List<object> BuildMinuteTrend(DateTime now, DateTime[] recent)
    {
        var baseMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc).AddMinutes(-9);
        var result = new List<object>(10);
        for (var i = 0; i < 10; i++)
        {
            var minuteStart = baseMinute.AddMinutes(i);
            var minuteEnd = minuteStart.AddMinutes(1);
            var count = recent.Count(t => t >= minuteStart && t < minuteEnd);
            result.Add(new
            {
                minute = minuteStart.ToLocalTime().ToString("HH:mm"),
                requests = count
            });
        }
        return result;
    }
}
