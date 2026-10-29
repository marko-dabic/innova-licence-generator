using InnovaTest.LicenceGenerator.Models;
using InnovaTest.LicenceGenerator.Serialization;
using InnovaTest.LicenceGenerator.Dtos;
using System.Text.Json.Nodes;
using InnovaTest.LicenceGenerator.Errors;

namespace InnovaTest.LicenceGenerator.Services;

public sealed class LicenceService(LicenceTokenCodec tokenCodec, TimeProvider timeProvider,
    LicenceActivationStore activationStore) : ILicenceService
{
    private readonly TimeProvider _clock = timeProvider;

    public LicenceDocument ActivateLicence(ActivateLicenceDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var verified = tokenCodec.Verify(request.Licence);
        var claims = verified.Claims;
        var payload = verified.Payload;

        if (claims.LicenceType != LicenceTypeEnum.ActivateOnImport)
            throw InvalidLicence("Only ActivateOnImport licences can be activated.");
        if (string.IsNullOrWhiteSpace(claims.LicenceId) || claims.ValidityInMonths is not > 0)
            throw InvalidLicence("The licence must have an ID and a positive ValidityInMonths.");

        // Activated tokens must never receive a new expiry, even if their stored record is lost.
        if (claims.ExpireAtUtc is not null) return new LicenceDocument(request.Licence);

        return activationStore.GetOrCreate(claims.LicenceId, () =>
        {
            DateTimeOffset expiry;
            try
            {
                expiry = _clock.GetUtcNow().AddMonths(claims.ValidityInMonths.Value);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw InvalidLicence("ValidityInMonths exceeds the supported expiry date range.");
            }
            payload["expireAtUtc"] = JsonValue.Create(expiry);
            return tokenCodec.Sign(payload);
        });
    }

    private static InvalidLicenceRequestException InvalidLicence(string message) => new("licence", message);

    public LicenceDocument GenerateLicence(BaseLicenceRequestDto licenceRequest)
    {
        var now = _clock.GetUtcNow();
        LicenceRules.Check(licenceRequest);

        LicenceClaims claims;
        switch (licenceRequest.LicenceType)
        {
            case LicenceTypeEnum.Standard when licenceRequest is StandardLicenceDto standard:
                claims = CreateStandardClaims(standard, now);
                break;
            case LicenceTypeEnum.ActivateOnImport when licenceRequest is ActivateOnImportLicenceDto import:
                claims = CreateActivateOnImportClaims(import, now);
                break;
            case LicenceTypeEnum.Perpetual when licenceRequest is PerpetualLicenceDto perpetual:
                claims = CreatePerpetualClaims(perpetual, now);
                break;
            default:
                throw new ArgumentException("Unsupported licence type or request DTO.", nameof(licenceRequest));
        }
        
        return tokenCodec.Sign(claims);
    }

    private static LicenceClaims CreateStandardClaims(StandardLicenceDto request, DateTimeOffset generatedAtUtc)
        => new()
        {
            LicenceId = Guid.NewGuid().ToString(),
            IssuedAtUtc = generatedAtUtc,
            LicenceType = LicenceTypeEnum.Standard,
            ExpireAtUtc = generatedAtUtc.AddMonths(request.ValidityInMonths),
            DeviceId = request.DeviceId,
            EnabledOptions = request.EnabledOptions,
            MaxConcurrentSessions = request.MaxConcurrentSessions
        };

    private static LicenceClaims CreateActivateOnImportClaims(ActivateOnImportLicenceDto request, DateTimeOffset generatedAtUtc)
        => new()
        {
            LicenceId = Guid.NewGuid().ToString(),
            IssuedAtUtc = generatedAtUtc,
            LicenceType = LicenceTypeEnum.ActivateOnImport,
            ValidityInMonths = request.ValidityInMonths,
            DeviceId = request.DeviceId,
            EnabledOptions = request.EnabledOptions,
            MaxConcurrentSessions = request.MaxConcurrentSessions
        };

    private static LicenceClaims CreatePerpetualClaims(PerpetualLicenceDto request, DateTimeOffset generatedAtUtc)
        => new()
        {
            LicenceId = Guid.NewGuid().ToString(),
            IssuedAtUtc = generatedAtUtc,
            LicenceType = LicenceTypeEnum.Perpetual,
            DeviceId = request.DeviceId,
            EnabledOptions = request.EnabledOptions,
            MaxConcurrentSessions = request.MaxConcurrentSessions
        };
}
