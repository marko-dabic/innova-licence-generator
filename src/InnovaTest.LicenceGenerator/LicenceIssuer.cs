using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InnovaTest.Licensing;

namespace InnovaTest.LicenceGenerator;

public sealed class LicenceIssuer(TimeProvider? timeProvider = null)
{
    private readonly TimeProvider _clock = timeProvider ?? TimeProvider.System;

    public LicenceDocument Issue(LicenceRequest request, string privateKeyPem)
    {
        var now = _clock.GetUtcNow().ToUnixTimeSeconds();
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
        // Apply exactly the same claim rules used by the offline application before issuing.
        new RsaLicenceValidator(_clock).Validate(document, rsa.ExportSubjectPublicKeyInfoPem(), request.DeviceId);
        return document;
    }

    private static string Encode(byte[] bytes) => Convert.ToBase64String(bytes)
        .TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
