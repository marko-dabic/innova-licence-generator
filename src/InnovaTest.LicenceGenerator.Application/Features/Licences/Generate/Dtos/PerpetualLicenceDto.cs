using InnovaTest.LicenceGenerator.Domain.Enums;
using System.Text.Json.Serialization;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.Dtos;

public sealed class PerpetualLicenceDto : BaseLicenceRequestDto
{
    [JsonIgnore]
    public override LicenceTypeEnum LicenceType => LicenceTypeEnum.Perpetual;
    public bool IsPerpetual => true;
}
