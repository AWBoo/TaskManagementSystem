using System.ComponentModel.DataAnnotations;

namespace backend.DTOS.Tasks
{
    public class TaskUpdateDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid ProjectId { get; set; } 
        public string? ProjectName { get; set; } 
        public Guid? UserId { get; set; } 
        public string? AssignedUserEmail { get; set; } 
    }
}
