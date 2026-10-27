using System.ComponentModel.DataAnnotations;
using InnovaTest.LicenceGenerator.Models;

namespace InnovaTest.LicenceGenerator.Dtos;

public sealed class ActivateOnImportLicenceDto : BaseLicenceRequestDto
{
    [System.Text.Json.Serialization.JsonIgnore]
    public override LicenceType LicenceType => LicenceType.ActivateOnImport;
    public bool IsActivateOnImport => true;

    [Range(1, int.MaxValue, ErrorMessage = "ValidityInMonths must be a positive number.")]
    [Required]
    public required int ValidityInMonths { get; init; }
}
