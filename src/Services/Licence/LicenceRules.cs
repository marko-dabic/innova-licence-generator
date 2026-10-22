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
    }
}
