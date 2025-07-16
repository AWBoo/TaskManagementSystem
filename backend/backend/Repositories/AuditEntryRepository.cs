using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace backend.Repositories
{
    // Repository for managing AuditEntry data.
    public class AuditEntryRepository : IAuditEntryRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<AuditEntryRepository> _logger; 

        public AuditEntryRepository(ApplicationDBContext context, ILogger<AuditEntryRepository> logger)
        {
            _context = context;
            _logger = logger; 
        }

        // Retrieves a specified number of audit entries for a given user.
        public async Task<IEnumerable<AuditEntry>> GetUserAuditHistoryAsync(Guid userId, int count = 10)
        {
            _logger.LogInformation("Retrieving top {Count} audit entries for user ID '{UserId}'.", count, userId);
            try
            {
                var auditEntries = await _context.AuditEntries
                                                 .Where(e => e.ChangedByUserId == userId)
                                                 .OrderByDescending(e => e.ChangeTimestamp)
                                                 .Take(count)
                                                 .AsNoTracking()
                                                 .ToListAsync();
                _logger.LogInformation("Retrieved {EntryCount} audit entries for user ID '{UserId}'.", auditEntries.Count, userId);
                return auditEntries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit history for user ID '{UserId}'.", userId);
                throw;
            }
        }

        // Retrieves the latest audit entries across all users.
        public async Task<IEnumerable<AuditEntry>> GetLatestAuditEntriesAsync(int count)
        {
            _logger.LogInformation("Retrieving latest {Count} audit entries across all users.", count);
            try
            {
                var auditEntries = await _context.AuditEntries
                                                 .OrderByDescending(e => e.ChangeTimestamp)
                                                 .Take(count)
                                                 .AsNoTracking()
                                                 .ToListAsync();
                _logger.LogInformation("Retrieved {EntryCount} latest audit entries.", auditEntries.Count);
                return auditEntries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest audit entries.");
                throw;
            }
        }
    }
}