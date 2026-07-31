namespace HddSanitizer.Domain;

public record DriveModel(
    string DevicePath,      // z.B. PD0 oder /dev/sg0
    string ModelName,       // z.B. ST20000NM000D
    string SerialNumber,    // z.B. ZVT12345
    long CapacityBytes,     // Kapazität in Bytes
    string InterfaceType,   // SATA, NVMe, SAS
    bool IsSystemDrive      // true = Gesperrt! (z.B. C:\)
)
{
    public double CapacityGB => Math.Round((double)CapacityBytes / (1024 * 1024 * 1024), 2);
    public double CapacityTB => Math.Round((double)CapacityBytes / (1024L * 1024 * 1024 * 1024), 2);
}
