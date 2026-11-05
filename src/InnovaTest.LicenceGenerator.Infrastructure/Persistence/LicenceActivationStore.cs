using InnovaTest.LicenceGenerator.Application.Features.Licences.Activate;
using InnovaTest.LicenceGenerator.Domain.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace InnovaTest.LicenceGenerator.Infrastructure.Persistence;

public sealed class LicenceActivationStore(IDbContextFactory<LicenceDbContext> contextFactory) : ILicenceActivationStore
{
    public async Task<LicenceDocument> GetOrCreateAsync(string licenceId, Func<ActivatedLicence> activate,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(licenceId);
        ArgumentNullException.ThrowIfNull(activate);

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(licenceId)));
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existingToken = await FindTokenAsync(context, hash, cancellationToken);
        if (existingToken is not null) return new LicenceDocument(existingToken);

        var activation = activate();
        context.LicenceActivations.Add(new LicenceActivation
        {
            LicenceIdHash = hash,
            LicenceId = licenceId,
            ActivatedToken = activation.Document.Licence,
            ActivatedAtUtc = activation.ActivatedAtUtc,
            ExpiresAtUtc = activation.ExpiresAtUtc
        });
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            await using var readContext = await contextFactory.CreateDbContextAsync(cancellationToken);
            var winnerToken = await FindTokenAsync(readContext, hash, cancellationToken);
            if (winnerToken is null) throw;

            return new LicenceDocument(winnerToken);
        }

        return activation.Document;
    }

    private static Task<string?> FindTokenAsync(LicenceDbContext context, string licenceIdHash,
        CancellationToken cancellationToken)
        => context.LicenceActivations
            .Where(activation => activation.LicenceIdHash == licenceIdHash)
            .Select(activation => activation.ActivatedToken)
            .SingleOrDefaultAsync(cancellationToken);
}
