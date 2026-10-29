using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using InnovaTest.LicenceGenerator.Models;

namespace InnovaTest.LicenceGenerator.Dtos;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "licenceType")]
[JsonDerivedType(typeof(StandardLicenceDto), nameof(Models.LicenceTypeEnum.Standard))]
[JsonDerivedType(typeof(ActivateOnImportLicenceDto), nameof(Models.LicenceTypeEnum.ActivateOnImport))]
[JsonDerivedType(typeof(PerpetualLicenceDto), nameof(Models.LicenceTypeEnum.Perpetual))]
public abstract class BaseLicenceRequestDto
{
    [Required]
    [StringLength(256, MinimumLength = 1)]
    public required string DeviceId { get; init; }

    [Required]
    public required LicenceOption[] EnabledOptions { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "MaxConcurrentSessions must be a positive number.")]
    [Required]
    public required int MaxConcurrentSessions { get; init; }

    [JsonIgnore]
    public abstract LicenceTypeEnum LicenceType { get; }
}
