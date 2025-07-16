using backend.DTOS.Projects;
using backend.Models;

namespace backend.Repositories.Interfaces
{
    // Defines operations for accessing and managing Project entities.
    public interface IProjectRepository
    {
        // Retrieves all projects.
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        // Retrieves a project by its ID.
        Task<Project?> GetProjectByIdAsync(Guid projectId);
        // Retrieves task counts for each project.
        Task<List<ProjectTaskCountDto>> GetProjectTaskCountsAsync();
        // Checks if a project with the given ID exists.
        Task<bool> ProjectExistsAsync(Guid projectId);
        // Adds a new project to the repository.
        Task AddProjectAsync(Project project);
        // Updates an existing project in the repository.
        Task UpdateProjectAsync(Project project);
        // Deletes a project from the repository.
        Task DeleteProjectAsync(Project project);
        // Saves all changes made in the current context.
        Task SaveChangesAsync();


        // Methods specifically tailored for the ProjectsService.
        Task<IEnumerable<Project>> GetAllProjectsWithTasksAndUsersAsync(DateTime? startDate, DateTime? endDate);
        // Retrieves a single project by ID including its tasks and assigned users.
        Task<Project?> GetProjectByIdWithTasksAndUsersAsync(Guid id);
        // Retrieves a project by ID including its tasks.
        Task<Project?> GetProjectByIdWithTasksAsync(Guid id);
        // Retrieves all projects mapped to ProjectSelectionDto for dropdowns.
        Task<IEnumerable<ProjectSelectionDto>> GetAllProjectSelectionDtosAsync();
    }
}