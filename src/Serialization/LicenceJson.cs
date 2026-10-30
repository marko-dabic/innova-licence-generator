using System.Text.Json.Serialization;
using System.Text.Json;

namespace InnovaTest.LicenceGenerator.Serialization;

public static class LicenceJson
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        };

        options.MakeReadOnly(populateMissingResolver: true);
        
        return options;
    }
}
