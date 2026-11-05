using InnovaTest.LicenceGenerator.Application.Features.Licences.Activate.Dtos;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.Dtos;
using InnovaTest.LicenceGenerator.Domain.Models;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences;

public interface ILicenceService
{
    LicenceDocument GenerateLicence(BaseLicenceRequestDto licenceRequest);
    Task<LicenceDocument> ActivateLicenceAsync(ActivateLicenceDto request, CancellationToken cancellationToken = default);
}
