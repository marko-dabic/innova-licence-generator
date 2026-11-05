using InnovaTest.LicenceGenerator.Domain.Enums;

namespace InnovaTest.LicenceGenerator.Domain.Models;

public sealed record GeneratedLicence
{
    public required string LicenceId { get; init; }
    public required string DeviceId { get; init; }
    public required LicenceTypeEnum LicenceType { get; init; }
    public required string Licence { get; init; }
    public required DateTimeOffset GeneratedAtUtc { get; init; }
}
