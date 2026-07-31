namespace HddSanitizer.Domain;

public record DriveModel(
    string DevicePath,
    string ModelName,
    string SerialNumber,
    long CapacityBytes,
    string InterfaceType,
    bool IsSystemDrive,
    string SmartStatus = "OK",
    int PowerOnHours = 0,
    int TemperatureC = 0
)
{
    public double CapacityTB => System.Math.Round(CapacityBytes / (1024.0 * 1024.0 * 1024.0 * 1024.0), 2);
}
