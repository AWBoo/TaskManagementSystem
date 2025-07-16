using backend.DTOS.Tasks;
using backend.Interfaces;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.Services
{
    // Service for managing task-related operations.
    public class TasksService : ITasksService
    {
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TasksService> _logger;

        public TasksService(
            ITaskItemRepository taskItemRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            ILogger<TasksService> logger)
        {
            _taskItemRepository = taskItemRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
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

        // Retrieves tasks assigned to a specific user with optional filters and sorting.
        public async Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(Guid userId, string? status, string? sortBy, string? sortOrder, Guid? projectId)
        {
            _logger.LogInformation("Retrieving tasks for user ID '{UserId}' with filters - Status: '{Status}', ProjectId: '{ProjectId}', SortBy: '{SortBy}', SortOrder: '{SortOrder}'.",
                userId, status ?? "N/A", projectId ?? Guid.Empty, sortBy ?? "N/A", sortOrder ?? "N/A");

            var tasks = await _taskItemRepository.GetTasksByUserIdWithProjectAndUserAsync(userId, status, projectId, sortBy, sortOrder);
            var taskDtos = tasks.Select(t => MapTaskToDto(t)).ToList();
            _logger.LogInformation("Retrieved {TaskCount} tasks for user ID '{UserId}'.", taskDtos.Count, userId);
            return taskDtos;
        }

        // Retrieves a single task by ID, ensuring it belongs to the specified user.
        public async Task<TaskResponseDto?> GetTaskByIdAsync(Guid taskId, Guid userId)
        {
            _logger.LogInformation("Attempting to retrieve task ID '{TaskId}' for user ID '{UserId}'.", taskId, userId);
            var task = await _taskItemRepository.GetTaskByIdAndUserIdWithProjectAndUserAsync(taskId, userId);
            if (task == null)
            {
                _logger.LogWarning("Task ID '{TaskId}' not found or does not belong to user ID '{UserId}'.", taskId, userId);
                return null;
            }
            _logger.LogInformation("Successfully retrieved task '{TaskTitle}' (ID: '{TaskId}') for user ID '{UserId}'.", task.Title, taskId, userId);
            return MapTaskToDto(task);
        }

        // Creates a new task.
        public async Task<TaskResponseDto?> CreateTaskAsync(Guid userId, CreateTaskRequestDto request)
        {
            _logger.LogInformation("Attempting to create new task '{TaskTitle}' for project ID '{ProjectId}' by user ID '{UserId}'.",
                request.Title, request.ProjectId, userId);

            // Verifies the existence of the associated project.
            var projectExists = await _projectRepository.ProjectExistsAsync(request.ProjectId);
            if (!projectExists)
            {
                _logger.LogWarning("Create task failed - Project ID '{ProjectId}' not found.", request.ProjectId);
                return null;
            }

            var newTask = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate.ToUniversalTime(),
                Status = "In Progress",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ProjectId = request.ProjectId,
                UserId = userId
            };

            await _taskItemRepository.AddTaskAsync(newTask);
            await _taskItemRepository.SaveChangesAsync();

            // Returns the created task with associated project and user details.
            var createdTask = await _taskItemRepository.GetTaskItemByIdWithProjectAndUserAsync(newTask.Id);
            if (createdTask == null)
            {
                _logger.LogError("Failed to retrieve created task ID '{TaskId}' after adding to repository. Data inconsistency.", newTask.Id);
                return null;
            }

            _logger.LogInformation("Task '{TaskTitle}' (ID: '{TaskId}') created successfully in project '{ProjectId}'.",
                createdTask.Title, createdTask.Id, createdTask.ProjectId);
            return MapTaskToDto(createdTask);
        }

        // Updates an existing task.
        public async Task<TaskResponseDto?> UpdateTaskAsync(Guid taskId, Guid userId, string currentUserRole, UpdateTaskRequest request)
        {
            _logger.LogInformation("Attempting to update task ID '{TaskId}' by user ID '{UserId}' with role '{CurrentUserRole}'.",
                taskId, userId, currentUserRole);

            bool isAdmin = currentUserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            // Fetches the task, applying user-specific filter if not an admin.
            var task = await _taskItemRepository.GetTaskItemByIdFilteredByUserAsync(taskId, userId, isAdmin);

            if (task == null)
            {
                _logger.LogWarning("Update task failed - Task ID '{TaskId}' not found or unauthorized for user ID '{UserId}'.", taskId, userId);
                return null;
            }

            // Verifies new ProjectId if changed.
            if (task.ProjectId != request.ProjectId)
            {
                _logger.LogInformation("Task ID '{TaskId}' project being changed from '{OldProjectId}' to '{NewProjectId}'.",
                    taskId, task.ProjectId, request.ProjectId);

                if (!await _projectRepository.ProjectExistsAsync(request.ProjectId))
                {
                    _logger.LogWarning("Update task failed - New Project ID '{NewProjectId}' not found for task '{TaskId}'.", request.ProjectId, taskId);
                    return null;
                }
            }

            _logger.LogDebug("Task '{TaskId}' Old Values - Title: '{OldTitle}', Description: '{OldDescription}', DueDate: '{OldDueDate}', Status: '{OldStatus}', ProjectId: '{OldProjectId}', UserId: '{OldUserId}'.",
                taskId, task.Title, task.Description, task.DueDate, task.Status, task.ProjectId, task.UserId);

            task.Title = request.Title;
            task.Description = request.Description;
            task.DueDate = request.DueDate.ToUniversalTime();
            task.Status = request.Status;
            task.ProjectId = request.ProjectId;

            // Handles task reassignment, checking for admin role.
            if (request.UserId.HasValue && task.UserId != request.UserId.Value)
            {
                _logger.LogInformation("Task ID '{TaskId}' assigned user being changed from '{OldAssignedUser}' to '{NewAssignedUser}'.",
                    taskId, task.UserId, request.UserId.Value);

                if (!isAdmin)
                {
                    _logger.LogWarning("Update task failed - User ID '{UserId}' is not authorized to reassign task ID '{TaskId}'. Only Admins can reassign tasks.", userId, taskId);
                    return null;
                }
                // Verifies the existence of the new assigned user.
                if (!await _userRepository.UserExistsByIdAsync(request.UserId.Value))
                {
                    _logger.LogWarning("Update task failed - New assigned user ID '{NewUserId}' not found for task '{TaskId}'.", request.UserId.Value, taskId);
                    return null;
                }
                task.UserId = request.UserId.Value;
            }

            task.UpdatedAt = DateTime.UtcNow;

            await _taskItemRepository.UpdateTaskAsync(task);
            await _taskItemRepository.SaveChangesAsync();

            // Returns the updated task with associated project and user details.
            var updatedTaskEntity = await _taskItemRepository.GetTaskItemByIdWithProjectAndUserAsync(task.Id);
            if (updatedTaskEntity == null)
            {
                _logger.LogError("Failed to retrieve updated task ID '{TaskId}' after saving changes. Data inconsistency.", taskId);
                return null;
            }

            _logger.LogInformation("Task '{TaskTitle}' (ID: '{TaskId}') updated successfully.", updatedTaskEntity.Title, updatedTaskEntity.Id);
            return MapTaskToDto(updatedTaskEntity);
        }

        // Deletes a task, ensuring user ownership.
        public async Task<bool> DeleteTaskAsync(Guid taskId, Guid userId)
        {
            _logger.LogInformation("Attempting to delete task ID '{TaskId}' by user ID '{UserId}'.", taskId, userId);
            var task = await _taskItemRepository.GetTaskItemByIdAndUserIdAsync(taskId, userId);

            if (task == null)
            {
                _logger.LogWarning("Delete task failed - Task ID '{TaskId}' not found or does not belong to user ID '{UserId}'.", taskId, userId);
                return false;
            }

            await _taskItemRepository.DeleteTaskAsync(task);
            await _taskItemRepository.SaveChangesAsync();
            _logger.LogInformation("Task '{TaskTitle}' (ID: '{TaskId}') successfully deleted by user ID '{UserId}'.", task.Title, taskId, userId);
            return true;
        }

        // Retrieves tasks assigned to any specified user ID (typically for admin use).
        public async Task<IEnumerable<TaskResponseDto>?> GetTasksByAssignedUserIdAsync(Guid assignedUserId, string? status, string? sortBy, string? sortOrder)
        {
            _logger.LogInformation("Retrieving tasks assigned to user ID '{AssignedUserId}' with filters - Status: '{Status}', SortBy: '{SortBy}', SortOrder: '{SortOrder}'.",
                assignedUserId, status ?? "N/A", sortBy ?? "N/A", sortOrder ?? "N/A");

            // Verifies the existence of the assigned user.
            var userExists = await _userRepository.UserExistsByIdAsync(assignedUserId);
            if (!userExists)
            {
                _logger.LogWarning("Get tasks by assigned user failed - Assigned user ID '{AssignedUserId}' not found.", assignedUserId);
                return null;
            }

            var tasks = await _taskItemRepository.GetTasksByAssignedUserIdWithProjectAndUserAsync(assignedUserId, status, sortBy, sortOrder);
            var taskDtos = tasks.Select(t => MapTaskToDto(t)).ToList();
            _logger.LogInformation("Retrieved {TaskCount} tasks assigned to user ID '{AssignedUserId}'.", taskDtos.Count, assignedUserId);
            return taskDtos;
        }
    }
}