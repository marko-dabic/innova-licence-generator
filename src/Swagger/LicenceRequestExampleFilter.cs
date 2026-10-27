using System.Text.Json.Nodes;
using InnovaTest.LicenceGenerator.Dtos;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

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
              "maxConcurrentSessions": 1
            }
            """);
    }
}
