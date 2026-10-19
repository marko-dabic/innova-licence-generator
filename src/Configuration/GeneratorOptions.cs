using System.ComponentModel.DataAnnotations;

namespace InnovaTest.LicenceGenerator.Configuration;

internal sealed class GeneratorOptions
{
    [Required]
    public string RequestPath { get; set; } = string.Empty;

    [Required]
    public string PrivateKeyPath { get; set; } = string.Empty;

    [Required]
    public string OutputPath { get; set; } = string.Empty;
}
