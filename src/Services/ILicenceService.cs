using InnovaTest.LicenceGenerator.Dtos;
using InnovaTest.LicenceGenerator.Models;

namespace InnovaTest.LicenceGenerator.Services;

public interface ILicenceService
{
    LicenceDocument GenerateLicence(BaseLicenceRequestDto licenceRequest);
    LicenceDocument ActivateLicence(ActivateLicenceDto request);
}
