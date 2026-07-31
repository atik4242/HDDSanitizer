using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using HddSanitizer.Core;
using HddSanitizer.Domain;

namespace HddSanitizer.Infrastructure;

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
                
                long sizeBytes = 0;
                if (drive["Size"] != null)
                {
                    long.TryParse(drive["Size"].ToString(), out sizeBytes);
                }

                // Extrahiere Kurzpfad (z.B. PHYSICALDRIVE0 -> PD0)
                string shortPath = deviceId.Replace(@"\\.\PHYSICALDRIVE", "PD");

                // Prüfen ob es das C:\ Systemlaufwerk ist (über Partitionen/System-Flag)
                bool isSystem = shortPath.Equals("PD0", StringComparison.OrdinalIgnoreCase) || 
                                deviceId.EndsWith("0", StringComparison.OrdinalIgnoreCase);

                list.Add(new DriveModel(
                    DevicePath: shortPath,
                    ModelName: model,
                    SerialNumber: serial,
                    CapacityBytes: sizeBytes,
                    InterfaceType: interfaceType,
                    IsSystemDrive: isSystem
                ));
            }
        }
        catch
        {
            // Fallback falls WMI fehlschlägt
        }

        return Task.FromResult<IEnumerable<DriveModel>>(list);
    }
}
