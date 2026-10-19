using System.Text.Json.Serialization;

namespace InnovaTest.LicenceGenerator.Models;

public sealed record LicenceClaims
{
    [JsonRequired, JsonPropertyName("iss")] public string Issuer { get; init; } = "InnovaTEST.LicenceGenerator";
    [JsonRequired, JsonPropertyName("aud")] public string Audience { get; init; } = "InnovaTEST";
    [JsonPropertyName("licenceId")] public required string LicenceId { get; init; }
    public required string DeviceId { get; init; }
    public required int ValidityInMonths { get; init; }
    public required int MaxConcurrentSessions { get; init; }
    public required string[] EnabledOptions { get; init; }
    [JsonPropertyName("iat")] public required long IssuedAt { get; init; }
}
