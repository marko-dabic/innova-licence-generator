namespace InnovaTest.LicenceGenerator.Errors;

public sealed class InvalidLicenceRequestException(string field, string message) : ArgumentException(message)
{
    public string Field { get; } = field;
}
