using System.ComponentModel.DataAnnotations;
using InnovaTest.LicenceGenerator.Models;

namespace InnovaTest.LicenceGenerator.Dtos;

public sealed class StandardLicenceDto : BaseLicenceRequestDto
{
    [System.Text.Json.Serialization.JsonIgnore]
    public override LicenceTypeEnum LicenceType => LicenceTypeEnum.Standard;

    [Range(1, int.MaxValue, ErrorMessage = "ValidityInMonths must be a positive number.")]
    [Required]
    public required int ValidityInMonths { get; init; }
}
