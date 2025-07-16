using System.ComponentModel.DataAnnotations;

namespace backend.DTOS.Projects
{
    public class ProjectCreateDto
    {
        [Required] public string Name { get; set; }
        public string? Description { get; set; }
    }
}
