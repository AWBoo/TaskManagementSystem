using backend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Repositories.Interfaces
{
    // Defines operations for accessing and managing audit entries.
    public interface IAuditEntryRepository
    {
        // Retrieves a specified number of audit entries for a given user.
        Task<IEnumerable<AuditEntry>> GetUserAuditHistoryAsync(Guid userId, int count = 10);
        // Retrieves the latest audit entries across all users.
        Task<IEnumerable<AuditEntry>> GetLatestAuditEntriesAsync(int count);
    }
}