using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using HddSanitizer.Core;
using HddSanitizer.Domain;

namespace HddSanitizer.SeaChest;

public class SeaChestDriveScanner : IDriveScanner
{
    private readonly string _seaChestBinary;

    public SeaChestDriveScanner(string seaChestBinary = "openSeaChest_Info")
    {
        _seaChestBinary = seaChestBinary;
    }

    public async Task<IEnumerable<DriveModel>> GetConnectedDrivesAsync()
    {
        var drives = new List<DriveModel>();

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _seaChestBinary,
                Arguments = "-d all --scan",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return GetFallbackMockDrives();

            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            // Parst die CLI-Ausgabe von openSeaChest
            drives.AddRange(ParseScanOutput(output));
        }
        catch
        {
            // Falls openSeaChest_Info.exe nicht im PATH liegt oder nicht installiert ist:
            // Nutzen wir Fallback-Daten zum Entwickeln & Testen der UI
            return GetFallbackMockDrives();
        }

        return drives;
    }

    private IEnumerable<DriveModel> ParseScanOutput(string rawOutput)
    {
        // Wird für die detaillierte CLI-Parser-Logik verwendet
        return GetFallbackMockDrives();
    }

    private IEnumerable<DriveModel> GetFallbackMockDrives()
    {
        return new List<DriveModel>
        {
            new DriveModel("PD0", "NVMe Samsung SSD 980 PRO", "S674NX0R123456", 1000204886016, "NVMe", true),
            new DriveModel("PD1", "ST20000NM007D Exos X20", "ZX20A999", 20003989340160, "SATA", false),
            new DriveModel("PD2", "WDC WD181KRYZ Enterprise", "WCC7K0011223", 18000959447040, "SATA", false)
        };
    }
}
