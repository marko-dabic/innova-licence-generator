using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using InnovaTest.LicenceGenerator.Application.Errors;
using InnovaTest.LicenceGenerator.Domain.Enums;
using InnovaTest.LicenceGenerator.Domain.Models;
using InnovaTest.LicenceGenerator.Infrastructure.Configuration;
using InnovaTest.LicenceGenerator.Infrastructure.Security;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace InnovaTest.LicenceGenerator.Tests;

public sealed class LicenceTokenCodecTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "licence-tests-" + Guid.NewGuid());
    private readonly LicenceTokenCodec codec;

    public LicenceTokenCodecTests()
    {
        Directory.CreateDirectory(directory);
        using var rsa = RSA.Create(3072);
        File.WriteAllText(Path.Combine(directory, "test.pem"), rsa.ExportRSAPrivateKeyPem());
        codec = new LicenceTokenCodec(new SigningKey(Options.Create(new GeneratorOptions { PrivateKeyPath = "test.pem" }), new TestEnvironment(directory)));
    }

    [Fact]
    public void SignedPayloadKeepsExistingWireFormat()
    {
        var token = codec.Sign(Claims());
        var payload = ReadPayload(token.Licence);
        Assert.Equal("InnovaTEST.LicenceGenerator", (string?)payload["iss"]);
        Assert.Equal("InnovaTEST", (string?)payload["aud"]);
        Assert.False(payload.ContainsKey("issuer"));
        Assert.Equal("ActivateOnImport", (string?)payload["licenceType"]);
        Assert.True(payload.ContainsKey("expireAtUtc"));
        Assert.Null(payload["expireAtUtc"]);
        Assert.False(payload["enabledOptions"]![0]!.AsObject().ContainsKey("options"));
        Assert.Equal(Claims().LicenceId, codec.Verify(token.Licence).Claims.LicenceId);
    }

    [Fact]
    public void ActivationOnlyChangesExpiryAndKeepsValidSignature()
    {
        var original = codec.Sign(Claims());
        var expiry = DateTimeOffset.UtcNow.AddMonths(1);
        var activated = codec.SignWithExpiry(codec.Verify(original.Licence), expiry);
        var expected = ReadPayload(original.Licence);
        var actual = ReadPayload(activated.Licence);
        expected["expireAtUtc"] = actual["expireAtUtc"]!.DeepClone();
        Assert.True(JsonNode.DeepEquals(expected, actual));
        Assert.Equal(expiry, codec.Verify(activated.Licence).Claims.ExpireAtUtc);
    }

    [Fact]
    public void TamperedPayloadIsRejected()
    {
        var parts = codec.Sign(Claims()).Licence.Split('.');
        var payload = ReadPayload(string.Join('.', parts));
        payload["deviceId"] = "another-device";
        parts[1] = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload.ToJsonString())).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        Assert.Throws<InvalidLicenceRequestException>(() => codec.Verify(string.Join('.', parts)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-token")]
    [InlineData("a.b.c")]
    public void MalformedTokensAreRequestErrors(string token)
        => Assert.Throws<InvalidLicenceRequestException>(() => codec.Verify(token));

    private static LicenceClaims Claims() => new()
    {
        LicenceId = "test-id", DeviceId = "device", EnabledOptions = [new LicenceOption { Name = "option" }],
        MaxConcurrentSessions = 1, IssuedAtUtc = DateTimeOffset.UnixEpoch,
        LicenceType = LicenceTypeEnum.ActivateOnImport, ValidityInMonths = 1
    };

    private static JsonObject ReadPayload(string token)
    {
        var part = token.Split('.')[1];
        return JsonNode.Parse(Convert.FromBase64String(part.Replace('-', '+').Replace('_', '/').PadRight((part.Length + 3) / 4 * 4, '=')))!.AsObject();
    }

    public void Dispose()
    {
        File.Delete(Path.Combine(directory, "test.pem"));
        Directory.Delete(directory);
    }

    private sealed class TestEnvironment(string root) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
