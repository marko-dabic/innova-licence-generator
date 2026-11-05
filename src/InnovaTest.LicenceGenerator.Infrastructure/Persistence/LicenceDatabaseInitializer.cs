using Microsoft.EntityFrameworkCore;

namespace InnovaTest.LicenceGenerator.Infrastructure.Persistence;

public sealed class LicenceDatabaseInitializer(IDbContextFactory<LicenceDbContext> contextFactory)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
    }
}
