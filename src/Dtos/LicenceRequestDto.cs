using System.ComponentModel.DataAnnotations;
using InnovaTest.LicenceGenerator.Models;

namespace InnovaTest.LicenceGenerator.Dtos;

public sealed record LicenceRequestDto
{
    [Required]
    [StringLength(256, MinimumLength = 1)]
    public required string DeviceId { get; init; }

    [Required]
    public required LicenceOption[] EnabledOptions { get; init; }

    [Required]
    public required bool IsPerpetual { get; init; }

    [Required]
    public required bool IsActivateOnImport { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "ValidityInMonths must be a positive number.")]
    [System.ComponentModel.DefaultValue(6)]
    public int ValidityInMonths { get; init; } = 6;

    [Range(1, int.MaxValue, ErrorMessage = "MaxConcurrentSessions must be a positive number.")]
    [Required]
    public required int MaxConcurrentSessions { get; init; }
}
