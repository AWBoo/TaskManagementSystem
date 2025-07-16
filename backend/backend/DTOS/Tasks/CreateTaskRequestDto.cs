using System.ComponentModel.DataAnnotations;

namespace backend.DTOS.Tasks
{
    public class CreateTaskRequestDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Project ID is required.")]
        public Guid ProjectId { get; set; } // Task must belong to a project
    }
}
