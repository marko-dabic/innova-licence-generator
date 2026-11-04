namespace InnovaTest.LicenceGenerator.Models;

public sealed record ActivatedLicence(
    LicenceDocument Document,
    DateTimeOffset? ActivatedAtUtc,
    DateTimeOffset ExpiresAtUtc);
