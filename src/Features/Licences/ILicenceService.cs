using InnovaTest.LicenceGenerator.Features.Licences.Activate.Dtos;
using InnovaTest.LicenceGenerator.Features.Licences.Generate.Dtos;
using InnovaTest.LicenceGenerator.Models;

namespace InnovaTest.LicenceGenerator.Features.Licences;

public interface ILicenceService
{
    LicenceDocument GenerateLicence(BaseLicenceRequestDto licenceRequest);
    Task<LicenceDocument> ActivateLicenceAsync(ActivateLicenceDto request, CancellationToken cancellationToken = default);
}
