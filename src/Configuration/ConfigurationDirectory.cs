namespace InnovaTest.LicenceGenerator.Configuration;

internal static class ConfigurationDirectory
{
    public static string Find()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "InnovaTest.LicenceGenerator.slnx"))
                && File.Exists(Path.Combine(directory.FullName, "appsettings.json")))
                return directory.FullName;
        }

        return AppContext.BaseDirectory;
    }
}
