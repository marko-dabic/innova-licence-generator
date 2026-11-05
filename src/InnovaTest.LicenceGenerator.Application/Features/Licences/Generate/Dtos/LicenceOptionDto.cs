using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using InnovaTest.LicenceGenerator.Domain.Models;

namespace InnovaTest.LicenceGenerator.Application.Features.Licences.Generate.Dtos;

public sealed record LicenceOptionDto
{
    [Required]
    public required string Name { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LicenceOptionDto[]? Options { get; init; }

    internal LicenceOption ToDomain() => new()
    {
        Name = Name,
        Options = Options?.Select(option => option.ToDomain()).ToArray()
    };
}
