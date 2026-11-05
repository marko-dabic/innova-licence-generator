using InnovaTest.LicenceGenerator.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace InnovaTest.LicenceGenerator.Infrastructure.Persistence;

public sealed class LicenceDbContext(DbContextOptions<LicenceDbContext> options) : DbContext(options)
{
    public DbSet<LicenceActivation> LicenceActivations => Set<LicenceActivation>();

    public DbSet<GeneratedLicence> GeneratedLicences => Set<GeneratedLicence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var generated = modelBuilder.Entity<GeneratedLicence>();
        generated.ToTable("GeneratedLicences");
        generated.HasKey(x => x.LicenceId);
        generated.Property(x => x.LicenceId).HasMaxLength(36);
        generated.Property(x => x.DeviceId).HasMaxLength(256).IsRequired();
        generated.Property(x => x.LicenceType).HasConversion<string>().HasMaxLength(32).IsRequired();
        generated.Property(x => x.Licence).IsRequired();
        generated.Property(x => x.GeneratedAtUtc).IsRequired();

        var activation = modelBuilder.Entity<LicenceActivation>();
        activation.ToTable("LicenceActivations");
        // Keep the database lookup key out of the domain model.
        activation.Property<string>("LicenceIdHash").HasColumnType("char(64)");
        activation.HasKey("LicenceIdHash");
        activation.Property(x => x.LicenceId).IsRequired();
        activation.Property(x => x.ActivatedToken).IsRequired();
    }
}
