using System;

namespace HddSanitizer.Domain;

public class ErasureProgress
{
    public long BytesWritten { get; set; }
    public long TotalBytes { get; set; }
    public double Percentage { get; set; }
    public long CurrentLba { get; set; }
    public double SpeedMBs { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public TimeSpan RemainingTime { get; set; }
    public string MethodName { get; set; } = string.Empty;
}