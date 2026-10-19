using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace InnovaTest.LicenceGenerator.Configuration;

public sealed class SigningKey
{
    public string Pem { get; }

    public SigningKey(IOptions<GeneratorOptions> options)
    {
        Pem = File.ReadAllText(options.Value.PrivateKeyPath);

        using var rsa = RSA.Create();
        
        rsa.ImportFromPem(Pem);
        
        if (rsa.KeySize < 3072)
            throw new InvalidOperationException("RSA key must be at least 3072 bits.");
            
        _ = rsa.SignData([0], HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
