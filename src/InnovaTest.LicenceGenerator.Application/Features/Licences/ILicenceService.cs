using InnovaTest.LicenceGenerator.Application.Features.Licences.Activate.Dtos;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.Dtos;
using InnovaTest.LicenceGenerator.Domain.Models;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences;

public interface ILicenceService
{
    Task<LicenceDocument> GenerateLicenceAsync(BaseLicenceRequestDto licenceRequest, CancellationToken cancellationToken = default);
    Task<LicenceDocument> ActivateLicenceAsync(ActivateLicenceDto request, CancellationToken cancellationToken = default);
}
