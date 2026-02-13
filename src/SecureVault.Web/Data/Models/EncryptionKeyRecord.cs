namespace SecureVault.Web.Data.Models;

public class EncryptionKeyRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = null!;
    public EncryptionAlgorithm Algorithm { get; set; }
    public byte[] EncryptedKeyData { get; set; } = null!;
    public byte[]? IV { get; set; }
    public int KeySize { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ApplicationUser User { get; set; } = null!;
    public ICollection<EncryptedFile> EncryptedFiles { get; set; } = new List<EncryptedFile>();
}
