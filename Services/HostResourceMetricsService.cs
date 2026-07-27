using System.Diagnostics;
using System.Runtime.InteropServices;
using sms.api.Interfaces.Shared;

namespace sms.api.Services;

/// <summary>
/// Samples machine CPU / RAM / disk for overload alerts (Windows-first, with portable fallbacks).
/// </summary>
public class HostResourceMetricsService : IHostResourceMetricsService
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<HostResourceMetricsService> _logger;

    public HostResourceMetricsService(IHostEnvironment env, ILogger<HostResourceMetricsService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<HostResourceSnapshot> SampleAsync(CancellationToken ct = default)
    {
        var snap = new HostResourceSnapshot();

        try
        {
            FillDisk(snap);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Disk metrics unavailable");
        }

        try
        {
            if (OperatingSystem.IsWindows())
            {
                FillWindowsRam(snap);
                snap.CpuPercent = await SampleWindowsCpuAsync(ct);
                snap.Source = "windows";
            }
            else
            {
                FillPortableRam(snap);
                snap.CpuPercent = await SampleProcessCpuAsync(ct);
                snap.Source = "portable";
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "CPU/RAM metrics unavailable");
            try
            {
                FillPortableRam(snap);
                snap.CpuPercent = await SampleProcessCpuAsync(ct);
                snap.Source = "fallback";
            }
            catch (Exception ex2)
            {
                _logger.LogDebug(ex2, "Fallback host metrics failed");
            }
        }

        return snap;
    }

    private void FillDisk(HostResourceSnapshot snap)
    {
        var root = Path.GetPathRoot(Path.GetFullPath(_env.ContentRootPath));
        if (string.IsNullOrWhiteSpace(root))
            root = OperatingSystem.IsWindows() ? "C:\\" : "/";

        var drive = DriveInfo.GetDrives()
            .FirstOrDefault(d => d.IsReady && string.Equals(d.Name.TrimEnd('\\', '/'), root.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
            ?? DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Fixed);

        if (drive == null || drive.TotalSize <= 0) return;

        var used = drive.TotalSize - drive.AvailableFreeSpace;
        snap.DiskRoot = drive.Name;
        snap.DiskTotalGb = (long)Math.Round(drive.TotalSize / (1024.0 * 1024 * 1024), 0);
        snap.DiskFreeGb = (long)Math.Round(drive.AvailableFreeSpace / (1024.0 * 1024 * 1024), 0);
        snap.DiskPercent = Math.Round(100.0 * used / drive.TotalSize, 1);
    }

    private static void FillPortableRam(HostResourceSnapshot snap)
    {
        var info = GC.GetGCMemoryInfo();
        if (info.TotalAvailableMemoryBytes <= 0) return;
        snap.RamTotalMb = info.TotalAvailableMemoryBytes / (1024 * 1024);
        snap.RamUsedMb = info.MemoryLoadBytes / (1024 * 1024);
        snap.RamPercent = Math.Round(100.0 * info.MemoryLoadBytes / info.TotalAvailableMemoryBytes, 1);
    }

    private static async Task<double?> SampleProcessCpuAsync(CancellationToken ct)
    {
        using var proc = Process.GetCurrentProcess();
        var start = proc.TotalProcessorTime;
        var wall0 = Stopwatch.GetTimestamp();
        await Task.Delay(400, ct);
        proc.Refresh();
        var end = proc.TotalProcessorTime;
        var wallSec = (Stopwatch.GetTimestamp() - wall0) / (double)Stopwatch.Frequency;
        if (wallSec <= 0) return null;
        var cpuMs = (end - start).TotalMilliseconds;
        var pct = cpuMs / (Environment.ProcessorCount * wallSec * 1000.0) * 100.0;
        return Math.Round(Math.Clamp(pct, 0, 100), 1);
    }

    private static void FillWindowsRam(HostResourceSnapshot snap)
    {
        var status = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
        if (!GlobalMemoryStatusEx(ref status)) return;

        snap.RamPercent = status.dwMemoryLoad;
        snap.RamTotalMb = (long)(status.ullTotalPhys / (1024 * 1024));
        snap.RamUsedMb = (long)((status.ullTotalPhys - status.ullAvailPhys) / (1024 * 1024));
    }

    private static async Task<double?> SampleWindowsCpuAsync(CancellationToken ct)
    {
        if (!GetSystemTimes(out var idle1, out var kernel1, out var user1))
            return await SampleProcessCpuAsync(ct);

        await Task.Delay(400, ct);

        if (!GetSystemTimes(out var idle2, out var kernel2, out var user2))
            return null;

        var idle = FileTimeToUInt64(idle2) - FileTimeToUInt64(idle1);
        var kernel = FileTimeToUInt64(kernel2) - FileTimeToUInt64(kernel1);
        var user = FileTimeToUInt64(user2) - FileTimeToUInt64(user1);
        var total = kernel + user;
        if (total == 0) return 0;
        // kernel includes idle time on Windows
        var busy = total - idle;
        var pct = 100.0 * busy / total;
        return Math.Round(Math.Clamp(pct, 0, 100), 1);
    }

    private static ulong FileTimeToUInt64(FILETIME ft) =>
        ((ulong)ft.dwHighDateTime << 32) | ft.dwLowDateTime;

    [StructLayout(LayoutKind.Sequential)]
    private struct FILETIME
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(out FILETIME idleTime, out FILETIME kernelTime, out FILETIME userTime);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
}
