using System.Text.Json.Nodes;
using InnovaTest.LicenceGenerator.Models;

namespace InnovaTest.LicenceGenerator.Serialization;

public sealed record VerifiedLicence(LicenceClaims Claims, JsonObject Payload);
