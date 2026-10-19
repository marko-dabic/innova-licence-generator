using System.Security.Cryptography;
using System.Text.Json;
using InnovaTest.LicenceGenerator;
using InnovaTest.Licensing;

try
{
    switch (args)
    {
        case ["keys", var directory]:
            KeyGenerator.Generate(directory);
            Console.WriteLine($"Keys created in {Path.GetFullPath(directory)}. Keep the private key on the issuing computer.");
            break;
        case ["issue", var requestPath, var privateKeyPath, var outputPath]:
            var request = JsonSerializer.Deserialize<LicenceRequest>(File.ReadAllText(requestPath), LicenceJson.Options)
                ?? throw new ArgumentException("Missing licence request.");
            var document = new LicenceIssuer().Issue(request, File.ReadAllText(privateKeyPath));
            KeyGenerator.SaveNew(outputPath, JsonSerializer.Serialize(document, LicenceJson.Options));
            Console.WriteLine($"Licence created: {Path.GetFullPath(outputPath)}");
            break;
        case ["validate", var licencePath, var publicKeyPath, var deviceId]:
            var licence = JsonSerializer.Deserialize<LicenceDocument>(File.ReadAllText(licencePath), LicenceJson.Options)
                ?? throw new ArgumentException("Missing licence document.");
            var claims = new RsaLicenceValidator().Validate(licence, File.ReadAllText(publicKeyPath), deviceId);
            Console.WriteLine(JsonSerializer.Serialize(claims, LicenceJson.Options));
            break;
        default:
            Console.WriteLine("""
                InnovaTEST Licence Generator
                  keys <new-key-directory>
                  issue <request.json> <private-key.pem> <output.json>
                  validate <licence.json> <public-key.pem> <actual-device-id>
                """);
            return args.Length == 0 ? 0 : 1;
    }
    return 0;
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException
    or ArgumentException or CryptographicException or LicenceValidationException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
