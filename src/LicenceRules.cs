using InnovaTest.LicenceGenerator.Models;
using InnovaTest.LicenceGenerator.Errors;

namespace InnovaTest.LicenceGenerator;

internal static class LicenceRules
{
    public static void Check(LicenceRequest request, long now)
    {
        ArgumentNullException.ThrowIfNull(request);

        var deviceId = request.DeviceId;
        var options = request.EnabledOptions;
        var sessions = request.MaxConcurrentSessions;

        if (string.IsNullOrWhiteSpace(deviceId) || deviceId != deviceId.Trim() || deviceId.Length > 256)
            throw new InvalidLicenceRequestException("Device ID must be 1-256 characters with no surrounding whitespace.");

        if (sessions < 1) throw new InvalidLicenceRequestException("MaxConcurrentSessions must be positive.");
        
        if (options is null || options.Length > 128 || options.Any(o => string.IsNullOrWhiteSpace(o)
            || o.Length > 128 || o != o.Trim()) || options.Distinct(StringComparer.Ordinal).Count() != options.Length)
            throw new InvalidLicenceRequestException("Options must be unique, non-empty names (at most 128 names, 128 characters each).");

        if (request.ExpiresAtUtc.ToUnixTimeSeconds() <= now)
            throw new InvalidLicenceRequestException("Expiry must be in the future.");
    }
}
