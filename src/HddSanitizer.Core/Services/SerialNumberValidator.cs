using System;
using System.Collections.Generic;
using System.Linq;

namespace HddSanitizer.Core;

public static class SerialNumberValidator
{
    private static readonly HashSet<string> DummySerials = new(StringComparer.OrdinalIgnoreCase)
    {
        "0123456789ABCDEF",
        "000000000000",
        "FFFFFFFFFFFF",
        "UNKNOWN",
        "N/A",
        "123456789",
        "0123456789",
        "0",
        "NONE"
    };

    public static bool IsDummySerial(string? serial)
    {
        if (string.IsNullOrWhiteSpace(serial)) return true;

        string clean = serial.Trim();
        if (DummySerials.Contains(clean)) return true;

        if (clean.All(c => c == '0') || clean.All(c => c == 'F') || clean.All(c => c == 'X'))
            return true;

        return false;
    }
}