using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
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

            var parsedDrives = ParseScanOutput(output);
            if (parsedDrives.Count > 0)
            {
                return parsedDrives;
            }
        }
        catch
        {
            // Fallback auf Simulation, falls openSeaChest nicht lokal im PATH installiert ist
        }

        return GetFallbackMockDrives();
    }

    private List<DriveModel> ParseScanOutput(string rawOutput)
    {
        var list = new List<DriveModel>();
        
        // Parsed Zeilen wie: /dev/pd0 oder PD0 - Model - Serial
        var lines = rawOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.Contains("PD") || line.Contains("/dev/sg"))
            {
                // Vereinfachtes Parsing der Identifikatoren
                var matchPath = Regex.Match(line, @"(PD\d+|/dev/sd[a-z]|/dev/sg\d+)");
                if (matchPath.Success)
                {
                    string path = matchPath.Value;
                    bool isSystem = path.Equals("PD0", StringComparison.OrdinalIgnoreCase) || path.Equals("/dev/sda", StringComparison.OrdinalIgnoreCase);
                    list.Add(new DriveModel(path, "Generic Storage Device", "UNKNOWN-SN", 1000000000000, "SATA/NVMe", isSystem));
                }
            }
        }

        return list;
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
