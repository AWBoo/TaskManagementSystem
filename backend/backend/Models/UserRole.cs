using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    // Represents the many-to-many relationship between Users and Roles.
    public class UserRole
    {
        [Key]
        public Guid Id { get; set; }

        // Foreign key to the User entity.
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public required User User { get; set; }

        // Foreign key to the Role entity.
        public Guid RoleId { get; set; }
        [ForeignKey("RoleId")]
        public required Role Role { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow; // Timestamp of role assignment.
    }
}