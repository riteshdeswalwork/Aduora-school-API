using System.Collections.Generic;

namespace sms.api.Interfaces.Shared;

public class TrafficRouteCount
{
    public string Route { get; set; } = "";
    public long Requests { get; set; }
}

public class TrafficHealthMetrics
{
    public long ActiveRequests { get; set; }
    public long TotalRequests { get; set; }
    public long Success2xx { get; set; }
    public long Server5xx { get; set; }
    public long Client4xx { get; set; }
    public long OtherStatus { get; set; }
    public double ErrorRatePercent { get; set; }
    public int RequestsPerMinute { get; set; }
    public int RequestsLast5Minutes { get; set; }
    public double AverageResponseMs { get; set; }
    public long UptimeSeconds { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public List<TrafficRouteCount> TopRoutes { get; set; } = new();
}

public interface ITrafficMetricsService
{
    void MarkRequestStarted();
    void MarkRequestCompleted(string path, int statusCode, double durationMs);
    object GetSnapshot();
    TrafficHealthMetrics GetHealthMetrics();
}
