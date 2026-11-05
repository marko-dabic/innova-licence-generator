using InnovaTest.LicenceGenerator.Domain.Models;

namespace InnovaTest.LicenceGenerator.Application.Abstractions;

public interface IVerifiedLicence
{
    LicenceClaims Claims { get; }
}

public interface ILicenceTokenCodec
{
    IVerifiedLicence Verify(string licence);
    LicenceDocument Sign(LicenceClaims claims);
    LicenceDocument SignWithExpiry(IVerifiedLicence licence, DateTimeOffset expiresAtUtc);
}
