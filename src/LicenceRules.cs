namespace InnovaTest.LicenceGenerator;

internal static class LicenceRules
{
    public static void Check(string deviceId, string[] options, int sessions)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || deviceId != deviceId.Trim() || deviceId.Length > 256)
            throw new ArgumentException("Device ID must be 1?256 characters with no surrounding whitespace.");
        if (sessions < 1) throw new ArgumentException("MaxConcurrentSessions must be positive.");
        if (options is null || options.Length > 128 || options.Any(o => string.IsNullOrWhiteSpace(o)
            || o.Length > 128 || o != o.Trim()) || options.Distinct(StringComparer.Ordinal).Count() != options.Length)
            throw new ArgumentException("Options must be unique, non-empty names (at most 128 names, 128 characters each).");
    }
}
