using System.ComponentModel.DataAnnotations;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences.Activate.Dtos;

public sealed class ActivateLicenceDto
{
    [Required]
    public required string Licence { get; init; }
}
