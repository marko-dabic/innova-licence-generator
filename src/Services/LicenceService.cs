using InnovaTest.LicenceGenerator.Dtos;
using InnovaTest.LicenceGenerator.Models;

namespace InnovaTest.LicenceGenerator.Services;

public sealed class LicenceService(LicenceGenerator generator, LicenceActivator activator) : ILicenceService
{
    public LicenceDocument GenerateLicence(BaseLicenceRequestDto licenceRequest)
        => generator.GenerateLicence(licenceRequest);

    public LicenceDocument ActivateLicence(ActivateLicenceDto request)
        => activator.ActivateLicence(request);
}
