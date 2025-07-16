using System.ComponentModel.DataAnnotations;

namespace backend.DTOS.Tasks
{
    public class UpdateTaskRequest
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Project ID is required.")]
        public Guid ProjectId { get; set; }
        public Guid? UserId { get; set; }
    }
}
