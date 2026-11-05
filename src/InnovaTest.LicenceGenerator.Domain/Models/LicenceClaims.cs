using InnovaTest.LicenceGenerator.Domain.Enums;

namespace InnovaTest.LicenceGenerator.Domain.Models;

public sealed record LicenceClaims
{
    public string Issuer { get; init; } = "InnovaTEST.LicenceGenerator";

    public string Audience { get; init; } = "InnovaTEST";

    public required string LicenceId { get; init; }

    public required string DeviceId { get; init; }
    public required LicenceTypeEnum LicenceType { get; init; }
    public int? ValidityInMonths { get; init; }
    public required int MaxConcurrentSessions { get; init; }

    public DateTimeOffset? ExpireAtUtc { get; init; }

    public required DateTimeOffset IssuedAtUtc { get; init; }

    public required LicenceOption[] EnabledOptions { get; init; }
}
