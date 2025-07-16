using backend.DTOS.Tasks;

namespace backend.DTOS.Projects
{
    public class ProjectResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Include tasks, but these tasks will be TaskResponseDto, not raw TaskItem models
        public ICollection<TaskResponseDto> Tasks { get; set; } = new List<TaskResponseDto>();
    }
}
