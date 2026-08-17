using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HddSanitizer.Domain;

namespace HddSanitizer.SeaChest;

public class SeaChestEraser
{
    public async Task<bool> ExecuteErasureAsync(
        DriveModel drive, 
        string method, 
        IProgress<ErasureProgress>? progress = null, 
        Action<string>? onOutputReceived = null,
        CancellationToken ct = default)
    {
        if (method.Contains("Zero-Fill") || method.Contains("Zero Fill") || method.Contains("NIST"))
        {
            var engine = new ManagedZeroFillEngine();
            long totalBytes = (long)(drive.CapacityTB * 1024.0 * 1024.0 * 1024.0 * 1024.0);
            
            return await Task.Run(() => engine.ExecuteZeroFillAsync(drive.DevicePath, totalBytes, progress, ct), ct);
        }

        string exeName = "openSeaChest_Erase.exe";
        string targetDrive = drive.DevicePath;
        string confirmFlag = "--confirm this-will-erase-data";

        string arguments = method switch
        {
            var m when m.Contains("Random") 
                => $"-d {targetDrive} --overwrite 0x55 {confirmFlag}",

            var m when m.Contains("Crypto") 
                => $"-d {targetDrive} --sanitize crypto {confirmFlag}",

            var m when m.Contains("Native Sanitize") || m.Contains("Hardware") || m.Contains("Sanitize")
                => $"-d {targetDrive} --sanitize overwrite {confirmFlag}",

            _ => $"-d {targetDrive} --overwrite 0x00 {confirmFlag}"
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
                    onOutputReceived?.Invoke(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    onOutputReceived?.Invoke($"[ERR] {e.Data}");
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(ct);
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            onOutputReceived?.Invoke($"Fehler bei der Ausführung: {ex.Message}");
            return false;
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