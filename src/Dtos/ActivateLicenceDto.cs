using System.ComponentModel.DataAnnotations;

namespace InnovaTest.LicenceGenerator.Dtos;

public sealed class ActivateLicenceDto
{
    [Required]
    [StringLength(65536, MinimumLength = 1)]
    public required string Licence { get; init; }
}
