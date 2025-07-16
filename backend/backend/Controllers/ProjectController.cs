using backend.DTOS.Projects;
using backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Defines the API controller for project management.
// Requires user authentication for access.
[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectsService _projectsService;
    private readonly ILogger<ProjectsController> _logger; 

    // Initializes a new instance of the ProjectsController.
    public ProjectsController(IProjectsService projectsService, ILogger<ProjectsController> logger) 
    {
        _projectsService = projectsService;
        _logger = logger; 
    }

    // GET /api/projects
    // Retrieves a list of all projects, optionally filtered by date range.
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetProjects(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        _logger.LogInformation("ProjectsController: Received request to get all projects. StartDate: {StartDate}, EndDate: {EndDate}", startDate, endDate);
        var projects = await _projectsService.GetAllProjectsAsync(startDate, endDate);
        _logger.LogInformation("ProjectsController: Successfully retrieved {ProjectCount} projects.", projects.Count());
        return Ok(projects);
    }

    // GET /api/projects/{id}
    // Retrieves a single project by its ID.
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetProject(Guid id)
    {
        _logger.LogInformation("ProjectsController: Received request to get project with ID '{ProjectId}'.", id);
        var project = await _projectsService.GetProjectByIdAsync(id);
        if (project == null)
        {
            _logger.LogWarning("ProjectsController: GetProject failed - Project with ID '{ProjectId}' not found.", id);
            return NotFound("Project not found.");
        }
        _logger.LogInformation("ProjectsController: Successfully retrieved project with ID '{ProjectId}'.", id);
        return Ok(project);
    }

    // POST /api/projects
    // Creates a new project.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProject(ProjectCreateDto dto)
    {
        _logger.LogInformation("ProjectsController: Received request to create new project: {ProjectName}.", dto.Name);
        // Assuming ModelState.IsValid is handled implicitly by [ApiController] and validation attributes on ProjectCreateDto
        // If validation fails, this method won't be hit or ModelState will be invalid.
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ProjectsController: CreateProject failed due to invalid model state. Errors: {ModelStateErrors}", ModelState);
            return BadRequest(ModelState);
        }

        var createdProject = await _projectsService.CreateProjectAsync(dto);
        // Assuming service returns null or throws if creation fails
        if (createdProject == null)
        {
            _logger.LogError("ProjectsController: Failed to create project '{ProjectName}'. Service returned null.", dto.Name);
            return StatusCode(500, "Failed to create project due to an unexpected error.");
        }

        _logger.LogInformation("ProjectsController: Successfully created project '{ProjectName}' with ID '{ProjectId}'.", createdProject.Name, createdProject.Id);
        return CreatedAtAction(nameof(GetProject), new { id = createdProject.Id }, createdProject);
    }

    // PUT /api/projects/{id}
    // Updates an existing project by its ID.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProject(Guid id, ProjectUpdateDto dto)
    {
        _logger.LogInformation("ProjectsController: Received request to update project ID '{ProjectId}'. New Name: {NewName}", id, dto.Name);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ProjectsController: UpdateProject for ID '{ProjectId}' failed due to invalid model state. Errors: {ModelStateErrors}", id, ModelState);
            return BadRequest(ModelState);
        }

        var updatedProject = await _projectsService.UpdateProjectAsync(id, dto);
        if (updatedProject == null)
        {
            _logger.LogWarning("ProjectsController: UpdateProject failed - Project with ID '{ProjectId}' not found or update failed.", id);
            return NotFound("Project not found or update failed.");
        }
        _logger.LogInformation("ProjectsController: Successfully updated project with ID '{ProjectId}'.", updatedProject.Id);
        return Ok(updatedProject);
    }

    // DELETE /api/projects/{id}
    // Deletes a project by its ID.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        _logger.LogInformation("ProjectsController: Received request to delete project with ID '{ProjectId}'.", id);
        var result = await _projectsService.DeleteProjectAsync(id);
        if (!result)
        {
            _logger.LogWarning("ProjectsController: DeleteProject failed - Project with ID '{ProjectId}' not found.", id);
            return NotFound("Project not found.");
        }
        _logger.LogInformation("ProjectsController: Successfully deleted project with ID '{ProjectId}'.", id);
        return NoContent(); // 204 No Content for successful deletion
    }

    // GET /api/projects/for-selection
    // Retrieves projects suitable for task association or dropdown selection.
    [HttpGet("for-selection")]
    [Authorize]
    public async Task<IActionResult> GetProjectsForSelection()
    {
        _logger.LogInformation("ProjectsController: Received request to get projects for selection dropdown.");
        var projects = await _projectsService.GetAllProjectsForSelectionAsync();
        _logger.LogInformation("ProjectsController: Retrieved {ProjectCount} projects for selection.", projects.Count());
        return Ok(projects);
    }

    // GET /api/projects/{id}/tasks
    // Retrieves all tasks associated with a specific project.
    [HttpGet("{id}/tasks")]
    [Authorize]
    public async Task<IActionResult> GetTasksForProject(Guid id)
    {
        _logger.LogInformation("ProjectsController: Received request to get tasks for project ID '{ProjectId}'.", id);
        var tasks = await _projectsService.GetTasksForProjectAsync(id);
        if (tasks == null)
        {
            _logger.LogWarning("ProjectsController: GetTasksForProject failed - Project with ID '{ProjectId}' not found.", id);
            return NotFound($"Project with ID '{id}' not found.");
        }
        _logger.LogInformation("ProjectsController: Successfully retrieved {TaskCount} tasks for project ID '{ProjectId}'.", tasks.Count(), id);
        return Ok(tasks);
    }
}