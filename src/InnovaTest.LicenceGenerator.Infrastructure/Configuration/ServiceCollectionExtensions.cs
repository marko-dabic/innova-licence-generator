using InnovaTest.LicenceGenerator.Application.Features.Licences.Generate;
using InnovaTest.LicenceGenerator.Application.Abstractions;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Activate;
using InnovaTest.LicenceGenerator.Infrastructure.Persistence;
using InnovaTest.LicenceGenerator.Infrastructure.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InnovaTest.LicenceGenerator.Infrastructure.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GeneratorOptions>()
            .Bind(configuration.GetRequiredSection("LicenceGenerator"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var connectionString = CreateConnectionString(configuration);
        services.AddDbContextFactory<LicenceDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        services.AddSingleton<ILicenceActivationStore, LicenceActivationStore>();
        services.AddSingleton<IGeneratedLicenceStore, GeneratedLicenceStore>();
        services.AddSingleton<LicenceDatabaseInitializer>();

        services.AddSingleton<SigningKey>();
        services.AddSingleton<ILicenceTokenCodec, LicenceTokenCodec>();

        return services;
    }

    private static string CreateConnectionString(IConfiguration configuration)
    {
        var configured = configuration.GetConnectionString("LicenceDatabase");
        if (string.IsNullOrWhiteSpace(configured))
            throw new InvalidOperationException("ConnectionStrings:LicenceDatabase is required.");

        var connectionString = new SqlConnectionStringBuilder(configured);
        
        if (string.IsNullOrEmpty(connectionString.Password)
            && configuration["MSSQL_SA_PASSWORD"] is { Length: > 0 } password)
            connectionString.Password = password;

        return connectionString.ConnectionString;
    }
}
