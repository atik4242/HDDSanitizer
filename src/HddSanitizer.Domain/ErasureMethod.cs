namespace HddSanitizer.Domain;

public enum ErasureMethod
{
    ZeroFill,
    NvmeSanitizeOrAtaSecureErase,
    RandomPattern
}

public static class ErasureMethodExtensions
{
    public static string ToDisplayName(this ErasureMethod method) => method switch
    {
        ErasureMethod.ZeroFill => "NIST 800-88 Clear (Zero-Fill Overwrite)",
        ErasureMethod.NvmeSanitizeOrAtaSecureErase => "Hardware Native Sanitize / Secure Erase",
        ErasureMethod.RandomPattern => "Random Pattern Overwrite (1-Pass)",
        _ => "Standard Zero-Fill"
    };
}
