using InnovaTest.LicenceGenerator.Errors;
using InnovaTest.LicenceGenerator.Features.Licences;
using InnovaTest.LicenceGenerator.Features.Licences.Activate;
using InnovaTest.LicenceGenerator.Persistence;
using InnovaTest.LicenceGenerator.Security;
using InnovaTest.LicenceGenerator.Swagger;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using LicenceGeneratorHandler = InnovaTest.LicenceGenerator.Features.Licences.Generate.LicenceGenerator;

namespace InnovaTest.LicenceGenerator.Configuration;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLicenceGenerator(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
            options.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = true;
        });
        services.AddSwaggerGen(options =>
        {
            options.UseOneOfForPolymorphism();
            options.SelectDiscriminatorNameUsing(_ => "licenceType");
            options.SelectDiscriminatorValueUsing(type => type.Name.Replace("LicenceDto", ""));
            options.SchemaFilter<LicenceRequestExampleFilter>();
        });
        services.AddProblemDetails();
        services.AddExceptionHandler<LicenceRequestExceptionHandler>();

        services.AddOptions<GeneratorOptions>()
            .Bind(configuration.GetRequiredSection("LicenceGenerator"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var connectionString = CreateConnectionString(configuration);
        services.AddDbContextFactory<LicenceDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        services.AddSingleton<ILicenceActivationStore, LicenceActivationStore>();
        services.AddSingleton<LegacyActivationImporter>();
        services.AddSingleton<LicenceDatabaseInitializer>();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<SigningKey>();
        services.AddSingleton<LicenceTokenCodec>();
        services.AddSingleton<LicenceGeneratorHandler>();
        services.AddSingleton<LicenceActivator>();
        services.AddSingleton<ILicenceService, LicenceService>();

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
