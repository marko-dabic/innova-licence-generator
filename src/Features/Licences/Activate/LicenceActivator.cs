using InnovaTest.LicenceGenerator.Enums;
using InnovaTest.LicenceGenerator.Errors;
using InnovaTest.LicenceGenerator.Features.Licences.Activate.Dtos;
using InnovaTest.LicenceGenerator.Models;
using InnovaTest.LicenceGenerator.Security;
using System.Text.Json.Nodes;

namespace InnovaTest.LicenceGenerator.Features.Licences.Activate;

public sealed class LicenceActivator(LicenceTokenCodec tokenCodec, TimeProvider timeProvider,
    ILicenceActivationStore activationStore)
{
    public Task<LicenceDocument> ActivateLicenceAsync(ActivateLicenceDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var verified = tokenCodec.Verify(request.Licence);
        var claims = verified.Claims;

        if (claims.LicenceType != LicenceTypeEnum.ActivateOnImport)
            throw InvalidLicenceRequestException.InvalidLicence("Only ActivateOnImport licences can be activated.");

        if (string.IsNullOrWhiteSpace(claims.LicenceId) || claims.ValidityInMonths is not > 0)
            throw InvalidLicenceRequestException.InvalidLicence("The licence must have an ID and a positive ValidityInMonths.");

        if (claims.ExpireAtUtc is not null) return Task.FromResult(new LicenceDocument(request.Licence));

        return activationStore.GetOrCreateAsync(claims.LicenceId,
            () => CreateActivation(verified, claims.ValidityInMonths.Value), cancellationToken);
    }

    private ActivatedLicence CreateActivation(VerifiedLicence licence, int validityInMonths)
    {
        var activatedAtUtc = timeProvider.GetUtcNow();
        DateTimeOffset expiry;
        try
        {
            expiry = activatedAtUtc.AddMonths(validityInMonths);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw InvalidLicenceRequestException.InvalidLicence("ValidityInMonths exceeds the supported expiry date range.");
        }

        licence.Payload["expireAtUtc"] = JsonValue.Create(expiry);
        return new ActivatedLicence(tokenCodec.Sign(licence.Payload), activatedAtUtc, expiry);
    }
}
