using backend.Data;
using backend.DTOS.Projects;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace backend.Repositories
{
    // Repository for managing Project data.
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<ProjectRepository> _logger;

        public ProjectRepository(ApplicationDBContext context, ILogger<ProjectRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Retrieves all projects from the database.
        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            _logger.LogInformation("Retrieving all projects from the database.");
            try
            {
                var projects = await _context.Projects
                                             .AsNoTracking()
                                             .ToListAsync();
                _logger.LogInformation("Retrieved {ProjectCount} projects.", projects.Count);
                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all projects.");
                throw;
            }
        }

        // Retrieves a project by its ID, including its associated tasks.
        public async Task<Project?> GetProjectByIdAsync(Guid projectId)
        {
            _logger.LogInformation("Retrieving project with ID '{ProjectId}', including tasks.", projectId);
            try
            {
                var project = await _context.Projects
                                            .Include(p => p.Tasks)
                                            .FirstOrDefaultAsync(p => p.Id == projectId);
                if (project == null)
                {
                    _logger.LogWarning("Project with ID '{ProjectId}' not found.", projectId);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved project '{ProjectName}' (ID: '{ProjectId}') with {TaskCount} tasks.", project.Name, projectId, project.Tasks?.Count ?? 0);
                }
                return project;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with ID '{ProjectId}'.", projectId);
                throw;
            }
        }

        // Retrieves task counts for each project, mapped to ProjectTaskCountDto.
        public async Task<List<ProjectTaskCountDto>> GetProjectTaskCountsAsync()
        {
            _logger.LogInformation("Retrieving project task counts.");
            try
            {
                var projectTaskCounts = await _context.Tasks
                                                      .Include(t => t.Project)
                                                      .Where(t => t.Project != null)
                                                      .GroupBy(t => new { t.ProjectId, t.Project!.Name })
                                                      .Select(g => new ProjectTaskCountDto
                                                      {
                                                          ProjectId = g.Key.ProjectId,
                                                          ProjectName = g.Key.Name,
                                                          TaskCount = g.Count()
                                                      })
                                                      .OrderBy(p => p.ProjectName)
                                                      .ToListAsync();
                _logger.LogInformation("Retrieved {Count} project task counts.", projectTaskCounts.Count);
                return projectTaskCounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project task counts.");
                throw;
            }
        }

        // Checks if a project with the given ID exists.
        public async Task<bool> ProjectExistsAsync(Guid projectId)
        {
            _logger.LogDebug("Checking if project with ID '{ProjectId}' exists.", projectId);
            try
            {
                var exists = await _context.Projects.AnyAsync(p => p.Id == projectId);
                _logger.LogDebug("Project with ID '{ProjectId}' exists: {Exists}.", projectId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of project with ID '{ProjectId}'.", projectId);
                throw;
            }
        }

        // Adds a new project to the database.
        public async Task AddProjectAsync(Project project)
        {
            _logger.LogInformation("Adding new project '{ProjectName}' (ID: '{ProjectId}') to the database.", project.Name, project.Id);
            try
            {
                await _context.Projects.AddAsync(project);
                await SaveChangesAsync();
                _logger.LogInformation("Project '{ProjectName}' (ID: '{ProjectId}') successfully added.", project.Name, project.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding project '{ProjectName}' (ID: '{ProjectId}').", project.Name, project.Id);
                throw;
            }
        }

        // Updates an existing project in the database.
        public async Task UpdateProjectAsync(Project project)
        {
            _logger.LogInformation("Updating project '{ProjectName}' (ID: '{ProjectId}').", project.Name, project.Id);
            try
            {
                _context.Projects.Update(project);
                await SaveChangesAsync();
                _logger.LogInformation("Project '{ProjectName}' (ID: '{ProjectId}') successfully updated.", project.Name, project.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project '{ProjectName}' (ID: '{ProjectId}').", project.Name, project.Id);
                throw;
            }
        }

        // Deletes a project from the database.
        public async Task DeleteProjectAsync(Project project)
        {
            _logger.LogInformation("Deleting project '{ProjectName}' (ID: '{ProjectId}') from the database.", project.Name, project.Id);
            try
            {
                _context.Projects.Remove(project);
                await SaveChangesAsync();
                _logger.LogInformation("Project '{ProjectName}' (ID: '{ProjectId}') successfully deleted.", project.Name, project.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project '{ProjectName}' (ID: '{ProjectId}').", project.Name, project.Id);
                throw;
            }
        }

        // Saves all changes made in the current context to the database.
        public async Task SaveChangesAsync()
        {
            _logger.LogDebug("Saving changes to the database.");
            try
            {
                var changes = await _context.SaveChangesAsync();
                _logger.LogDebug("{Changes} changes saved to the database.", changes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to the database.");
                throw;
            }
        }

        // Gets all projects with their tasks and the assigned users for those tasks, with optional date filtering.
        public async Task<IEnumerable<Project>> GetAllProjectsWithTasksAndUsersAsync(DateTime? startDate, DateTime? endDate)
        {
            _logger.LogInformation("Retrieving all projects with tasks and users. StartDate: {StartDate}, EndDate: {EndDate}.", startDate, endDate);
            try
            {
                IQueryable<Project> query = _context.Projects
                    .Include(p => p.Tasks)
                        .ThenInclude(t => t.User);

                if (startDate.HasValue)
                {
                    query = query.Where(p => p.CreatedAt.Date >= startDate.Value.Date);
                    _logger.LogDebug("Filtering projects by CreatedAt >= {StartDate}.", startDate.Value.Date.ToShortDateString());
                }

                if (endDate.HasValue)
                {
                    query = query.Where(p => p.CreatedAt.Date <= endDate.Value.Date);
                    _logger.LogDebug("Filtering projects by CreatedAt <= {EndDate}.", endDate.Value.Date.ToShortDateString());
                }

                var projects = await query.ToListAsync();
                _logger.LogInformation("Retrieved {ProjectCount} projects with tasks and users.", projects.Count);
                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all projects with tasks and users.");
                throw;
            }
        }

        // Retrieves a project by ID including its tasks and assigned users.
        public async Task<Project?> GetProjectByIdWithTasksAndUsersAsync(Guid id)
        {
            _logger.LogInformation("Retrieving project with ID '{ProjectId}', including tasks and assigned users.", id);
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Tasks)
                        .ThenInclude(t => t.User)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    _logger.LogWarning("Project with ID '{ProjectId}' not found (with tasks and users).", id);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved project '{ProjectName}' (ID: '{ProjectId}') with {TaskCount} tasks and users.", project.Name, id, project.Tasks?.Count ?? 0);
                }
                return project;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with ID '{ProjectId}' (with tasks and users).", id);
                throw;
            }
        }

        // Retrieves a project by ID including its tasks.
        public async Task<Project?> GetProjectByIdWithTasksAsync(Guid id)
        {
            _logger.LogInformation("Retrieving project with ID '{ProjectId}', including tasks.", id);
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Tasks)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    _logger.LogWarning("Project with ID '{ProjectId}' not found (with tasks).", id);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved project '{ProjectName}' (ID: '{ProjectId}') with {TaskCount} tasks.", project.Name, id, project.Tasks?.Count ?? 0);
                }
                return project;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with ID '{ProjectId}' (with tasks).", id);
                throw;
            }
        }

        // Retrieves all projects mapped to ProjectSelectionDto for dropdowns.
        public async Task<IEnumerable<ProjectSelectionDto>> GetAllProjectSelectionDtosAsync()
        {
            _logger.LogInformation("Retrieving all projects for selection dropdowns.");
            try
            {
                var projectSelectionDtos = await _context.Projects
                    .Select(p => new ProjectSelectionDto
                    {
                        Id = p.Id,
                        Name = p.Name
                    })
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                _logger.LogInformation("Retrieved {Count} projects for selection.", projectSelectionDtos.Count);
                return projectSelectionDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all project selection DTOs.");
                throw;
            }
        }
    }
}