using System.ComponentModel.DataAnnotations;

namespace backend.DTOS.Projects
{
    public class ProjectUpdateDto
    {
        [Required] public string Name { get; set; }
        public string? Description { get; set; }
    }
}
