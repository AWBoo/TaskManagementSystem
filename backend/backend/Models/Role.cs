using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    // Represents a user role in the system.
    public class Role
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        // Navigation property for users assigned to this role.
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}