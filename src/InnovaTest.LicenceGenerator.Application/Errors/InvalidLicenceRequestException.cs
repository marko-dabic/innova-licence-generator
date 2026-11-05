namespace InnovaTest.LicenceGenerator.Application.Errors;

public sealed class InvalidLicenceRequestException(string field, string message) : ArgumentException(message)
{
    public string Field { get; } = field;

    public static InvalidLicenceRequestException InvalidLicence(string message) => new("licence", message);
}
