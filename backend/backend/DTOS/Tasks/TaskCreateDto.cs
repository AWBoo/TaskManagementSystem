using System.ComponentModel.DataAnnotations;

namespace backend.DTOS.Tasks
{
    public class TaskCreateDto
    {
        [Required] 
        public Guid ProjectId { get; set; } 

        [Required] 
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required] 
        public DateTime DueDate { get; set; }

        [Required] 
        public string Status { get; set; }
        public Guid? UserId { get; set; }
    }
}
