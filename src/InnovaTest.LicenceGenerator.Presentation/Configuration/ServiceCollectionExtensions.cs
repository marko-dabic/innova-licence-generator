using InnovaTest.LicenceGenerator.Presentation.Errors;
using InnovaTest.LicenceGenerator.Application.Features.Licences;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Activate;
using InnovaTest.LicenceGenerator.Infrastructure.Configuration;
using InnovaTest.LicenceGenerator.Presentation.Swagger;
using System.Text.Json.Serialization;
using LicenceGeneratorHandler = InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.LicenceGenerator;

namespace InnovaTest.LicenceGenerator.Presentation.Configuration;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLicenceGenerator(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<InnovaTest.LicenceGenerator.Domain.Enums.LicenceTypeEnum>());
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

        services.AddInfrastructure(configuration);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<LicenceGeneratorHandler>();
        services.AddSingleton<LicenceActivator>();
        services.AddSingleton<ILicenceService, LicenceService>();

        return services;
    }
}
