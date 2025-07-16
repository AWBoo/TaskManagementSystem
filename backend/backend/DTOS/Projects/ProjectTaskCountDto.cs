namespace backend.DTOS.Projects
{
    public class ProjectTaskCountDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public int TaskCount { get; set; }
    }
}
