using InnovaTest.LicenceGenerator.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.Dtos;

public sealed class ActivateOnImportLicenceDto : BaseLicenceRequestDto
{
    [JsonIgnore]
    public override LicenceTypeEnum LicenceType => LicenceTypeEnum.ActivateOnImport;
    public bool IsActivateOnImport => true;

    [Range(1, int.MaxValue, ErrorMessage = "ValidityInMonths must be a positive number.")]
    public required int ValidityInMonths { get; init; }
}
