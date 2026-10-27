using InnovaTest.LicenceGenerator.Models;
using InnovaTest.LicenceGenerator.Serialization;
using InnovaTest.LicenceGenerator.Dtos;
using InnovaTest.LicenceGenerator.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace InnovaTest.LicenceGenerator.Services;

public sealed class LicenceService(SigningKey signingKey, TimeProvider timeProvider) : ILicenceService
{
    private readonly TimeProvider _clock = timeProvider;

    public LicenceDocument IssueLicence(BaseLicenceRequestDto licenceRequest)
    {
        var now = _clock.GetUtcNow();
        LicenceRules.Check(licenceRequest);

        var claims = new LicenceClaims
        {
            LicenceId = Guid.NewGuid().ToString(),
            IssuedAtUtc = now,
            LicenceType = licenceRequest.LicenceType,
            ExpireAtUtc = licenceRequest is StandardLicenceDto ? now.AddMonths(6) : null,
            ValidityInMonths = licenceRequest is ActivateOnImportLicenceDto import ? import.ValidityInMonths : null,
            DeviceId = licenceRequest.DeviceId,
            EnabledOptions = licenceRequest.EnabledOptions, MaxConcurrentSessions = licenceRequest.MaxConcurrentSessions
        };
        
        using var rsa = CreateRsa(signingKey.Pem);

        return EncodeLicence(claims, rsa);
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

    private static LicenceDocument EncodeLicence(LicenceClaims claims, RSA rsa)
    {
        var header = Encode(Encoding.UTF8.GetBytes("{\"alg\":\"RS256\",\"typ\":\"JWT\"}"));
        var payload = Encode(JsonSerializer.SerializeToUtf8Bytes(claims, LicenceJson.Options));
        var input = $"{header}.{payload}";
        var signature = rsa.SignData(Encoding.ASCII.GetBytes(input), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var document = new LicenceDocument($"{input}.{Encode(signature)}");

        return document;
    }

    private static string Encode(byte[] bytes) => Convert.ToBase64String(bytes)
        .TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
