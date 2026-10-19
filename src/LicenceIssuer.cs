using InnovaTest.LicenceGenerator.Models;
using InnovaTest.LicenceGenerator.Dtos;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace InnovaTest.LicenceGenerator;

public sealed class LicenceIssuer(TimeProvider? timeProvider = null)
{
    private readonly TimeProvider _clock = timeProvider ?? TimeProvider.System;

    public LicenceDocument Issue(LicenceRequestDto request, string privateKeyPem)
    {
        var now = _clock.GetUtcNow().ToUnixTimeSeconds();
        LicenceRules.Check(request, now);

        var claims = new LicenceClaims
        {
            LicenceId = Guid.NewGuid().ToString(), IssuedAt = now, NotBefore = now,
            ExpiresAt = request.ExpiresAtUtc.ToUnixTimeSeconds(), DeviceId = request.DeviceId,
            EnabledOptions = request.EnabledOptions, MaxConcurrentSessions = request.MaxConcurrentSessions
        };
        
        using var rsa = createRSA(privateKeyPem);

        return EncodeLicence(claims, rsa);
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

    private static RSA createRSA(string privateKeyPem)
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
}
