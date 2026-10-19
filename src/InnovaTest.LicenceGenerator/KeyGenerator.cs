using System.Security.Cryptography;

namespace InnovaTest.LicenceGenerator;

internal static class KeyGenerator
{
    public static void Generate(string directory)
    {
        Directory.CreateDirectory(directory);
        var privatePath = Path.Combine(directory, "licence.private.pem");
        var publicPath = Path.Combine(directory, "licence.public.pem");
        if (File.Exists(privatePath) || File.Exists(publicPath))
            throw new IOException("Key files already exist. Use a new directory; never overwrite an issuing key.");
        using var rsa = RSA.Create(3072);
        SaveNew(privatePath, rsa.ExportPkcs8PrivateKeyPem());
        SaveNew(publicPath, rsa.ExportSubjectPublicKeyInfoPem());
    }

    internal static void SaveNew(string path, string content)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream);
        writer.Write(content);
    }
}
