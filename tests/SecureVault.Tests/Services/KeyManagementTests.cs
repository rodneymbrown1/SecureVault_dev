using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using SecureVault.Web.Data;
using SecureVault.Web.Data.Models;
using SecureVault.Web.Services;

namespace SecureVault.Tests.Services;

public class KeyManagementTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly KeyManagementService _service;
    private readonly string _testUserId = "test-user-123";

    public KeyManagementTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);

        // Generate a test master key
        var masterKey = RandomNumberGenerator.GetBytes(32);
        var masterKeyBase64 = Convert.ToBase64String(masterKey);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ENCRYPTION_MASTER_KEY"] = masterKeyBase64
            })
            .Build();

        _service = new KeyManagementService(_dbContext, config, NullLogger<KeyManagementService>.Instance);
    }

    [Fact]
    public async Task StoreAndRetrieveKey_RoundTrip()
    {
        var originalKey = RandomNumberGenerator.GetBytes(32);
        var iv = RandomNumberGenerator.GetBytes(16);

        var record = await _service.StoreKeyAsync(_testUserId, EncryptionAlgorithm.AES, originalKey, iv, 256);

        Assert.NotEqual(Guid.Empty, record.Id);
        Assert.Equal(_testUserId, record.UserId);
        Assert.Equal(EncryptionAlgorithm.AES, record.Algorithm);
        Assert.True(record.IsActive);

        var (retrievedKey, retrievedIv) = await _service.RetrieveKeyAsync(record.Id, _testUserId);

        Assert.Equal(originalKey, retrievedKey);
        Assert.Equal(iv, retrievedIv);
    }

    [Fact]
    public async Task RetrieveKey_WrongUser_ThrowsException()
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var record = await _service.StoreKeyAsync(_testUserId, EncryptionAlgorithm.AES, key, null, 256);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.RetrieveKeyAsync(record.Id, "wrong-user"));
    }

    [Fact]
    public async Task DeactivateKey_SetsInactive()
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var record = await _service.StoreKeyAsync(_testUserId, EncryptionAlgorithm.AES, key, null, 256);

        await _service.DeactivateKeyAsync(record.Id, _testUserId);

        var updated = await _dbContext.EncryptionKeys.FindAsync(record.Id);
        Assert.False(updated!.IsActive);
    }

    [Fact]
    public async Task StoreKey_EncryptsKeyData()
    {
        var originalKey = RandomNumberGenerator.GetBytes(32);
        var record = await _service.StoreKeyAsync(_testUserId, EncryptionAlgorithm.AES, originalKey, null, 256);

        // The stored encrypted key data should be different from the original
        Assert.NotEqual(originalKey, record.EncryptedKeyData);
        // And longer (nonce + tag + ciphertext)
        Assert.True(record.EncryptedKeyData.Length > originalKey.Length);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
