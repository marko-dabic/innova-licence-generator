using InnovaTest.LicenceGenerator.Models;

namespace InnovaTest.LicenceGenerator.Features.Licences.Activate;

public interface ILicenceActivationStore
{
    Task<LicenceDocument> GetOrCreateAsync(string licenceId, Func<ActivatedLicence> activate,
        CancellationToken cancellationToken = default);
}
