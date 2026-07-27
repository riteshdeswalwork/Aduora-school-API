namespace sms.api.Models;

public class NotificationSettings
{
    public bool EnablePush { get; set; } = true;
    public bool EnableEmail { get; set; } = true;
    public bool EnableSms { get; set; } = true;
    public bool EnableInApp { get; set; } = true;

    public List<string> AdminUserIds { get; set; } = new();
    public List<string> AdminPhones { get; set; } = new();
    public List<string> AdminEmails { get; set; } = new();

    /// <summary>Error rate % (4xx+5xx) that triggers overload when traffic is sufficient.</summary>
    public double SystemAlertErrorRatePercent { get; set; } = 10;

    /// <summary>Minimum total requests before traffic-based alerts (error rate / avg response) apply.</summary>
    public int SystemAlertMinRequests { get; set; } = 50;

    /// <summary>Optional: concurrent in-flight requests threshold (0 = disabled).</summary>
    public int SystemAlertActiveRequestsThreshold { get; set; } = 100;

    /// <summary>Average response time in ms that triggers overload when traffic is sufficient.</summary>
    public int SystemAlertAvgResponseMs { get; set; } = 4000;

    /// <summary>Machine CPU % threshold (0 = disabled).</summary>
    public double SystemAlertCpuPercent { get; set; } = 80;

    /// <summary>Machine RAM % threshold (0 = disabled).</summary>
    public double SystemAlertRamPercent { get; set; } = 90;

    /// <summary>Disk used % threshold for content-root drive (0 = disabled).</summary>
    public double SystemAlertDiskPercent { get; set; } = 75;

    public int SystemAlertCooldownMinutes { get; set; } = 15;
}
