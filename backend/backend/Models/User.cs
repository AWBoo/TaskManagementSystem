using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    // Represents a user in the system.
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // User account status (e.g., "Active", "Deactivated").

        // Navigation property for roles associated with this user.
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // Navigation property for tasks assigned to this user.
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}