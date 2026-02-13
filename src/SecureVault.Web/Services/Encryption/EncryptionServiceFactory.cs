using SecureVault.Web.Data.Models;

namespace SecureVault.Web.Services.Encryption;

public class EncryptionServiceFactory
{
    private readonly Dictionary<EncryptionAlgorithm, IEncryptionService> _services;

    public EncryptionServiceFactory(IEnumerable<IEncryptionService> services)
    {
        _services = services.ToDictionary(s => s.Algorithm);
    }

    public IEncryptionService GetService(EncryptionAlgorithm algorithm)
    {
        if (_services.TryGetValue(algorithm, out var service))
            return service;

        throw new ArgumentException($"No encryption service registered for algorithm: {algorithm}");
    }

    public IReadOnlyCollection<EncryptionAlgorithm> SupportedAlgorithms => _services.Keys.ToList().AsReadOnly();
}
