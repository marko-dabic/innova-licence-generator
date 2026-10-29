using System.ComponentModel.DataAnnotations;
using InnovaTest.LicenceGenerator.Dtos;
using InnovaTest.LicenceGenerator.Errors;

namespace InnovaTest.LicenceGenerator.Services;

internal static class LicenceRules
{
    public static void Check(BaseLicenceRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request,
                new ValidationContext(request), results, validateAllProperties: true))
            throw new InvalidLicenceRequestException(results[0].MemberNames.FirstOrDefault() ?? "request",
                results[0].ErrorMessage ?? "Invalid licence request.");

        if (!string.IsNullOrWhiteSpace(request.DeviceId) && request.DeviceId != request.DeviceId.Trim())
            throw new InvalidLicenceRequestException(nameof(request.DeviceId),
                "Device ID must not have surrounding whitespace.");
    }
}
