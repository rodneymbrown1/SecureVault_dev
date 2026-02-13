using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SecureVault.Web.Data;
using SecureVault.Web.Data.Models;

namespace SecureVault.Web.Services;

public class KeyManagementService : IKeyManagementService
{
    private readonly AppDbContext _dbContext;
    private readonly byte[] _masterKey;
    private readonly ILogger<KeyManagementService> _logger;

    public KeyManagementService(AppDbContext dbContext, IConfiguration configuration, ILogger<KeyManagementService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;

        var masterKeyBase64 = configuration["ENCRYPTION_MASTER_KEY"]
            ?? throw new InvalidOperationException("ENCRYPTION_MASTER_KEY environment variable is not set.");
        _masterKey = Convert.FromBase64String(masterKeyBase64);
    }

    public async Task<EncryptionKeyRecord> StoreKeyAsync(string userId, EncryptionAlgorithm algorithm, byte[] key, byte[]? iv, int keySize, CancellationToken cancellationToken = default)
    {
        var encryptedKey = EncryptWithMasterKey(key);

        var record = new EncryptionKeyRecord
        {
            UserId = userId,
            Algorithm = algorithm,
            EncryptedKeyData = encryptedKey,
            IV = iv,
            KeySize = keySize,
            IsActive = true
        };

        _dbContext.EncryptionKeys.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Stored encryption key {KeyId} for user {UserId}, algorithm {Algorithm}", record.Id, userId, algorithm);
        return record;
    }

    public async Task<(byte[] key, byte[]? iv)> RetrieveKeyAsync(Guid keyRecordId, string userId, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.EncryptionKeys
            .FirstOrDefaultAsync(k => k.Id == keyRecordId && k.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException($"Key record {keyRecordId} not found for user.");

        var decryptedKey = DecryptWithMasterKey(record.EncryptedKeyData);
        return (decryptedKey, record.IV);
    }

    public async Task DeactivateKeyAsync(Guid keyRecordId, string userId, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.EncryptionKeys
            .FirstOrDefaultAsync(k => k.Id == keyRecordId && k.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException($"Key record {keyRecordId} not found for user.");

        record.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private byte[] EncryptWithMasterKey(byte[] data)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var ciphertext = new byte[data.Length];
        var tag = new byte[16];

        using var aesGcm = new AesGcm(_masterKey, 16);
        aesGcm.Encrypt(nonce, data, ciphertext, tag);

        // [nonce(12)][tag(16)][ciphertext]
        var result = new byte[12 + 16 + ciphertext.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, 12);
        ciphertext.CopyTo(result, 28);
        return result;
    }

    private byte[] DecryptWithMasterKey(byte[] encryptedData)
    {
        var nonce = encryptedData.AsSpan(0, 12).ToArray();
        var tag = encryptedData.AsSpan(12, 16).ToArray();
        var ciphertext = encryptedData.AsSpan(28).ToArray();

        var plaintext = new byte[ciphertext.Length];
        using var aesGcm = new AesGcm(_masterKey, 16);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }
}
