using InnovaTest.LicenceGenerator.Enums;
using System.Text.Json.Serialization;

namespace InnovaTest.LicenceGenerator.Models;

public sealed record LicenceClaims
{
    [JsonRequired, JsonPropertyName("iss")] public string Issuer { get; init; } = "InnovaTEST.LicenceGenerator";
    [JsonRequired, JsonPropertyName("aud")] public string Audience { get; init; } = "InnovaTEST";
    [JsonPropertyName("licenceId")] public required string LicenceId { get; init; }
    public required string DeviceId { get; init; }
    public required LicenceTypeEnum LicenceType { get; init; }
    public int? ValidityInMonths { get; init; }
    public required int MaxConcurrentSessions { get; init; }
    [JsonPropertyName("expireAtUtc")]
    public DateTimeOffset? ExpireAtUtc { get; init; }
    [JsonPropertyName("issuedAtUtc")]
    public required DateTimeOffset IssuedAtUtc { get; init; }
    public required LicenceOption[] EnabledOptions { get; init; }
}
