using backend.DTOS.User.dashboard;
using backend.DTOS.Projects; 
using backend.Models;



namespace backend.Repositories.Interfaces
{
    // Defines operations for accessing and managing TaskItem entities.
    public interface ITaskItemRepository
    {
        // Retrieves all tasks.
        Task<IEnumerable<TaskItem>> GetAllTasksAsync();
        // Retrieves a task by its ID.
        Task<TaskItem?> GetTaskByIdAsync(Guid taskId);
        // Retrieves all tasks assigned to a specific user.
        Task<IEnumerable<TaskItem>> GetTasksByUserIdAsync(Guid userId);
        // Retrieves all tasks belonging to a specific project.
        Task<IEnumerable<TaskItem>> GetTasksByProjectIdAsync(Guid projectId);
        // Retrieves a dictionary of task counts by status.
        Task<Dictionary<string, int>> GetTaskCountsByStatusAsync();
        // Checks if a task with the given ID exists.
        Task<bool> TaskExistsAsync(Guid taskId);
        // Adds a new task to the repository.
        Task AddTaskAsync(TaskItem task);
        // Updates an existing task in the repository.
        Task UpdateTaskAsync(TaskItem task);
        // Deletes a task from the repository.
        Task DeleteTaskAsync(TaskItem task);
        // Saves all changes made in the current context.
        Task SaveChangesAsync();


        // Methods specifically tailored for the Project Service.
        Task<IEnumerable<TaskItem>> GetTasksByProjectIdWithProjectAndUserAsync(Guid projectId);

        // Methods specifically tailored for the Task Service.
        Task<IEnumerable<TaskItem>> GetTasksByUserIdWithProjectAndUserAsync(Guid userId, string? status, Guid? projectId, string? sortBy, string? sortOrder);
        // Retrieves a task by ID and UserId, including project and user details.
        Task<TaskItem?> GetTaskByIdAndUserIdWithProjectAndUserAsync(Guid taskId, Guid userId);
        // Retrieves a task by ID including project and user details.
        Task<TaskItem?> GetTaskItemByIdWithProjectAndUserAsync(Guid id);
        // Retrieves a task by ID, filtered by user access or admin status.
        Task<TaskItem?> GetTaskItemByIdFilteredByUserAsync(Guid taskId, Guid? userId, bool isAdmin);
        // Retrieves a task by ID and UserId without including related entities.
        Task<TaskItem?> GetTaskItemByIdAndUserIdAsync(Guid taskId, Guid userId);
        // Retrieves tasks assigned to a specific user with optional filters and sorting, including project and user details.
        Task<IEnumerable<TaskItem>> GetTasksByAssignedUserIdWithProjectAndUserAsync(Guid assignedUserId, string? status, string? sortBy, string? sortOrder);

        
        // Methods specifically tailored for the User Service Dashboard.
        Task<int> GetTotalTasksCountByUserIdAsync(Guid userId);
        // Retrieves the count of tasks due soon for a user, excluding specified statuses.
        Task<int> GetTasksDueSoonCountByUserIdAsync(Guid userId, DateTime dueTodayOrLater, DateTime dueTomorrowOrEarlier, string[] excludeStatuses);
        // Retrieves the count of overdue tasks for a user, excluding specified statuses.
        Task<int> GetOverdueTasksCountByUserIdAsync(Guid userId, DateTime pastDate, string[] excludeStatuses);
        // Retrieves task status counts for a user.
        Task<List<TaskStatusCountDto>> GetTaskStatusCountsByUserIdAsync(Guid userId);
        // Retrieves task counts per project for a user, including project names.
        Task<List<DTOS.User.dashboard.ProjectTaskCountDto>> GetProjectTaskCountsByUserIdWithProjectNameAsync(Guid userId);
    }
}