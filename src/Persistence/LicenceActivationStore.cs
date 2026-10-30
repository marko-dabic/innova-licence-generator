using InnovaTest.LicenceGenerator.Models;
using System.Security.Cryptography;
using System.Text;

namespace InnovaTest.LicenceGenerator.Persistence;

public sealed class LicenceActivationStore(IHostEnvironment environment)
{
    private readonly string _directory = Path.Combine(environment.ContentRootPath, "licences", "activations");

    public LicenceDocument GetOrCreate(string licenceId, Func<LicenceDocument> activate)
    {
        Directory.CreateDirectory(_directory);
        var name = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(licenceId)));
        var path = Path.Combine(_directory, name + ".jwt");
        if (File.Exists(path)) return new LicenceDocument(File.ReadAllText(path));

        var document = activate();
        var temporaryPath = Path.Combine(_directory, Guid.NewGuid() + ".tmp");
        try
        {
            File.WriteAllText(temporaryPath, document.Licence);
            try
            {
                // Publish only complete files; concurrent activations share the first result.
                File.Move(temporaryPath, path, overwrite: false);
            }
            catch (IOException) when (File.Exists(path))
            {
                return new LicenceDocument(File.ReadAllText(path));
            }
            return document;
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
