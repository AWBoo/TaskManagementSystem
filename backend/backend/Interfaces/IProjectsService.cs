using backend.DTOS.Projects;
using backend.DTOS.Tasks;

namespace backend.Interfaces
{
    public interface IProjectsService
    {
        // Retrieves all projects, with optional date range filtering.
        Task<IEnumerable<ProjectResponseDto>> GetAllProjectsAsync(DateTime? startDate, DateTime? endDate);
        // Retrieves a single project by its ID.
        Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id);
        // Retrieves all tasks associated with a specific project.
        Task<IEnumerable<TaskResponseDto>> GetTasksForProjectAsync(Guid projectId);
        // Creates a new project.
        Task<ProjectResponseDto> CreateProjectAsync(ProjectCreateDto dto);
        // Updates an existing project by its ID.
        Task<ProjectResponseDto?> UpdateProjectAsync(Guid id, ProjectUpdateDto dto);
        // Deletes a project by its ID.
        Task<bool> DeleteProjectAsync(Guid id);
        // Retrieves a simplified list of projects for selection purposes.
        Task<IEnumerable<ProjectSelectionDto>> GetAllProjectsForSelectionAsync();
    }
}