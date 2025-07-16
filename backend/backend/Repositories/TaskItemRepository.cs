using backend.Data;
using backend.DTOS.User.dashboard;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    // Repository for managing TaskItem data.
    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<TaskItemRepository> _logger; 

        public TaskItemRepository(ApplicationDBContext context, ILogger<TaskItemRepository> logger)
        {
            _context = context;
            _logger = logger; 
        }

        // Retrieves all tasks, including their assigned user and associated project.
        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            _logger.LogInformation("Retrieving all tasks.");
            try
            {
                var tasks = await _context.Tasks
                                         .Include(t => t.User)
                                         .Include(t => t.Project)
                                         .AsNoTracking()
                                         .ToListAsync();
                _logger.LogInformation("Retrieved {TaskCount} tasks.", tasks.Count);
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tasks.");
                throw;
            }
        }

        // Retrieves a task by its ID, including its assigned user and associated project.
        public async Task<TaskItem?> GetTaskByIdAsync(Guid taskId)
        {
            _logger.LogInformation("Retrieving task with ID '{TaskId}', including user and project.", taskId);
            try
            {
                var task = await _context.Tasks
                                         .Include(t => t.User)
                                         .Include(t => t.Project)
                                         .FirstOrDefaultAsync(t => t.Id == taskId);
                if (task == null)
                {
                    _logger.LogWarning("Task with ID '{TaskId}' not found.", taskId);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved task '{TaskTitle}' (ID: '{TaskId}').", task.Title, taskId);
                }
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task with ID '{TaskId}'.", taskId);
                throw;
            }
        }

        // Retrieves tasks assigned to a specific user, including their associated project.
        public async Task<IEnumerable<TaskItem>> GetTasksByUserIdAsync(Guid userId)
        {
            _logger.LogInformation("Retrieving tasks for user ID '{UserId}', including project.", userId);
            try
            {
                var tasks = await _context.Tasks
                                         .Where(t => t.UserId == userId)
                                         .Include(t => t.Project)
                                         .AsNoTracking()
                                         .ToListAsync();
                _logger.LogInformation("Retrieved {TaskCount} tasks for user ID '{UserId}'.", tasks.Count, userId);
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for user ID '{UserId}'.", userId);
                throw;
            }
        }

        // Retrieves tasks belonging to a specific project, including their assigned user.
        public async Task<IEnumerable<TaskItem>> GetTasksByProjectIdAsync(Guid projectId)
        {
            _logger.LogInformation("Retrieving tasks for project ID '{ProjectId}', including user.", projectId);
            try
            {
                var tasks = await _context.Tasks
                                         .Where(t => t.ProjectId == projectId)
                                         .Include(t => t.User)
                                         .AsNoTracking()
                                         .ToListAsync();
                _logger.LogInformation("Retrieved {TaskCount} tasks for project ID '{ProjectId}'.", tasks.Count, projectId);
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for project ID '{ProjectId}'.", projectId);
                throw;
            }
        }

        // Retrieves a dictionary of task counts by status.
        public async Task<Dictionary<string, int>> GetTaskCountsByStatusAsync()
        {
            _logger.LogInformation("Retrieving task counts by status.");
            try
            {
                var statusCounts = await _context.Tasks
                                                 .GroupBy(t => t.Status)
                                                 .Select(g => new { Status = g.Key, Count = g.Count() })
                                                 .ToDictionaryAsync(x => x.Status, x => x.Count);
                _logger.LogInformation("Retrieved {StatusCount} distinct task statuses with counts.", statusCounts.Count);
                return statusCounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task counts by status.");
                throw;
            }
        }

        // Checks if a task with the given ID exists.
        public async Task<bool> TaskExistsAsync(Guid taskId)
        {
            _logger.LogDebug("Checking if task with ID '{TaskId}' exists.", taskId);
            try
            {
                var exists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
                _logger.LogDebug("Task with ID '{TaskId}' exists: {Exists}.", taskId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of task with ID '{TaskId}'.", taskId);
                throw;
            }
        }

        // Adds a new task to the database.
        public async Task AddTaskAsync(TaskItem task)
        {
            _logger.LogInformation("Adding new task '{TaskTitle}' (ID: '{TaskId}') to the database.", task.Title, task.Id);
            try
            {
                await _context.Tasks.AddAsync(task);
                // No SaveChangesAsync here as it's typically called by the service layer
                _logger.LogInformation("Task '{TaskTitle}' (ID: '{TaskId}') added to context.", task.Title, task.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding task '{TaskTitle}' (ID: '{TaskId}').", task.Title, task.Id);
                throw;
            }
        }

        // Updates an existing task in the database.
        public async Task UpdateTaskAsync(TaskItem task)
        {
            _logger.LogInformation("Updating task '{TaskTitle}' (ID: '{TaskId}').", task.Title, task.Id);
            try
            {
                _context.Tasks.Update(task);
                // No SaveChangesAsync here as it's typically called by the service layer
                _logger.LogInformation("Task '{TaskTitle}' (ID: '{TaskId}') marked as updated in context.", task.Title, task.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task '{TaskTitle}' (ID: '{TaskId}').", task.Title, task.Id);
                throw;
            }
        }

        // Deletes a task from the database.
        public async Task DeleteTaskAsync(TaskItem task)
        {
            _logger.LogInformation("Deleting task '{TaskTitle}' (ID: '{TaskId}') from the database.", task.Title, task.Id);
            try
            {
                _context.Tasks.Remove(task);
                // No SaveChangesAsync here as it's typically called by the service layer
                _logger.LogInformation("Task '{TaskTitle}' (ID: '{TaskId}') marked for removal from context.", task.Title, task.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task '{TaskTitle}' (ID: '{TaskId}').", task.Title, task.Id);
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

        // Methods specifically tailored for the Project Service.
        // Retrieves tasks for a project, including project and user details.
        public async Task<IEnumerable<TaskItem>> GetTasksByProjectIdWithProjectAndUserAsync(Guid projectId)
        {
            _logger.LogInformation("Retrieving tasks for project ID '{ProjectId}' with project and user details.", projectId);
            try
            {
                var tasks = await _context.Tasks
                    .Where(t => t.ProjectId == projectId)
                    .Include(t => t.Project)
                    .Include(t => t.User)
                    .OrderBy(t => t.DueDate)
                    .ToListAsync();
                _logger.LogInformation("Retrieved {TaskCount} tasks for project ID '{ProjectId}'.", tasks.Count, projectId);
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for project ID '{ProjectId}' with project and user details.", projectId);
                throw;
            }
        }

        // Methods specifically tailored for the Task Service.
        // Retrieves tasks for a user with optional filtering and sorting, including project and user details.
        public async Task<IEnumerable<TaskItem>> GetTasksByUserIdWithProjectAndUserAsync(Guid userId, string? status, Guid? projectId, string? sortBy, string? sortOrder)
        {
            _logger.LogInformation("Retrieving tasks for user ID '{UserId}' with filters - Status: '{Status}', ProjectId: '{ProjectId}', SortBy: '{SortBy}', SortOrder: '{SortOrder}'.",
                userId, status ?? "N/A", projectId ?? Guid.Empty, sortBy ?? "N/A", sortOrder ?? "N/A");

            try
            {
                IQueryable<TaskItem> query = _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.User)
                    .Where(t => t.UserId == userId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.Status == status);
                    _logger.LogDebug("Filtering by status: '{Status}'.", status);
                }

                if (projectId.HasValue)
                {
                    query = query.Where(t => t.ProjectId == projectId.Value);
                    _logger.LogDebug("Filtering by project ID: '{ProjectId}'.", projectId.Value);
                }

                // Apply sorting
                query = ApplyTaskSorting(query, sortBy, sortOrder);

                var tasks = await query.ToListAsync();
                _logger.LogInformation("Retrieved {TaskCount} tasks for user ID '{UserId}' with specified filters.", tasks.Count, userId);
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for user ID '{UserId}' with filters.", userId);
                throw;
            }
        }

        // Retrieves a task by ID and UserId, including project and user details.
        public async Task<TaskItem?> GetTaskByIdAndUserIdWithProjectAndUserAsync(Guid taskId, Guid userId)
        {
            _logger.LogInformation("Retrieving task ID '{TaskId}' for user ID '{UserId}' with project and user details.", taskId, userId);
            try
            {
                var task = await _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.User)
                    .Where(t => t.Id == taskId && t.UserId == userId)
                    .FirstOrDefaultAsync();

                if (task == null)
                {
                    _logger.LogWarning("Task ID '{TaskId}' not found or does not belong to user ID '{UserId}'.", taskId, userId);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved task '{TaskTitle}' (ID: '{TaskId}') for user ID '{UserId}'.", task.Title, taskId, userId);
                }
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task ID '{TaskId}' for user ID '{UserId}' with project and user details.", taskId, userId);
                throw;
            }
        }

        // Retrieves a task by ID including project and user details.
        public async Task<TaskItem?> GetTaskItemByIdWithProjectAndUserAsync(Guid id)
        {
            _logger.LogInformation("Retrieving task with ID '{TaskId}' including project and user details.", id);
            try
            {
                var task = await _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    _logger.LogWarning("Task with ID '{TaskId}' not found (with project and user).", id);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved task '{TaskTitle}' (ID: '{TaskId}') with project and user details.", task.Title, id);
                }
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task with ID '{TaskId}' with project and user details.", id);
                throw;
            }
        }

        // Retrieves a task by ID, filtered by user access or admin status.
        public async Task<TaskItem?> GetTaskItemByIdFilteredByUserAsync(Guid taskId, Guid? userId, bool isAdmin)
        {
            _logger.LogInformation("Retrieving task ID '{TaskId}' filtered by user access. User ID: '{UserId}', IsAdmin: {IsAdmin}.", taskId, userId, isAdmin);
            try
            {
                IQueryable<TaskItem> query = _context.Tasks.AsQueryable();
                if (!isAdmin && userId.HasValue)
                {
                    query = query.Where(t => t.UserId == userId.Value);
                    _logger.LogDebug("Applying user ID '{UserId}' filter for non-admin request.", userId.Value);
                }
                var task = await query.FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                {
                    _logger.LogWarning("Task ID '{TaskId}' not found or access denied for user ID '{UserId}' (IsAdmin: {IsAdmin}).", taskId, userId, isAdmin);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved filtered task '{TaskTitle}' (ID: '{TaskId}').", task.Title, taskId);
                }
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task ID '{TaskId}' filtered by user access.", taskId);
                throw;
            }
        }

        // Retrieves a task by ID and UserId without including related entities.
        public async Task<TaskItem?> GetTaskItemByIdAndUserIdAsync(Guid taskId, Guid userId)
        {
            _logger.LogInformation("Retrieving task ID '{TaskId}' by user ID '{UserId}'.", taskId, userId);
            try
            {
                var task = await _context.Tasks
                    .Where(t => t.Id == taskId && t.UserId == userId)
                    .FirstOrDefaultAsync();

                if (task == null)
                {
                    _logger.LogWarning("Task ID '{TaskId}' not found or does not belong to user ID '{UserId}'.", taskId, userId);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved task '{TaskTitle}' (ID: '{TaskId}') by user ID '{UserId}'.", task.Title, taskId, userId);
                }
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task ID '{TaskId}' by user ID '{UserId}'.", taskId, userId);
                throw;
            }
        }

        // Retrieves tasks assigned to a specific user with optional filters and sorting, including project and user details.
        public async Task<IEnumerable<TaskItem>> GetTasksByAssignedUserIdWithProjectAndUserAsync(Guid assignedUserId, string? status, string? sortBy, string? sortOrder)
        {
            _logger.LogInformation("Retrieving tasks assigned to user ID '{AssignedUserId}' with filters - Status: '{Status}', SortBy: '{SortBy}', SortOrder: '{SortOrder}'.",
                assignedUserId, status ?? "N/A", sortBy ?? "N/A", sortOrder ?? "N/A");

            try
            {
                IQueryable<TaskItem> query = _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.User)
                    .Where(t => t.UserId == assignedUserId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.Status == status);
                    _logger.LogDebug("Filtering by status: '{Status}'.", status);
                }

                // Apply sorting
                query = ApplyTaskSorting(query, sortBy, sortOrder);

                var tasks = await query.ToListAsync();
                _logger.LogInformation("Retrieved {TaskCount} tasks assigned to user ID '{AssignedUserId}'.", tasks.Count, assignedUserId);
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks assigned to user ID '{AssignedUserId}' with filters.", assignedUserId);
                throw;
            }
        }

        // Helper method for applying sorting logic to task queries.
        private static IQueryable<TaskItem> ApplyTaskSorting(IQueryable<TaskItem> query, string? sortBy, string? sortOrder)
        {
            // Log sorting parameters
            // This method is static, so direct ILogger injection isn't possible.
            // In a real application, you might pass ILogger or use a static logger from a utility class,
            // or ensure logging happens before calling this static helper.
            // For now, removing direct logger calls as per the "no AI-like comments/setup" rule for the code itself.

            switch (sortBy?.ToLower())
            {
                case "status":
                    query = (sortOrder?.ToLower() == "desc") ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status);
                    break;
                case "duedate":
                    query = (sortOrder?.ToLower() == "desc") ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate);
                    break;
                case "project":
                    query = (sortOrder?.ToLower() == "desc") ? query.OrderByDescending(t => t.Project!.Name) : query.OrderBy(t => t.Project!.Name);
                    break;
                case "createdat":
                    query = (sortOrder?.ToLower() == "desc") ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt);
                    break;
                default: // Default to DueDate ascending
                    query = query.OrderBy(t => t.DueDate);
                    break;
            }
            return query;
        }

        // Methods specifically tailored for the User Service Dashboard.
        // Retrieves the total count of tasks for a user.
        public async Task<int> GetTotalTasksCountByUserIdAsync(Guid userId)
        {
            _logger.LogInformation("Calculating total tasks count for user ID '{UserId}'.", userId);
            try
            {
                var count = await _context.Tasks
                                         .Where(t => t.UserId == userId)
                                         .CountAsync();
                _logger.LogInformation("User ID '{UserId}': Total tasks = {Count}.", userId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total tasks count for user ID '{UserId}'.", userId);
                throw;
            }
        }

        // Retrieves the count of tasks due soon for a user, excluding specified statuses.
        public async Task<int> GetTasksDueSoonCountByUserIdAsync(Guid userId, DateTime dueTodayOrLater, DateTime dueTomorrowOrEarlier, string[] excludeStatuses)
        {
            _logger.LogInformation("Calculating tasks due soon count for user ID '{UserId}' (Due from {DueToday} to {DueTomorrow}).", userId, dueTodayOrLater.ToShortDateString(), dueTomorrowOrEarlier.ToShortDateString());
            try
            {
                var count = await _context.Tasks
                                         .Where(t => t.UserId == userId &&
                                                     !excludeStatuses.Contains(t.Status) &&
                                                     t.DueDate.Date >= dueTodayOrLater.Date &&
                                                     t.DueDate.Date <= dueTomorrowOrEarlier.Date)
                                         .CountAsync();
                _logger.LogInformation("User ID '{UserId}': Tasks due soon = {Count}.", userId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating tasks due soon count for user ID '{UserId}'.", userId);
                throw;
            }
        }

        // Retrieves the count of overdue tasks for a user, excluding specified statuses.
        public async Task<int> GetOverdueTasksCountByUserIdAsync(Guid userId, DateTime pastDate, string[] excludeStatuses)
        {
            _logger.LogInformation("Calculating overdue tasks count for user ID '{UserId}' (Past date: {PastDate}).", userId, pastDate.ToShortDateString());
            try
            {
                var count = await _context.Tasks
                                         .Where(t => t.UserId == userId &&
                                                     !excludeStatuses.Contains(t.Status) &&
                                                     t.DueDate.Date < pastDate.Date)
                                         .CountAsync();
                _logger.LogInformation("User ID '{UserId}': Overdue tasks = {Count}.", userId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating overdue tasks count for user ID '{UserId}'.", userId);
                throw;
            }
        }

        // Retrieves task status counts for a user.
        public async Task<List<TaskStatusCountDto>> GetTaskStatusCountsByUserIdAsync(Guid userId)
        {
            _logger.LogInformation("Retrieving task status counts for user ID '{UserId}'.", userId);
            try
            {
                var statusCounts = await _context.Tasks
                                                 .Where(t => t.UserId == userId)
                                                 .GroupBy(t => t.Status)
                                                 .Select(g => new TaskStatusCountDto
                                                 {
                                                     Status = g.Key,
                                                     Count = g.Count()
                                                 })
                                                 .ToListAsync();
                _logger.LogInformation("User ID '{UserId}': Retrieved {Count} task status counts.", userId, statusCounts.Count);
                return statusCounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task status counts for user ID '{UserId}'.", userId);
                throw;
            }
        }

        // Retrieves task counts per project for a user, including project names.
        public async Task<List<DTOS.User.dashboard.ProjectTaskCountDto>> GetProjectTaskCountsByUserIdWithProjectNameAsync(Guid userId)
        {
            _logger.LogInformation("Retrieving project task counts for user ID '{UserId}'.", userId);
            try
            {
                var projectTaskCounts = await _context.Tasks
                                                     .Include(t => t.Project)
                                                     .Where(t => t.UserId == userId && t.Project != null)
                                                     .GroupBy(t => new { t.ProjectId, ProjectName = t.Project!.Name })
                                                     .Select(g => new DTOS.User.dashboard.ProjectTaskCountDto
                                                     {
                                                         ProjectName = g.Key.ProjectName,
                                                         TaskCount = g.Count()
                                                     })
                                                     .OrderBy(p => p.ProjectName)
                                                     .ToListAsync();
                _logger.LogInformation("User ID '{UserId}': Retrieved {Count} project task counts.", userId, projectTaskCounts.Count);
                return projectTaskCounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project task counts for user ID '{UserId}'.", userId);
                throw;
            }
        }
    }
}