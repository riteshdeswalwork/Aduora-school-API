namespace sms.api.Interfaces.Shared;

public class HostResourceSnapshot
{
    public double? CpuPercent { get; set; }
    public double? RamPercent { get; set; }
    public double? DiskPercent { get; set; }
    public string? DiskRoot { get; set; }
    public long? RamTotalMb { get; set; }
    public long? RamUsedMb { get; set; }
    public long? DiskTotalGb { get; set; }
    public long? DiskFreeGb { get; set; }
    public string Source { get; set; } = "unknown";
}

public interface IHostResourceMetricsService
{
    /// <summary>Samples CPU over a short interval, plus current RAM/disk usage.</summary>
    Task<HostResourceSnapshot> SampleAsync(CancellationToken ct = default);
}
