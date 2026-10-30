using InnovaTest.LicenceGenerator.Features.Licences.Generate.Dtos;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace InnovaTest.LicenceGenerator.Swagger;

public sealed class LicenceRequestExampleFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(BaseLicenceRequestDto) || schema is not OpenApiSchema requestSchema) return;

        requestSchema.Example = JsonNode.Parse("""
            {
              "deviceId": "DEVICE-123",
              "enabledOptions": [
                {
                  "name": "hardness-testing",
                  "options": [
                    { "name": "vickers" },
                    { "name": "rockwell" }
                  ]
                },
                { "name": "report-export" }
              ],
              "licenceType": "Standard",
              "validityInMonths": 6,
              "maxConcurrentSessions": 1
            }
            """);
    }
}
