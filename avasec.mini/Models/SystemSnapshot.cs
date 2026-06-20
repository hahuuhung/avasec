namespace AVASec.Mini.Models;

public sealed class SystemSnapshot
{
    public float CpuPercent { get; init; }
    public float RamPercent { get; init; }
    public float RamUsedMb { get; init; }
    public float RamTotalMb { get; init; }
    public float DiskPercent { get; init; }
    public float DiskFreeGb { get; init; }
}

public sealed class BoostResult
{
    public long BytesCleaned { get; init; }
    public long RamFreedMb { get; init; }
    public int ProcessesTrimmed { get; init; }
    public string MessageVi { get; init; } = string.Empty;
    public string MessageEn { get; init; } = string.Empty;
}