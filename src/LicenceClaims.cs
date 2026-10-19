using System.Text.Json.Serialization;

namespace InnovaTest.LicenceGenerator;

public sealed record LicenceClaims
{
    [JsonRequired, JsonPropertyName("iss")] public string Issuer { get; init; } = "InnovaTEST.LicenceGenerator";
    [JsonRequired, JsonPropertyName("aud")] public string Audience { get; init; } = "InnovaTEST";
    [JsonPropertyName("jti")] public required string LicenceId { get; init; }
    [JsonPropertyName("iat")] public required long IssuedAt { get; init; }
    [JsonPropertyName("nbf")] public required long NotBefore { get; init; }
    [JsonPropertyName("exp")] public required long ExpiresAt { get; init; }
    [JsonRequired]
    public int Version { get; init; } = 1;
    public required string DeviceId { get; init; }
    public required string[] EnabledOptions { get; init; }
    public required int MaxConcurrentSessions { get; init; }

}
