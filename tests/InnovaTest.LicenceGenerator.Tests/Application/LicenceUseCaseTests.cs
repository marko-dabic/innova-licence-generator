using InnovaTest.LicenceGenerator.Application.Abstractions;
using InnovaTest.LicenceGenerator.Application.Errors;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Activate;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Activate.Dtos;
using GenerateLicence = InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.LicenceGenerator;
using InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.Dtos;
using InnovaTest.LicenceGenerator.Domain.Enums;
using InnovaTest.LicenceGenerator.Domain.Models;
using Xunit;

namespace InnovaTest.LicenceGenerator.Tests;

public sealed class LicenceUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 31, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(1, 2026, 2, 28)]
    [InlineData(13, 2027, 2, 28)]
    public void StandardLicenceUsesCalendarMonths(int months, int year, int month, int day)
    {
        var codec = new FakeCodec();
        var generator = new GenerateLicence(codec, new FixedClock(Now));
        generator.GenerateLicence(new StandardLicenceDto
        {
            DeviceId = "device", EnabledOptions = [], MaxConcurrentSessions = 2, ValidityInMonths = months
        });
        Assert.Equal(new DateTimeOffset(year, month, day, 12, 0, 0, TimeSpan.Zero), codec.Signed!.ExpireAtUtc);
        Assert.Equal(Now, codec.Signed.IssuedAtUtc);
        Assert.Equal(2, codec.Signed.MaxConcurrentSessions);
    }

    [Fact]
    public void PerpetualLicenceHasNoExpiryOrImportDuration()
    {
        var codec = new FakeCodec();
        new GenerateLicence(codec, new FixedClock(Now)).GenerateLicence(new PerpetualLicenceDto
        {
            DeviceId = "device", EnabledOptions = [], MaxConcurrentSessions = 1
        });
        Assert.Null(codec.Signed!.ExpireAtUtc);
        Assert.Null(codec.Signed.ValidityInMonths);
    }

    [Fact]
    public void ImportLicenceDefersExpiryUntilActivation()
    {
        var codec = new FakeCodec();
        new GenerateLicence(codec, new FixedClock(Now)).GenerateLicence(new ActivateOnImportLicenceDto
        {
            DeviceId = "device", EnabledOptions = [], MaxConcurrentSessions = 1, ValidityInMonths = 6
        });
        Assert.Null(codec.Signed!.ExpireAtUtc);
        Assert.Equal(6, codec.Signed.ValidityInMonths);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void InvalidDurationIsARequestError(int months)
    {
        var generator = new GenerateLicence(new FakeCodec(), new FixedClock(Now));
        Assert.Throws<InvalidLicenceRequestException>(() => generator.GenerateLicence(new StandardLicenceDto
        {
            DeviceId = "device", EnabledOptions = [], MaxConcurrentSessions = 1, ValidityInMonths = months
        }));
    }

    [Theory]
    [InlineData(LicenceTypeEnum.Standard)]
    [InlineData(LicenceTypeEnum.Perpetual)]
    public async Task OnlyImportLicencesCanBeActivated(LicenceTypeEnum type)
    {
        var codec = new FakeCodec { Claims = Claims() with { LicenceType = type } };
        var store = new MemoryStore();
        var activator = new LicenceActivator(codec, new FixedClock(Now), store);
        await Assert.ThrowsAsync<InvalidLicenceRequestException>(() => activator.ActivateLicenceAsync(new ActivateLicenceDto { Licence = "original" }));
        Assert.Equal(0, store.Calls);
    }

    [Fact]
    public async Task RepeatedAndConcurrentActivationReturnsFirstToken()
    {
        var codec = new FakeCodec();
        var store = new MemoryStore();
        var activator = new LicenceActivator(codec, new FixedClock(Now), store);
        var results = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
            activator.ActivateLicenceAsync(new ActivateLicenceDto { Licence = "original" }))));
        Assert.All(results, result => Assert.Equal(results[0], result));
        Assert.Equal(1, codec.ActivationSignCount);
        Assert.Equal(Now.AddMonths(1), codec.ActivationExpiry);
    }

    [Fact]
    public async Task ActivatedTokenIsReturnedWithoutAnotherWrite()
    {
        var codec = new FakeCodec { Claims = Claims() with { ExpireAtUtc = Now.AddMonths(1) } };
        var store = new MemoryStore();
        var result = await new LicenceActivator(codec, new FixedClock(Now), store)
            .ActivateLicenceAsync(new ActivateLicenceDto { Licence = "already-activated" });
        Assert.Equal("already-activated", result.Licence);
        Assert.Equal(0, store.Calls);
    }

    [Fact]
    public async Task StorageFailurePropagates()
    {
        var activator = new LicenceActivator(new FakeCodec(), new FixedClock(Now), new FailingStore());
        await Assert.ThrowsAsync<IOException>(() => activator.ActivateLicenceAsync(new ActivateLicenceDto { Licence = "original" }));
    }

    private static LicenceClaims Claims() => new()
    {
        LicenceId = "id", DeviceId = "device", EnabledOptions = [], MaxConcurrentSessions = 1,
        IssuedAtUtc = Now, LicenceType = LicenceTypeEnum.ActivateOnImport, ValidityInMonths = 1
    };

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed record Verified(LicenceClaims Claims) : IVerifiedLicence;

    private sealed class FakeCodec : ILicenceTokenCodec
    {
        public LicenceClaims Claims { get; init; } = LicenceUseCaseTests.Claims();
        public LicenceClaims? Signed { get; private set; }
        public int ActivationSignCount { get; private set; }
        public DateTimeOffset? ActivationExpiry { get; private set; }
        public IVerifiedLicence Verify(string licence) => new Verified(Claims);
        public LicenceDocument Sign(LicenceClaims claims) { Signed = claims; return new("generated"); }
        public LicenceDocument SignWithExpiry(IVerifiedLicence licence, DateTimeOffset expiresAtUtc)
        {
            ActivationSignCount++;
            ActivationExpiry = expiresAtUtc;
            return new($"activated-{ActivationSignCount}");
        }
    }

    private sealed class MemoryStore : ILicenceActivationStore
    {
        private readonly object gate = new();
        private readonly Dictionary<string, LicenceDocument> documents = [];
        public int Calls { get; private set; }
        public Task<LicenceDocument> GetOrCreateAsync(string licenceId, Func<ActivatedLicence> activate, CancellationToken cancellationToken = default)
        {
            lock (gate)
            {
                Calls++;
                if (!documents.TryGetValue(licenceId, out var document))
                    documents[licenceId] = document = activate().Document;
                return Task.FromResult(document);
            }
        }
    }

    private sealed class FailingStore : ILicenceActivationStore
    {
        public Task<LicenceDocument> GetOrCreateAsync(string licenceId, Func<ActivatedLicence> activate, CancellationToken cancellationToken = default)
            => throw new IOException("Database unavailable");
    }
}
