using InnovaTest.LicenceGenerator.Dtos;
using InnovaTest.LicenceGenerator.Models;

namespace InnovaTest.LicenceGenerator.Services;

public interface ILicenceService
{
    LicenceDocument IssueLicence(BaseLicenceRequestDto licenceRequest);
}
