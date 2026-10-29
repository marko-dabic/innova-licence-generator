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

        LicenceClaims claims;
        switch (licenceRequest.LicenceType)
        {
            case LicenceType.Standard when licenceRequest is StandardLicenceDto standard:
                claims = CreateStandardClaims(standard, now);
                break;
            case LicenceType.ActivateOnImport when licenceRequest is ActivateOnImportLicenceDto import:
                claims = CreateActivateOnImportClaims(import, now);
                break;
            case LicenceType.Perpetual when licenceRequest is PerpetualLicenceDto perpetual:
                claims = CreatePerpetualClaims(perpetual, now);
                break;
            default:
                throw new ArgumentException("Unsupported licence type or request DTO.", nameof(licenceRequest));
        }
        
        using var rsa = CreateRsa(signingKey.Pem);

        return EncodeLicence(claims, rsa);
    }

    private static LicenceClaims CreateStandardClaims(StandardLicenceDto request, DateTimeOffset issuedAtUtc)
        => new()
        {
            LicenceId = Guid.NewGuid().ToString(),
            IssuedAtUtc = issuedAtUtc,
            LicenceType = LicenceType.Standard,
            ExpireAtUtc = issuedAtUtc.AddMonths(request.ValidityInMonths),
            DeviceId = request.DeviceId,
            EnabledOptions = request.EnabledOptions,
            MaxConcurrentSessions = request.MaxConcurrentSessions
        };

    private static LicenceClaims CreateActivateOnImportClaims(ActivateOnImportLicenceDto request, DateTimeOffset issuedAtUtc)
        => new()
        {
            LicenceId = Guid.NewGuid().ToString(),
            IssuedAtUtc = issuedAtUtc,
            LicenceType = LicenceType.ActivateOnImport,
            ValidityInMonths = request.ValidityInMonths,
            DeviceId = request.DeviceId,
            EnabledOptions = request.EnabledOptions,
            MaxConcurrentSessions = request.MaxConcurrentSessions
        };

    private static LicenceClaims CreatePerpetualClaims(PerpetualLicenceDto request, DateTimeOffset issuedAtUtc)
        => new()
        {
            LicenceId = Guid.NewGuid().ToString(),
            IssuedAtUtc = issuedAtUtc,
            LicenceType = LicenceType.Perpetual,
            DeviceId = request.DeviceId,
            EnabledOptions = request.EnabledOptions,
            MaxConcurrentSessions = request.MaxConcurrentSessions
        };

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
