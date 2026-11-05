using InnovaTest.LicenceGenerator.Domain.Models;
using InnovaTest.LicenceGenerator.Domain.Enums;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace InnovaTest.LicenceGenerator.Infrastructure.Serialization;

public static class LicenceJson
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(typeInfo =>
        {
            foreach (var property in typeInfo.Properties)
            {
                if (typeInfo.Type == typeof(LicenceClaims))
                {
                    if (property.Name == "issuer") { property.Name = "iss"; property.IsRequired = true; }
                    if (property.Name == "audience") { property.Name = "aud"; property.IsRequired = true; }
                }
                if (typeInfo.Type == typeof(LicenceOption) && property.Name == "options")
                    property.ShouldSerialize = (_, value) => value is not null;
            }
        });
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            TypeInfoResolver = resolver,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        };

        options.Converters.Add(new JsonStringEnumConverter<LicenceTypeEnum>());
        options.MakeReadOnly(populateMissingResolver: true);
        
        return options;
    }
}
