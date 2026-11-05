using InnovaTest.LicenceGenerator.Application.Features.Licences.Activate;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Activate.Dtos;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.Dtos;
using InnovaTest.LicenceGenerator.Domain.Models;
using LicenceGeneratorHandler = InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.LicenceGenerator;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences;

public sealed class LicenceService(LicenceGeneratorHandler generator, LicenceActivator activator) : ILicenceService
{
    public Task<LicenceDocument> GenerateLicenceAsync(BaseLicenceRequestDto licenceRequest, CancellationToken cancellationToken = default)
        => generator.GenerateLicenceAsync(licenceRequest, cancellationToken);

    public Task<LicenceDocument> ActivateLicenceAsync(ActivateLicenceDto request, CancellationToken cancellationToken = default)
        => activator.ActivateLicenceAsync(request, cancellationToken);
}
