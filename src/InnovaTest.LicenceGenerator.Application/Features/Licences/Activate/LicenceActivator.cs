using InnovaTest.LicenceGenerator.Domain.Enums;
using InnovaTest.LicenceGenerator.Application.Errors;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Activate.Dtos;
using InnovaTest.LicenceGenerator.Domain.Models;
using InnovaTest.LicenceGenerator.Application.Abstractions;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences.Activate;

public sealed class LicenceActivator(ILicenceTokenCodec tokenCodec, TimeProvider timeProvider,
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

    private ActivatedLicence CreateActivation(IVerifiedLicence licence, int validityInMonths)
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

        return new ActivatedLicence(tokenCodec.SignWithExpiry(licence, expiry), activatedAtUtc, expiry);
    }
}
