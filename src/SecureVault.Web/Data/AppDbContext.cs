using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecureVault.Web.Data.Models;

namespace SecureVault.Web.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<EncryptedFile> EncryptedFiles => Set<EncryptedFile>();
    public DbSet<EncryptionKeyRecord> EncryptionKeys => Set<EncryptionKeyRecord>();
    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<EncryptedFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.S3Key).IsUnique();
            entity.Property(e => e.Algorithm).HasConversion<string>();
            entity.Property(e => e.OriginalFileName).HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(200);
            entity.Property(e => e.S3Key).HasMaxLength(1000);

            entity.HasOne(e => e.User)
                .WithMany(u => u.EncryptedFiles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.KeyRecord)
                .WithMany(k => k.EncryptedFiles)
                .HasForeignKey(e => e.KeyRecordId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<EncryptionKeyRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Algorithm).HasConversion<string>();

            entity.HasOne(e => e.User)
                .WithMany(u => u.EncryptionKeys)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AuditLogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.Action).HasConversion<string>();
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.Details).HasMaxLength(2000);
        });
    }
}
