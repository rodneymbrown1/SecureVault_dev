namespace SecureVault.Web.Data.Models;

public class AuditLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = null!;
    public AuditAction Action { get; set; }
    public Guid? FileId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? Details { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
