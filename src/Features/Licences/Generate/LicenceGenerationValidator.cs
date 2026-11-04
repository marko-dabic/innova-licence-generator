using InnovaTest.LicenceGenerator.Errors;
using InnovaTest.LicenceGenerator.Features.Licences.Generate.Dtos;
using System.ComponentModel.DataAnnotations;

namespace InnovaTest.LicenceGenerator.Features.Licences.Generate;

internal static class LicenceGenerationValidator
{
    public static void Validate(BaseLicenceRequestDto request)
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
