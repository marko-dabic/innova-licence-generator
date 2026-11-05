using InnovaTest.LicenceGenerator.Domain.Models;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences.Generate;

public interface IGeneratedLicenceStore
{
    Task SaveAsync(GeneratedLicence licence, CancellationToken cancellationToken = default);
}
