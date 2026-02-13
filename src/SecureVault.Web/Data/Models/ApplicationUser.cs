using Microsoft.AspNetCore.Identity;

namespace SecureVault.Web.Data.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EncryptedFile> EncryptedFiles { get; set; } = new List<EncryptedFile>();
    public ICollection<EncryptionKeyRecord> EncryptionKeys { get; set; } = new List<EncryptionKeyRecord>();
    public ICollection<AuditLogEntry> AuditLogs { get; set; } = new List<AuditLogEntry>();
}
