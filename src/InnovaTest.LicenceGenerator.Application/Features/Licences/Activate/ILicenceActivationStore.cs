using InnovaTest.LicenceGenerator.Domain.Models;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences.Activate;

public interface ILicenceActivationStore
{
    Task<LicenceDocument> CreateAsync(string licenceId, Func<ActivatedLicence> activate,
        CancellationToken cancellationToken = default);
}
