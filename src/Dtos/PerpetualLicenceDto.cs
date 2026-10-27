using InnovaTest.LicenceGenerator.Models;

namespace InnovaTest.LicenceGenerator.Dtos;

public sealed class PerpetualLicenceDto : BaseLicenceRequestDto
{
    [System.Text.Json.Serialization.JsonIgnore]
    public override LicenceType LicenceType => LicenceType.Perpetual;
    public bool IsPerpetual => true;
}
