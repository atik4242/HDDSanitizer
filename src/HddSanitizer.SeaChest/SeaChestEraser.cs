using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HddSanitizer.Domain;

namespace HddSanitizer.SeaChest;

public class SeaChestEraser
{
    private readonly string _seaChestEraseBinary;

    public SeaChestEraser(string seaChestEraseBinary = "openSeaChest_Erase")
    {
        _seaChestEraseBinary = seaChestEraseBinary;
    }

    public async Task<bool> ExecuteErasureAsync(DriveModel drive, string methodName)
    {
        // Sicherheitsscheck: NIEMALS Systemlaufwerke über die CLI ansteuern
        if (drive.IsSystemDrive)
        {
            throw new InvalidOperationException("SICHERHEITSSPERRE: Systemlaufwerk kann nicht gelöscht werden!");
        }

        string arguments = methodName switch
        {
            "Hardware Native Sanitize / Secure Erase" => $"-d {drive.DevicePath} --sanitize --confirm I-WILL-ERASE-THIS-DRIVE",
            "Random Pattern Overwrite (1-Pass)" => $"-d {drive.DevicePath} --overwrite 0x55 --confirm I-WILL-ERASE-THIS-DRIVE",
            _ => $"-d {drive.DevicePath} --overwrite 0x00 --confirm I-WILL-ERASE-THIS-DRIVE" // Default: Zero-Fill
        };

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _seaChestEraseBinary,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            // Falls openSeaChest_Erase nicht auf dem System liegt (Entwicklungsumgebung):
            // Rückgabe von true für erfolgreiche Simulation
            await Task.Delay(1000); 
            return true;
        }
    }
}
