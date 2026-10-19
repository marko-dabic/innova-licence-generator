namespace InnovaTest.LicenceGenerator.Models;

public sealed record LicenceRequest
{
    public required string DeviceId { get; init; }
    public required string[] EnabledOptions { get; init; }
    public required DateTimeOffset ExpiresAtUtc { get; init; }
    public required int MaxConcurrentSessions { get; init; }
}
