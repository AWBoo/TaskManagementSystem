using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    // Represents an entry in the system's audit log.
    public class AuditEntry
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Generic ID of the audited entity (Task, Project, User).
        public Guid EntityId { get; set; }

        // The type of entity being audited (e.g., "TaskItem").
        [Required]
        [MaxLength(50)]
        public string EntityType { get; set; } = string.Empty;

        // The specific property that was changed.
        [Required]
        [MaxLength(100)]
        public string PropertyName { get; set; } = string.Empty;

        public string? OldValue { get; set; } // The value before the change.
        public string? NewValue { get; set; } // The value after the change.

        public Guid ChangedByUserId { get; set; } // User who made the change.
        public DateTime ChangeTimestamp { get; set; } = DateTime.UtcNow;

        // Type of change (e.g., "Created", "Updated").
        [Required]
        [MaxLength(20)]
        public string ChangeType { get; set; } = string.Empty;
    }
}