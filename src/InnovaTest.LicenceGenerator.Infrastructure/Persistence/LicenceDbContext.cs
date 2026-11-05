using Microsoft.EntityFrameworkCore;

namespace InnovaTest.LicenceGenerator.Infrastructure.Persistence;

public sealed class LicenceDbContext(DbContextOptions<LicenceDbContext> options) : DbContext(options)
{
    public DbSet<LicenceActivation> LicenceActivations => Set<LicenceActivation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var activation = modelBuilder.Entity<LicenceActivation>();
        activation.HasKey(x => x.LicenceIdHash);
        activation.Property(x => x.LicenceIdHash).HasColumnType("char(64)");
        activation.Property(x => x.LicenceId).IsRequired();
        activation.Property(x => x.ActivatedToken).IsRequired();
    }
}
