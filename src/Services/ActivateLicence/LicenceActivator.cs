using System.Text.Json.Nodes;
using InnovaTest.LicenceGenerator.Dtos;
using InnovaTest.LicenceGenerator.Errors;
using InnovaTest.LicenceGenerator.Models;
using InnovaTest.LicenceGenerator.Serialization;

namespace InnovaTest.LicenceGenerator.Services;

public sealed class LicenceActivator(LicenceTokenCodec tokenCodec, TimeProvider timeProvider,
    LicenceActivationStore activationStore)
{
    public LicenceDocument ActivateLicence(ActivateLicenceDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var verified = tokenCodec.Verify(request.Licence);
        var claims = verified.Claims;
        var payload = verified.Payload;

        if (claims.LicenceType != LicenceTypeEnum.ActivateOnImport)
            throw InvalidLicenceRequestException.InvalidLicence("Only ActivateOnImport licences can be activated.");
        if (string.IsNullOrWhiteSpace(claims.LicenceId) || claims.ValidityInMonths is not > 0)
            throw InvalidLicenceRequestException.InvalidLicence("The licence must have an ID and a positive ValidityInMonths.");

        // Activated tokens must never receive a new expiry, even if their stored record is lost.
        if (claims.ExpireAtUtc is not null) return new LicenceDocument(request.Licence);

        return activationStore.GetOrCreate(claims.LicenceId, () =>
        {
            DateTimeOffset expiry;
            try
            {
                expiry = timeProvider.GetUtcNow().AddMonths(claims.ValidityInMonths.Value);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw InvalidLicenceRequestException.InvalidLicence("ValidityInMonths exceeds the supported expiry date range.");
            }
            payload["expireAtUtc"] = JsonValue.Create(expiry);
            return tokenCodec.Sign(payload);
        });
    }
}
