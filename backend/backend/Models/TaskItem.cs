using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    // Represents a single task item.
    public class TaskItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required] // Title is required.
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "ToDo"; // Default status for a new task.

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProjectId")]
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; } // Navigation property to the associated project.

        [ForeignKey("UserId")]
        public Guid? UserId { get; set; } // ID of the user assigned to this task.
        public User? User { get; set; } // Navigation property to the assigned user.
    }
}