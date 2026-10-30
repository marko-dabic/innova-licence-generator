using InnovaTest.LicenceGenerator.Errors;
using InnovaTest.LicenceGenerator.Models;
using InnovaTest.LicenceGenerator.Serialization;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;

namespace InnovaTest.LicenceGenerator.Security;

public sealed class LicenceTokenCodec(SigningKey signingKey)
{
    public VerifiedLicence Verify(string licence)
    {
        using var rsa = CreateRsa(signingKey.Pem);
        var payload = VerifyLicence(licence, rsa);
        LicenceClaims claims;

        try
        {
            claims = payload.Deserialize<LicenceClaims>(LicenceJson.Options)
                ?? throw InvalidLicenceRequestException.InvalidLicence("Invalid licence payload.");
        }
        catch (JsonException)
        {
            throw InvalidLicenceRequestException.InvalidLicence("Invalid licence payload.");
        }

        if (claims.Issuer != "InnovaTEST.LicenceGenerator" || claims.Audience != "InnovaTEST")
            throw InvalidLicenceRequestException.InvalidLicence("Invalid licence issuer or audience.");

        return new VerifiedLicence(claims, payload);
    }

    public LicenceDocument Sign(LicenceClaims claims)
        => Sign(JsonSerializer.SerializeToUtf8Bytes(claims, LicenceJson.Options));

    public LicenceDocument Sign(JsonObject payload)
        => Sign(JsonSerializer.SerializeToUtf8Bytes(payload, LicenceJson.Options));

    private LicenceDocument Sign(byte[] payload)
    {
        using var rsa = CreateRsa(signingKey.Pem);
        return EncodeLicence(payload, rsa);
    }

    private static JsonObject VerifyLicence(string licence, RSA rsa)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(licence) || licence.Length > 65536)
                throw InvalidLicenceRequestException.InvalidLicence("A signed licence is required.");

            var parts = licence.Split('.');
            
            if (parts.Length != 3) throw InvalidLicenceRequestException.InvalidLicence("Invalid licence format.");

            using var header = JsonDocument.Parse(Decode(parts[0]));
            var properties = header.RootElement.EnumerateObject().ToArray();

            if (properties.Length != 2
                || !header.RootElement.TryGetProperty("alg", out var algorithm) || algorithm.GetString() != "RS256"
                || !header.RootElement.TryGetProperty("typ", out var type) || type.GetString() != "JWT")
                throw InvalidLicenceRequestException.InvalidLicence("Unsupported licence header.");

            if (!rsa.VerifyData(Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}"), Decode(parts[2]),
                    HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                throw InvalidLicenceRequestException.InvalidLicence("Invalid licence signature.");

            return JsonNode.Parse(Decode(parts[1]), new JsonNodeOptions { PropertyNameCaseInsensitive = false },
                new JsonDocumentOptions { AllowDuplicateProperties = false }) as JsonObject
                ?? throw InvalidLicenceRequestException.InvalidLicence("Invalid licence payload.");
        }
        catch (Exception exception) when (exception is FormatException or JsonException
                                        or InvalidOperationException or CryptographicException or ArgumentException
                                        && exception is not InvalidLicenceRequestException)
        {
            throw InvalidLicenceRequestException.InvalidLicence("Invalid signed licence.");
        }
    }

    private static byte[] Decode(string value)
    {
        if (value.Length == 0 || value.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_'))
            throw new FormatException("Invalid base64url.");

        return Convert.FromBase64String(value.Replace('-', '+').Replace('_', '/')
            .PadRight((value.Length + 3) / 4 * 4, '='));
    }

    private static RSA CreateRsa(string privateKeyPem)
    {
        var rsa = RSA.Create();
        
        try
        {
            rsa.ImportFromPem(privateKeyPem);
            if (rsa.KeySize < 3072)
                throw new ArgumentException("RSA key must be at least 3072 bits.");

            return rsa;
        }
        catch
        {
            rsa.Dispose();
            throw;
        }
    }

    private static LicenceDocument EncodeLicence(byte[] claims, RSA rsa)
    {
        var header = Encode(Encoding.UTF8.GetBytes("{\"alg\":\"RS256\",\"typ\":\"JWT\"}"));
        var payload = Encode(claims);
        var input = $"{header}.{payload}";
        var signature = rsa.SignData(Encoding.ASCII.GetBytes(input), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var document = new LicenceDocument($"{input}.{Encode(signature)}");

        return document;
    }

    private static string Encode(byte[] bytes) => Convert.ToBase64String(bytes)
        .TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
