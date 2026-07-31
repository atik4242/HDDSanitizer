using System;
using System.IO;
using System.Threading.Tasks;
using HddSanitizer.Core;
using HddSanitizer.Domain;
using HddSanitizer.Infrastructure;
using Xunit;

namespace HddSanitizer.Tests;

public class SafetyGuardAndCertTests
{
    [Fact]
    public void CanErase_ReturnsFalse_ForSystemDrive()
    {
        // Arrange
        var drive = new DriveModel("PD0", "NVMe SSD", "SN123", 1000000000000, "NVMe", true);

        // Act
        bool result = SanitizerSafetyGuard.CanErase(drive);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanErase_ReturnsTrue_ForNonSystemDrive()
    {
        // Arrange
        var drive = new DriveModel("PD1", "SATA HDD", "SN456", 20000000000000, "SATA", false);

        // Act
        bool result = SanitizerSafetyGuard.CanErase(drive);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("SN456", true)]
    [InlineData("sn456", true)]
    [InlineData(" WRONG_SN ", false)]
    [InlineData("", false)]
    public void VerifySerialNumber_ValidatesInputCorrectly(string input, bool expected)
    {
        // Arrange
        var drive = new DriveModel("PD1", "SATA HDD", "SN456", 20000000000000, "SATA", false);

        // Act
        bool result = SanitizerSafetyGuard.VerifySerialNumber(drive, input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CertificateWriter_CreatesValidJsonFile()
    {
        // Arrange
        string testLogDir = Path.Combine(Path.GetTempPath(), "HddSanitizerTestLogs_" + Guid.NewGuid().ToString("N"));
        var writer = new CertificateWriter(testLogDir);
        var drive = new DriveModel("PD1", "Exos X20", "TEST-SN-999", 20003989340160, "SATA", false);

        try
        {
            // Act
            string filePath = await writer.GenerateCertificateAsync(drive, "Zero Fill");

            // Assert
            Assert.True(File.Exists(filePath));
            string content = await File.ReadAllTextAsync(filePath);
            Assert.Contains("TEST-SN-999", content);
            Assert.Contains("SUCCESSFUL", content);
        }
        finally
        {
            if (Directory.Exists(testLogDir))
            {
                Directory.Delete(testLogDir, true);
            }
        }
    }
}
