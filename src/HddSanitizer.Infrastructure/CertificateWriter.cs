using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using HddSanitizer.Domain;

namespace HddSanitizer.Infrastructure;

public class CertificateWriter
{
    private readonly string _outputDirectory;

    public CertificateWriter(string outputDirectory = "logs")
    {
        _outputDirectory = outputDirectory;
        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }
    }

    public async Task<string> GenerateCertificateAsync(DriveModel drive, string method = "NIST 800-88 Rev 1 Clear / Zero Fill")
    {
        var cert = new ErasureCertificate(
            CertificateId: $"CERT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            TimestampUtc: DateTime.UtcNow,
            ModelName: drive.ModelName,
            SerialNumber: drive.SerialNumber,
            CapacityBytes: drive.CapacityBytes,
            ErasureMethod: method,
            Status: "SUCCESSFUL",
            PerformedBy: Environment.UserName
        );

        string fileName = $"Sanitization_{drive.SerialNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string filePath = Path.Combine(_outputDirectory, fileName);

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(cert, options);

        await File.WriteAllTextAsync(filePath, json);

        return filePath;
    }

    public async Task<List<ErasureCertificate>> GetAllCertificatesAsync()
    {
        var certificates = new List<ErasureCertificate>();

        if (!Directory.Exists(_outputDirectory)) return certificates;

        var files = Directory.GetFiles(_outputDirectory, "*.json");
        foreach (var file in files)
        {
            try
            {
                string json = await File.ReadAllTextAsync(file);
                var cert = JsonSerializer.Deserialize<ErasureCertificate>(json);
                if (cert != null)
                {
                    certificates.Add(cert);
                }
            }
            catch
            {
                // Defekte oder fehlerhafte Log-Dateien überspringen
            }
        }

        return certificates;
    }
}
