using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text.Json;
using InnovaTest.LicenceGenerator;
using InnovaTest.LicenceGenerator.Models;
using InnovaTest.LicenceGenerator.Configuration;

try
{
    using var configuration = new ConfigurationManager();
    configuration.SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
    var options = configuration.GetRequiredSection("LicenceGenerator").Get<GeneratorOptions>()
        ?? throw new InvalidOperationException("Missing LicenceGenerator configuration.");
    Validator.ValidateObject(options, new ValidationContext(options), validateAllProperties: true);

    var request = JsonSerializer.Deserialize<LicenceRequest>(File.ReadAllText(options.RequestPath), LicenceJson.Options)
        ?? throw new ArgumentException("Missing licence request.");

    var document = new LicenceIssuer().Issue(request, File.ReadAllText(options.PrivateKeyPath));
    
    LicenceFile.SaveNew(options.OutputPath, JsonSerializer.Serialize(document, LicenceJson.Options));

    Console.WriteLine($"Licence created: {Path.GetFullPath(options.OutputPath)}");
    return 0;
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException
    or ArgumentException or CryptographicException or InvalidOperationException or ValidationException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
