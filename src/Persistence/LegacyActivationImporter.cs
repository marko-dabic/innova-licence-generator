using InnovaTest.LicenceGenerator.Enums;
using InnovaTest.LicenceGenerator.Features.Licences.Activate;
using InnovaTest.LicenceGenerator.Models;
using InnovaTest.LicenceGenerator.Security;

namespace InnovaTest.LicenceGenerator.Persistence;

public sealed class LegacyActivationImporter(IHostEnvironment environment, LicenceTokenCodec codec,
    ILicenceActivationStore store, ILogger<LegacyActivationImporter> logger)
{
    public async Task ImportAsync(CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(environment.ContentRootPath, "licences", "activations");
        if (!Directory.Exists(directory)) return;

        var count = 0;
        foreach (var path in Directory.EnumerateFiles(directory, "*.jwt"))
        {
            var token = await File.ReadAllTextAsync(path, cancellationToken);
            var claims = codec.Verify(token).Claims;
            if (claims.LicenceType != LicenceTypeEnum.ActivateOnImport
                || string.IsNullOrWhiteSpace(claims.LicenceId) || claims.ExpireAtUtc is null)
                throw new InvalidOperationException($"Invalid legacy activation file: {Path.GetFileName(path)}");

            await store.GetOrCreateAsync(claims.LicenceId,
                () => new ActivatedLicence(new LicenceDocument(token), null, claims.ExpireAtUtc.Value), cancellationToken);
            count++;
        }
        logger.LogInformation("Imported or verified {Count} legacy activation records. Original files were preserved.", count);
    }
}
