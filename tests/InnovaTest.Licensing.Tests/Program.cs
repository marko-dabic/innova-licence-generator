using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InnovaTest.LicenceGenerator;
using InnovaTest.Licensing;

var now = new DateTimeOffset(2026, 10, 19, 12, 0, 0, TimeSpan.Zero);
var clock = new FixedTimeProvider(now);
using var rsa = RSA.Create(3072);
var publicKey = rsa.ExportSubjectPublicKeyInfoPem();
var issuer = new LicenceIssuer(clock);
var validator = new RsaLicenceValidator(clock);
var request = new LicenceRequest
{
    DeviceId = "DEVICE-123", EnabledOptions = ["testing", "export"],
    ExpiresAtUtc = now.AddDays(1), MaxConcurrentSessions = 2
};
var licence = issuer.Issue(request, rsa.ExportPkcs8PrivateKeyPem());
var roundTrip = JsonSerializer.Deserialize<LicenceDocument>(JsonSerializer.Serialize(licence, LicenceJson.Options), LicenceJson.Options)!;
var claims = validator.Validate(roundTrip, publicKey, request.DeviceId);
Check(claims.Enables("export") && !claims.Enables("unknown") && claims.MaxConcurrentSessions == 2, "valid round trip and options");
Reject(() => validator.Validate(licence, publicKey, "OTHER"), "wrong device");
using var other = RSA.Create(3072);
Reject(() => validator.Validate(licence, other.ExportSubjectPublicKeyInfoPem(), request.DeviceId), "wrong public key");
Reject(() => new RsaLicenceValidator(new FixedTimeProvider(request.ExpiresAtUtc)).Validate(licence, publicKey, request.DeviceId), "expiry boundary");
Reject(() => new RsaLicenceValidator(new FixedTimeProvider(now.AddSeconds(-1))).Validate(licence, publicKey, request.DeviceId), "future licence");
Reject(() => issuer.Issue(request with { MaxConcurrentSessions = 0 }, rsa.ExportPkcs8PrivateKeyPem()), "invalid session count");
Reject(() => issuer.Issue(request with { ExpiresAtUtc = now }, rsa.ExportPkcs8PrivateKeyPem()), "expired issuance");
Reject(() => issuer.Issue(request with { EnabledOptions = ["export", "export"] }, rsa.ExportPkcs8PrivateKeyPem()), "duplicate options");
Reject(() => validator.Validate(new LicenceDocument("bad"), publicKey, request.DeviceId), "malformed JWT");
Reject(() => validator.Validate(new LicenceDocument("W10.e30.AA"), publicKey, request.DeviceId), "array header");
var parts = licence.Token.Split('.');
var altered = claims with { ExpiresAt = claims.ExpiresAt + 999999, DeviceId = "OTHER", MaxConcurrentSessions = 999 };
parts[1] = Encode(JsonSerializer.SerializeToUtf8Bytes(altered, LicenceJson.Options));
Reject(() => validator.Validate(new LicenceDocument(string.Join('.', parts)), publicKey, "OTHER"), "edited claims");
parts = licence.Token.Split('.');
parts[0] = Encode(Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"));
Reject(() => validator.Validate(new LicenceDocument(string.Join('.', parts)), publicKey, request.DeviceId), "algorithm substitution");
Console.WriteLine("All 12 checks passed.");

static string Encode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
static void Check(bool success, string name)
{
    if (!success) throw new Exception($"FAIL: {name}");
    Console.WriteLine($"PASS: {name}");
}
static void Reject(Action action, string name)
{
    try { action(); }
    catch (Exception ex) when (ex is LicenceValidationException or ArgumentException)
    {
        Console.WriteLine($"PASS: {name}");
        return;
    }
    throw new Exception($"FAIL: {name} was accepted");
}
