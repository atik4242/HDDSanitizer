using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using HddSanitizer.Domain;

namespace HddSanitizer.SeaChest;

public class ManagedZeroFillEngine
{
    public async Task<bool> ExecuteZeroFillAsync(
        string devicePath, 
        long totalSizeBytes, 
        IProgress<ErasureProgress>? progress, 
        CancellationToken ct)
    {
        // 4 MiB Blockgröße für maximale Bandbreite
        const int chunkSize = 4 * 1024 * 1024;
        byte[] zeroBuffer = new byte[chunkSize];

        const int sectorSize = 512;

        try
        {
            using SafeFileHandle handle = File.OpenHandle(
                devicePath,
                FileMode.Open,
                FileAccess.Write,
                FileShare.ReadWrite,
                FileOptions.Asynchronous | FileOptions.WriteThrough);

            using var stream = new FileStream(handle, FileAccess.Write, chunkSize, isAsync: true);

            long bytesWritten = 0;
            var stopwatch = Stopwatch.StartNew();
            var lastProgressReport = Stopwatch.StartNew();

            while (bytesWritten < totalSizeBytes)
            {
                ct.ThrowIfCancellationRequested();

                int bytesToWrite = (int)Math.Min(chunkSize, totalSizeBytes - bytesWritten);

                await stream.WriteAsync(zeroBuffer.AsMemory(0, bytesToWrite), ct);
                bytesWritten += bytesToWrite;

                // Drosselung: maximal 5 UI-Updates pro Sekunde (alle 200ms)
                if (lastProgressReport.ElapsedMilliseconds >= 200 || bytesWritten >= totalSizeBytes)
                {
                    double elapsedSec = stopwatch.Elapsed.TotalSeconds;
                    double speedMBs = elapsedSec > 0 ? (bytesWritten / (1024.0 * 1024.0)) / elapsedSec : 0;
                    double percentage = (double)bytesWritten / totalSizeBytes * 100.0;
                    long remainingBytes = totalSizeBytes - bytesWritten;
                    double remainingSec = speedMBs > 0 ? (remainingBytes / (1024.0 * 1024.0)) / speedMBs : 0;

                    progress?.Report(new ErasureProgress
                    {
                        BytesWritten = bytesWritten,
                        TotalBytes = totalSizeBytes,
                        Percentage = Math.Min(100.0, percentage),
                        CurrentLba = bytesWritten / sectorSize,
                        SpeedMBs = speedMBs,
                        ElapsedTime = stopwatch.Elapsed,
                        RemainingTime = TimeSpan.FromSeconds(Math.Max(0, remainingSec)),
                        MethodName = "NIST 800-88 Clear (Managed Direct Zero-Fill)"
                    });

                    lastProgressReport.Restart();
                }
            }

            await stream.FlushAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fehler beim Direct Zero-Fill: {ex.Message}");
            return false;
        }
    }
}