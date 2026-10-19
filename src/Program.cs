using InnovaTest.LicenceGenerator.Models;
using System.Security.Cryptography;
using System.Text.Json;
using InnovaTest.LicenceGenerator;

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
        default:
            Console.WriteLine("""
                InnovaTEST Licence Generator
                  keys <new-key-directory>
                  issue <request.json> <private-key.pem> <output.json>
                """);
            return args.Length == 0 ? 0 : 1;
    }
    return 0;
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException
    or ArgumentException or CryptographicException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
