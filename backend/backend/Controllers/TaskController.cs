using backend.DTOS.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.Interfaces;


// Defines the API controller for task management.
// Requires user authentication for access.
[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITasksService _tasksService;
    private readonly ILogger<TasksController> _logger; 

    // Initializes a new instance of the TasksController.
    public TasksController(ITasksService tasksService, ILogger<TasksController> logger) 
    {
        _tasksService = tasksService;
        _logger = logger; 
    }

    // Helper to get the current authenticated user's ID.
    // This relies on the `User` property from ControllerBase.
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            // This should ideally not happen if [Authorize] is used correctly,
            // but log a critical error if it does.
            _logger.LogCritical("TasksController: GetUserId failed - User authenticated but NameIdentifier claim is missing or empty.");
            throw new UnauthorizedAccessException("User ID claim not found. User is not authenticated or token is invalid.");
        }
        return Guid.Parse(userIdClaim);
    }

    // Helper to get the current authenticated user's role.
    // Retrieves the role claim from the user's principal.
    protected string? GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }

    // GET /api/tasks/my-tasks
    // Retrieves tasks assigned to the currently authenticated user.
    [HttpGet("my-tasks")]
    public async Task<IActionResult> GetMyTasks(
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = "dueDate",
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] Guid? projectId = null)
    {
        var userId = GetUserId();
        _logger.LogInformation("TasksController: Received request to get tasks for user ID '{UserId}'. Status: {Status}, ProjectId: {ProjectId}", userId, status, projectId);
        var tasks = await _tasksService.GetMyTasksAsync(userId, status, sortBy, sortOrder, projectId);
        _logger.LogInformation("TasksController: Retrieved {TaskCount} tasks for user ID '{UserId}'.", tasks.Count(), userId);
        return Ok(tasks);
    }

    // GET /api/tasks/{id}
    // Retrieves a specific task by ID, verifying user ownership.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        var userId = GetUserId();
        _logger.LogInformation("TasksController: Received request to get task ID '{TaskId}' for user ID '{UserId}'.", id, userId);
        var task = await _tasksService.GetTaskByIdAsync(id, userId);
        if (task == null)
        {
            _logger.LogWarning("TasksController: GetTaskById failed - Task ID '{TaskId}' not found or not assigned to user ID '{UserId}'.", id, userId);
            return NotFound("Task not found or not assigned to current user.");
        }
        _logger.LogInformation("TasksController: Successfully retrieved task ID '{TaskId}' for user ID '{UserId}'.", id, userId);
        return Ok(task);
    }

    // POST /api/tasks
    // Creates a new task assigned to the current user.
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequestDto request)
    {
        var userId = GetUserId();
        _logger.LogInformation("TasksController: Received request to create task '{TaskTitle}' for user ID '{UserId}', Project ID: '{ProjectId}'.", request.Title, userId, request.ProjectId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("TasksController: CreateTask failed for user ID '{UserId}' due to invalid model state. Errors: {ModelStateErrors}", userId, ModelState);
            return BadRequest(ModelState);
        }

        var createdTask = await _tasksService.CreateTaskAsync(userId, request);

        if (createdTask == null)
        {
            // Indicates a business logic error from the service (e.g., invalid ProjectId).
            _logger.LogWarning("TasksController: CreateTask failed for user ID '{UserId}'. Could not create task, possibly invalid Project ID '{ProjectId}'.", userId, request.ProjectId);
            return BadRequest("Could not create task. Ensure Project ID is valid.");
        }

        _logger.LogInformation("TasksController: Successfully created task '{TaskTitle}' with ID '{TaskId}' for user ID '{UserId}'.", createdTask.Title, createdTask.Id, userId);
        return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
    }

    // PUT /api/tasks/{id}
    // Updates an existing task, verifying user ownership or admin role.
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request)
    {
        var userId = GetUserId();
        var userRole = GetUserRole();
        _logger.LogInformation("TasksController: Received request to update task ID '{TaskId}' for user ID '{UserId}'. User Role: '{UserRole}'.", id, userId, userRole);


        if (!ModelState.IsValid)
        {
            _logger.LogWarning("TasksController: UpdateTask for ID '{TaskId}' failed due to invalid model state. Errors: {ModelStateErrors}", id, ModelState);
            return BadRequest(ModelState);
        }

        if (userId == Guid.Empty || string.IsNullOrEmpty(userRole))
        {
            // This case should ideally be caught by [Authorize] or GetUserId() throwing.
            _logger.LogError("TasksController: UpdateTask failed - User identity or role not found for user ID '{UserId}'.", userId);
            return Unauthorized("User identity or role not found.");
        }

        // Passes userId and userRole to the service for authorization.
        var updatedTask = await _tasksService.UpdateTaskAsync(id, userId, userRole, request);

        if (updatedTask == null)
        {
            // Covers task not found, unauthorized access, or invalid data (e.g., invalid ProjectId/AssignedPersonId).
            _logger.LogWarning("TasksController: UpdateTask failed for ID '{TaskId}' by user '{UserId}'. Task not found, unauthorized, or invalid data provided.", id, userId);
            return NotFound("Task not found, you are not authorized, or invalid data provided.");
        }

        _logger.LogInformation("TasksController: Successfully updated task ID '{TaskId}' by user ID '{UserId}'.", updatedTask.Id, userId);
        return Ok(updatedTask);
    }

    // DELETE /api/tasks/{id}
    // Deletes a task, verifying user ownership.
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var userId = GetUserId();
        _logger.LogInformation("TasksController: Received request to delete task ID '{TaskId}' by user ID '{UserId}'.", id, userId);
        var result = await _tasksService.DeleteTaskAsync(id, userId);

        if (!result)
        {
            _logger.LogWarning("TasksController: DeleteTask failed - Task ID '{TaskId}' not found or not assigned to user ID '{UserId}'.", id, userId);
            return NotFound("Task not found or not assigned to current user.");
        }

        _logger.LogInformation("TasksController: Successfully deleted task ID '{TaskId}' by user ID '{UserId}'.", id, userId);
        return NoContent(); // 204 No Content
    }

    // GET /api/tasks/users/{userId}
    // Admin route for retrieving all tasks for a specific user ID.
    [HttpGet("users/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTasksByUserId(
        Guid userId,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = "dueDate",
        [FromQuery] string? sortOrder = "asc")
    {
        _logger.LogInformation("TasksController: Admin request to get tasks for user ID '{TargetUserId}'. Status: {Status}", userId, status);
        // The service handles user existence and task retrieval.
        var tasks = await _tasksService.GetTasksByAssignedUserIdAsync(userId, status, sortBy, sortOrder);

        if (tasks == null)
        {
            _logger.LogWarning("TasksController: Admin request GetTasksByUserId failed - Tasks for user ID '{TargetUserId}' not found or user does not exist.", userId);
            return NotFound($"Tasks for user ID {userId} not found or user does not exist.");
        }

        _logger.LogInformation("TasksController: Admin successfully retrieved {TaskCount} tasks for user ID '{TargetUserId}'.", tasks.Count(), userId);
        return Ok(tasks);
    }
}