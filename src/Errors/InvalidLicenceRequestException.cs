namespace InnovaTest.LicenceGenerator.Errors;

public sealed class InvalidLicenceRequestException(string message) : ArgumentException(message);
