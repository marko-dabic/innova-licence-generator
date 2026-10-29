using InnovaTest.LicenceGenerator.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace InnovaTest.LicenceGenerator.Security;

public sealed class SigningKey
{
    public string Pem { get; }

    public SigningKey(IOptions<GeneratorOptions> options, IHostEnvironment environment)
    {
        var path = Path.GetFullPath(options.Value.PrivateKeyPath, environment.ContentRootPath);
        
        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Signing key not found at '{path}'. Configure LicenceGenerator:PrivateKeyPath in appsettings.json or LicenceGenerator__PrivateKeyPath.", path);

        Pem = File.ReadAllText(path);

        using var rsa = RSA.Create();
        
        rsa.ImportFromPem(Pem);
        
        if (rsa.KeySize < 3072)
            throw new InvalidOperationException("RSA key must be at least 3072 bits.");
            
        _ = rsa.SignData([0], HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
