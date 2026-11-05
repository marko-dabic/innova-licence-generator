using InnovaTest.LicenceGenerator.Application.Errors;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Activate;
using InnovaTest.LicenceGenerator.Domain.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace InnovaTest.LicenceGenerator.Infrastructure.Persistence;

public sealed class LicenceActivationStore(IDbContextFactory<LicenceDbContext> contextFactory) : ILicenceActivationStore
{
    public async Task<LicenceDocument> CreateAsync(string licenceId, Func<ActivatedLicence> activate,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(licenceId);
        ArgumentNullException.ThrowIfNull(activate);

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(licenceId)));
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.LicenceActivations.AnyAsync(x => EF.Property<string>(x, "LicenceIdHash") == hash, cancellationToken))
            throw new LicenceAlreadyActivatedException();

        var activation = activate();
        var entry = context.LicenceActivations.Add(new LicenceActivation
        {
            LicenceId = licenceId,
            ActivatedToken = activation.Document.Licence,
            ActivatedAtUtc = activation.ActivatedAtUtc,
            ExpiresAtUtc = activation.ExpiresAtUtc
        });

        entry.Property<string>("LicenceIdHash").CurrentValue = hash;
        
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new LicenceAlreadyActivatedException();
        }

        return activation.Document;
    }

}
