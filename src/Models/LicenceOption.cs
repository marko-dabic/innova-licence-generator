using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InnovaTest.LicenceGenerator.Models;

public sealed record LicenceOption
{
    [Required]
    public required string Name { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LicenceOption[]? Options { get; init; }
}
