using System;

namespace HddSanitizer.Domain;

public record ErasureCertificate(
    string CertificateId,
    DateTime TimestampUtc,
    string ModelName,
    string SerialNumber,
    long CapacityBytes,
    string ErasureMethod,
    string Status,
    string PerformedBy
);
