namespace InnovaTest.LicenceGenerator.Infrastructure.Persistence;

public sealed class LicenceActivation
{
    public required string LicenceIdHash { get; init; }
    public required string LicenceId { get; init; }
    public required string ActivatedToken { get; init; }
    public DateTimeOffset? ActivatedAtUtc { get; init; }
    public required DateTimeOffset ExpiresAtUtc { get; init; }
}
