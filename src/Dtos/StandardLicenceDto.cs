using InnovaTest.LicenceGenerator.Models;

namespace InnovaTest.LicenceGenerator.Dtos;

public sealed class StandardLicenceDto : BaseLicenceRequestDto
{
    [System.Text.Json.Serialization.JsonIgnore]
    public override LicenceType LicenceType => LicenceType.Standard;
}
