using SecureVault.Web.Data.Models;
using SecureVault.Web.Services.Encryption;

namespace SecureVault.Tests.Services;

public class EncryptionServiceFactoryTests
{
    [Fact]
    public void GetService_ReturnsCorrectService()
    {
        var services = new IEncryptionService[]
        {
            new AesEncryptionService(),
            new DesEncryptionService(),
            new TripleDesEncryptionService(),
            new RsaEncryptionService(),
            new EccEncryptionService()
        };
        var factory = new EncryptionServiceFactory(services);

        Assert.IsType<AesEncryptionService>(factory.GetService(EncryptionAlgorithm.AES));
        Assert.IsType<DesEncryptionService>(factory.GetService(EncryptionAlgorithm.DES));
        Assert.IsType<TripleDesEncryptionService>(factory.GetService(EncryptionAlgorithm.TripleDES));
        Assert.IsType<RsaEncryptionService>(factory.GetService(EncryptionAlgorithm.RSA));
        Assert.IsType<EccEncryptionService>(factory.GetService(EncryptionAlgorithm.ECC));
    }

    [Fact]
    public void SupportedAlgorithms_ReturnsAll()
    {
        var services = new IEncryptionService[]
        {
            new AesEncryptionService(),
            new DesEncryptionService(),
            new TripleDesEncryptionService(),
            new RsaEncryptionService(),
            new EccEncryptionService()
        };
        var factory = new EncryptionServiceFactory(services);

        Assert.Equal(5, factory.SupportedAlgorithms.Count);
    }
}
