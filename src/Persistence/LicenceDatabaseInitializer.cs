using Microsoft.EntityFrameworkCore;

namespace InnovaTest.LicenceGenerator.Persistence;

internal sealed class LicenceDatabaseInitializer(IDbContextFactory<LicenceDbContext> contextFactory)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
    }
}
