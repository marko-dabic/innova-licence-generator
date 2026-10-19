using System.ComponentModel.DataAnnotations;

namespace InnovaTest.LicenceGenerator.Dtos;

public sealed record LicenceRequestDto
{
    [Required]
    [StringLength(256, MinimumLength = 1)]
    public required string DeviceId { get; init; }

    [Required]
    [MaxLength(128)]
    public required string[] EnabledOptions { get; init; }

    [Required]
    public required DateTimeOffset ExpiresAtUtc { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "MaxConcurrentSessions must be a positive number.")]
    public required int MaxConcurrentSessions { get; init; }
}
