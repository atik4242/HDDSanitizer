using System;
using HddSanitizer.Domain;

namespace HddSanitizer.Core;

public static class SanitizerSafetyGuard
{
    public static bool CanErase(DriveModel drive)
    {
        if (drive == null) return false;
        if (drive.IsSystemDrive) return false; // Systemplatte niemals zulassen
        
        return true;
    }

    public static bool VerifySerialNumber(DriveModel drive, string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput)) return false;
        return string.Equals(drive.SerialNumber.Trim(), userInput.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
