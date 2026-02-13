namespace SecureVault.Web.Data.Models;

public class EncryptedFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;
    public long OriginalFileSize { get; set; }
    public string? ContentType { get; set; }
    public EncryptionAlgorithm Algorithm { get; set; }
    public string S3Key { get; set; } = null!;
    public Guid KeyRecordId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastAccessedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public EncryptionKeyRecord KeyRecord { get; set; } = null!;
}
