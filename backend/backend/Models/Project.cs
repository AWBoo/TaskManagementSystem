namespace backend.Models
{
    // Represents a project within the task management system.
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for tasks associated with this project.
        public ICollection<TaskItem> Tasks { get; set; }
    }
}