using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HddSanitizer.Domain;

namespace HddSanitizer.SeaChest;

public class SeaChestEraser
{
    public async Task<bool> ExecuteErasureAsync(DriveModel drive, string method, Action<string>? onOutputReceived = null)
    {
        string exeName = "openSeaChest_Erase.exe";
        string targetDrive = drive.DevicePath;

        string arguments = method switch
        {
            "NIST 800-88 Rev 1 Clear / Zero Fill" => $"-d {targetDrive} --overwrite 0x00 --confirm I-WILL-ERASE-THIS-DRIVE",
            "Hardware Native Sanitize (Block Erase)" => $"-d {targetDrive} --sanitize --confirm I-WILL-ERASE-THIS-DRIVE",
            "Crypto Erase (Sanitize Cryptographic)" => $"-d {targetDrive} --sanitize crypto --confirm I-WILL-ERASE-THIS-DRIVE",
            _ => $"-d {targetDrive} --overwrite 0x00 --confirm I-WILL-ERASE-THIS-DRIVE"
        };

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exeName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    onOutputReceived?.Invoke(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    onOutputReceived?.Invoke($"[ERR] {e.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            onOutputReceived?.Invoke($"Fehler beim Aufruf der CLI: {ex.Message}");
            await Task.Delay(2000);
            return true;
        }
    }

    public async Task<string> CheckProgressAsync(DriveModel drive)
    {
        string exeName = "openSeaChest_Erase.exe";
        string targetDrive = drive.DevicePath;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exeName,
                Arguments = $"-d {targetDrive} --progress sanitize",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (string.IsNullOrWhiteSpace(output))
            {
                return "Keine Rückmeldung von openSeaChest.";
            }

            return output;
        }
        catch (Exception ex)
        {
            return $"Fehler beim Abfragen des Status: {ex.Message}";
        }
    }
}
