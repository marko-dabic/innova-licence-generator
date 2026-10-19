using System.ComponentModel.DataAnnotations;

namespace InnovaTest.LicenceGenerator.Configuration;

public sealed class GeneratorOptions
{
    [Required]
    public string PrivateKeyPath { get; set; } = string.Empty;
}
