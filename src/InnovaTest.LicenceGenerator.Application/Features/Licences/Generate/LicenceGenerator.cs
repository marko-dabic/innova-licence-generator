using InnovaTest.LicenceGenerator.Application.Errors;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.Dtos;
using InnovaTest.LicenceGenerator.Domain.Models;
using InnovaTest.LicenceGenerator.Application.Abstractions;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences.Generate;

public sealed class LicenceGenerator(ILicenceTokenCodec tokenCodec, TimeProvider timeProvider)
{
    public LicenceDocument GenerateLicence(BaseLicenceRequestDto licenceRequest)
    {
        LicenceGenerationValidator.Validate(licenceRequest);
        var now = timeProvider.GetUtcNow();
        var claims = new LicenceClaims
        {
            LicenceId = Guid.NewGuid().ToString("D"),
            IssuedAtUtc = now,
            LicenceType = licenceRequest.LicenceType,
            DeviceId = licenceRequest.DeviceId,
            EnabledOptions = licenceRequest.EnabledOptions.Select(option => option.ToDomain()).ToArray(),
            MaxConcurrentSessions = licenceRequest.MaxConcurrentSessions
        };

        claims = licenceRequest switch
        {
            StandardLicenceDto standard => claims with { ExpireAtUtc = CalculateExpiry(now, standard.ValidityInMonths) },
            ActivateOnImportLicenceDto import => claims with { ValidityInMonths = import.ValidityInMonths },
            PerpetualLicenceDto => claims,
            _ => throw new ArgumentException("Unsupported licence request type.", nameof(licenceRequest))
        };

        return tokenCodec.Sign(claims);
    }

    private static DateTimeOffset CalculateExpiry(DateTimeOffset issuedAtUtc, int validityInMonths)
    {
        try
        {
            return issuedAtUtc.AddMonths(validityInMonths);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new InvalidLicenceRequestException("validityInMonths",
                "ValidityInMonths exceeds the supported expiry date range.");
        }
    }
}
