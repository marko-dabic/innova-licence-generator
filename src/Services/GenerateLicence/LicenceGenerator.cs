using InnovaTest.LicenceGenerator.Dtos;
using InnovaTest.LicenceGenerator.Models;
using InnovaTest.LicenceGenerator.Serialization;

namespace InnovaTest.LicenceGenerator.Services;

public sealed class LicenceGenerator(LicenceTokenCodec tokenCodec, TimeProvider timeProvider)
{
    public LicenceDocument GenerateLicence(BaseLicenceRequestDto licenceRequest)
    {
        var now = timeProvider.GetUtcNow();
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
