using InnovaTest.LicenceGenerator.Application.Abstractions;
using InnovaTest.LicenceGenerator.Domain.Models;
using System.Text.Json.Nodes;

namespace InnovaTest.LicenceGenerator.Infrastructure.Security;

internal sealed record VerifiedLicence(LicenceClaims Claims, JsonObject Payload) : IVerifiedLicence;
