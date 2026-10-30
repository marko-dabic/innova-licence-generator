using InnovaTest.LicenceGenerator.Models;
using System.Text.Json.Nodes;

namespace InnovaTest.LicenceGenerator.Security;

public sealed record VerifiedLicence(LicenceClaims Claims, JsonObject Payload);
