using Microsoft.EntityFrameworkCore;

namespace InnovaTest.LicenceGenerator.Persistence;

internal sealed class LicenceDatabaseInitializer(IDbContextFactory<LicenceDbContext> contextFactory,
    LegacyActivationImporter legacyImporter)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
        await legacyImporter.ImportAsync(cancellationToken);
    }
}
