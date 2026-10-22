using InnovaTest.LicenceGenerator.Dtos;
using InnovaTest.LicenceGenerator.Errors;

namespace InnovaTest.LicenceGenerator.Services;

internal static class LicenceRules
{
    public static void Check(LicenceRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.DeviceId) && request.DeviceId != request.DeviceId.Trim())
            throw new InvalidLicenceRequestException(nameof(request.DeviceId),
                "Device ID must not have surrounding whitespace.");

        if (request.EnabledOptions is not null)
        {
            if (request.EnabledOptions.Any(option => string.IsNullOrWhiteSpace(option)
                || option.Length > 128 || option != option.Trim()))
                throw new InvalidLicenceRequestException(nameof(request.EnabledOptions),
                    "Option names must be 1-128 characters with no surrounding whitespace.");

            if (request.EnabledOptions.Distinct(StringComparer.Ordinal).Count() != request.EnabledOptions.Length)
                throw new InvalidLicenceRequestException(nameof(request.EnabledOptions),
                    "Option names must be unique.");
        }

    }
}
