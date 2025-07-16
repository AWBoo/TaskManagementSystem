using backend.DTOS.Projects;
using backend.DTOS.Tasks;
using backend.Interfaces;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.Services
{
    // Service for managing project-related operations.
    public class ProjectsService : IProjectsService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly ILogger<ProjectsService> _logger; 

        public ProjectsService(IProjectRepository projectRepository, ITaskItemRepository taskItemRepository, ILogger<ProjectsService> logger)
        {
            _projectRepository = projectRepository;
            _taskItemRepository = taskItemRepository;
            _logger = logger; 
        }

        // Maps a TaskItem entity to a TaskResponseDto.
        private static TaskResponseDto MapTaskToDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Status = task.Status,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name,
                UserId = task.UserId,
                AssignedUserEmail = task.User?.Email
            };
        }

        // Maps a Project entity to a ProjectResponseDto.
        private static ProjectResponseDto MapProjectToDto(Project project)
        {
            return new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                Tasks = project.Tasks?.Select(task => MapTaskToDto(task)).ToList() ?? new List<TaskResponseDto>()
            };
        }

        // Retrieves all projects, optionally filtered by date range.
        public async Task<IEnumerable<ProjectResponseDto>> GetAllProjectsAsync(DateTime? startDate, DateTime? endDate)
        {
            _logger.LogInformation("ProjectsService: Attempting to retrieve all projects. StartDate: {StartDate}, EndDate: {EndDate}.", startDate, endDate);
            var projects = await _projectRepository.GetAllProjectsWithTasksAndUsersAsync(startDate, endDate);
            var projectDtos = projects.Select(p => MapProjectToDto(p)).ToList();
            _logger.LogInformation("ProjectsService: Retrieved {ProjectCount} projects.", projectDtos.Count);
            return projectDtos;
        }

        // Retrieves a single project by its ID.
        public async Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id)
        {
            _logger.LogInformation("ProjectsService: Attempting to retrieve project with ID '{ProjectId}'.", id);
            var project = await _projectRepository.GetProjectByIdWithTasksAndUsersAsync(id);
            if (project == null)
            {
                _logger.LogWarning("ProjectsService: Project with ID '{ProjectId}' not found.", id);
                return null;
            }
            _logger.LogInformation("ProjectsService: Successfully retrieved project '{ProjectName}' (ID: '{ProjectId}').", project.Name, id);
            return MapProjectToDto(project);
        }

        // Retrieves all tasks associated with a specific project.
        public async Task<IEnumerable<TaskResponseDto>> GetTasksForProjectAsync(Guid projectId)
        {
            _logger.LogInformation("ProjectsService: Attempting to retrieve tasks for project ID '{ProjectId}'.", projectId);
            var projectExists = await _projectRepository.ProjectExistsAsync(projectId);
            if (!projectExists)
            {
                _logger.LogWarning("ProjectsService: Project with ID '{ProjectId}' not found when trying to get tasks.", projectId);
                return null;
            }

            var tasks = await _taskItemRepository.GetTasksByProjectIdWithProjectAndUserAsync(projectId);
            var taskDtos = tasks.Select(task => MapTaskToDto(task)).ToList();
            _logger.LogInformation("ProjectsService: Retrieved {TaskCount} tasks for project ID '{ProjectId}'.", taskDtos.Count, projectId);
            return taskDtos;
        }

        // Creates a new project.
        public async Task<ProjectResponseDto> CreateProjectAsync(ProjectCreateDto dto)
        {
            _logger.LogInformation("ProjectsService: Attempting to create a new project named '{ProjectName}'.", dto.Name);
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _projectRepository.AddProjectAsync(project);
            await _projectRepository.SaveChangesAsync();

            var createdProject = await _projectRepository.GetProjectByIdWithTasksAndUsersAsync(project.Id);
            _logger.LogInformation("ProjectsService: Project '{ProjectName}' (ID: '{ProjectId}') created successfully.", createdProject?.Name, createdProject?.Id);
            return MapProjectToDto(createdProject!);
        }

        // Updates an existing project.
        public async Task<ProjectResponseDto?> UpdateProjectAsync(Guid id, ProjectUpdateDto dto)
        {
            _logger.LogInformation("ProjectsService: Attempting to update project with ID '{ProjectId}'.", id);
            var project = await _projectRepository.GetProjectByIdAsync(id);

            if (project == null)
            {
                _logger.LogWarning("ProjectsService: Update failed - Project with ID '{ProjectId}' not found.", id);
                return null;
            }

            string oldName = project.Name;
            string oldDescription = project.Description;

            project.Name = dto.Name ?? project.Name;
            project.Description = dto.Description ?? project.Description;
            project.UpdatedAt = DateTime.UtcNow;

            await _projectRepository.UpdateProjectAsync(project);
            await _projectRepository.SaveChangesAsync();

            var updatedProject = await _projectRepository.GetProjectByIdWithTasksAndUsersAsync(id);
            _logger.LogInformation("ProjectsService: Project '{OldName}' (ID: '{ProjectId}') updated to '{NewName}'.", oldName, id, updatedProject?.Name);
            return MapProjectToDto(updatedProject!);
        }

        // Deletes a project by its ID.
        public async Task<bool> DeleteProjectAsync(Guid id)
        {
            _logger.LogInformation("ProjectsService: Attempting to delete project with ID '{ProjectId}'.", id);
            var project = await _projectRepository.GetProjectByIdWithTasksAsync(id);

            if (project == null)
            {
                _logger.LogWarning("ProjectsService: Delete failed - Project with ID '{ProjectId}' not found.", id);
                return false;
            }

            await _projectRepository.DeleteProjectAsync(project);
            await _projectRepository.SaveChangesAsync();
            _logger.LogInformation("ProjectsService: Project '{ProjectName}' (ID: '{ProjectId}') successfully deleted.", project.Name, id);
            return true;
        }

        // Retrieves projects for selection dropdowns.
        public async Task<IEnumerable<ProjectSelectionDto>> GetAllProjectsForSelectionAsync()
        {
            _logger.LogInformation("ProjectsService: Retrieving all projects for selection dropdowns.");
            var projects = await _projectRepository.GetAllProjectSelectionDtosAsync();
            _logger.LogInformation("ProjectsService: Retrieved {ProjectCount} projects for selection.", projects.Count());
            return projects;
        }
    }
}