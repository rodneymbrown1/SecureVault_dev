using SecureVault.Web.Data;
using SecureVault.Web.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace SecureVault.Web.Services;

public interface IAuditService
{
    Task LogAsync(string userId, AuditAction action, Guid? fileId, string? ipAddress, string? details, CancellationToken cancellationToken = default);
    Task<List<AuditLogEntry>> GetUserLogsAsync(string userId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
}

public class AuditService : IAuditService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AppDbContext dbContext, ILogger<AuditService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task LogAsync(string userId, AuditAction action, Guid? fileId, string? ipAddress, string? details, CancellationToken cancellationToken = default)
    {
        var entry = new AuditLogEntry
        {
            UserId = userId,
            Action = action,
            FileId = fileId,
            IpAddress = ipAddress,
            Details = details
        };

        _dbContext.AuditLogs.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Audit: {Action} by {UserId} on file {FileId}", action, userId, fileId);
    }

    public async Task<List<AuditLogEntry>> GetUserLogsAsync(string userId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
