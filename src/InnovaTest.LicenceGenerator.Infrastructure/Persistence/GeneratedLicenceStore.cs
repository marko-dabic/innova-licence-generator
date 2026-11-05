using InnovaTest.LicenceGenerator.Application.Features.Licences.Generate;
using InnovaTest.LicenceGenerator.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace InnovaTest.LicenceGenerator.Infrastructure.Persistence;

public sealed class GeneratedLicenceStore(IDbContextFactory<LicenceDbContext> contextFactory) : IGeneratedLicenceStore
{
    public async Task SaveAsync(GeneratedLicence licence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(licence);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        
        context.GeneratedLicences.Add(licence);
        await context.SaveChangesAsync(cancellationToken);
    }
}
