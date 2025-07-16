namespace backend.DTOS.Tasks
{
    public class TaskResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? ProjectName { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? UserId { get; set; } 
        public string? AssignedUserEmail { get; set; }
    }
}
