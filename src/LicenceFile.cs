namespace InnovaTest.LicenceGenerator;

internal static class LicenceFile
{
    public static void SaveNew(string path, string content)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream);
        writer.Write(content);
    }
}
