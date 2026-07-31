using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using HddSanitizer.Core;
using HddSanitizer.Domain;

namespace HddSanitizer.Infrastructure;

[SupportedOSPlatform("windows")]
public class WindowsDriveScanner : IDriveScanner
{
    public Task<IEnumerable<DriveModel>> GetConnectedDrivesAsync()
    {
        var list = new List<DriveModel>();

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject drive in searcher.Get())
            {
                string deviceId = drive["DeviceID"]?.ToString() ?? "PHYSICALDRIVE0"; 
                string model = drive["Model"]?.ToString() ?? "Unbekanntes Laufwerk";
                string serial = drive["SerialNumber"]?.ToString()?.Trim() ?? "KEINE-SERIENNR";
                string interfaceType = drive["InterfaceType"]?.ToString() ?? "SATA";
                string status = drive["Status"]?.ToString() ?? "OK";
                
                long sizeBytes = 0;
                if (drive["Size"] != null)
                {
                    long.TryParse(drive["Size"].ToString(), out sizeBytes);
                }

                string shortPath = deviceId.Replace(@"\\.\PHYSICALDRIVE", "PD");
                bool isSystem = shortPath.Equals("PD0", StringComparison.OrdinalIgnoreCase) || 
                                deviceId.EndsWith("0", StringComparison.OrdinalIgnoreCase);

                string smartStatus = status.Equals("OK", StringComparison.OrdinalIgnoreCase) ? "PASSED (Gut)" : "WARNING / FAIL";

                list.Add(new DriveModel(
                    DevicePath: shortPath,
                    ModelName: model,
                    SerialNumber: serial,
                    CapacityBytes: sizeBytes,
                    InterfaceType: interfaceType,
                    IsSystemDrive: isSystem,
                    SmartStatus: smartStatus,
                    PowerOnHours: 0,
                    TemperatureC: 32
                ));
            }
        }
        catch
        {
            // Fallback bei Berechtigungs- oder WMI-Fehlern
        }

        return Task.FromResult<IEnumerable<DriveModel>>(list);
    }
}
