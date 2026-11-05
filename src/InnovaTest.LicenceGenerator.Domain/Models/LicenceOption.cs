namespace InnovaTest.LicenceGenerator.Domain.Models;

public sealed record LicenceOption
{
    public required string Name { get; init; }

    public LicenceOption[]? Options { get; init; }
}
