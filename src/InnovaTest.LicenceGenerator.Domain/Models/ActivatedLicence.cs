namespace InnovaTest.LicenceGenerator.Domain.Models;

public sealed record ActivatedLicence(
    LicenceDocument Document,
    DateTimeOffset? ActivatedAtUtc,
    DateTimeOffset ExpiresAtUtc);
