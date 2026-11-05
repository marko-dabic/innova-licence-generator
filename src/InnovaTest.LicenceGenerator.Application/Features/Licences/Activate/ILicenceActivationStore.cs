using InnovaTest.LicenceGenerator.Domain.Models;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences.Activate;

public interface ILicenceActivationStore
{
    // Atomically creates the activation; duplicates throw LicenceAlreadyActivatedException.
    Task<LicenceDocument> CreateAsync(string licenceId, Func<ActivatedLicence> activate,
        CancellationToken cancellationToken = default);
}
