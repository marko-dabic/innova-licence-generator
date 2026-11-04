using InnovaTest.LicenceGenerator.Features.Licences.Activate;
using InnovaTest.LicenceGenerator.Features.Licences.Activate.Dtos;
using InnovaTest.LicenceGenerator.Features.Licences.Generate.Dtos;
using InnovaTest.LicenceGenerator.Models;
using LicenceGeneratorHandler = InnovaTest.LicenceGenerator.Features.Licences.Generate.LicenceGenerator;

namespace InnovaTest.LicenceGenerator.Features.Licences;

public sealed class LicenceService(LicenceGeneratorHandler generator, LicenceActivator activator) : ILicenceService
{
    public LicenceDocument GenerateLicence(BaseLicenceRequestDto licenceRequest)
        => generator.GenerateLicence(licenceRequest);

    public Task<LicenceDocument> ActivateLicenceAsync(ActivateLicenceDto request, CancellationToken cancellationToken = default)
        => activator.ActivateLicenceAsync(request, cancellationToken);
}
