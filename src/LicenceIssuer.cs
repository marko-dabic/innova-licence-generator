using InnovaTest.LicenceGenerator.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace InnovaTest.LicenceGenerator;

public sealed class LicenceIssuer(TimeProvider? timeProvider = null)
{
    private readonly TimeProvider _clock = timeProvider ?? TimeProvider.System;

    public LicenceDocument Issue(LicenceRequest request, string privateKeyPem)
    {
        ArgumentNullException.ThrowIfNull(request);
        LicenceRules.Check(request.DeviceId, request.EnabledOptions, request.MaxConcurrentSessions);
        var now = _clock.GetUtcNow().ToUnixTimeSeconds();
        if (request.ExpiresAtUtc.ToUnixTimeSeconds() <= now)
            throw new ArgumentException("Expiry must be in the future.");
        var claims = new LicenceClaims
        {
            LicenceId = Guid.NewGuid().ToString(), IssuedAt = now, NotBefore = now,
            ExpiresAt = request.ExpiresAtUtc.ToUnixTimeSeconds(), DeviceId = request.DeviceId,
            EnabledOptions = request.EnabledOptions, MaxConcurrentSessions = request.MaxConcurrentSessions
        };
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        if (rsa.KeySize < 3072) throw new ArgumentException("RSA key must be at least 3072 bits.");
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
