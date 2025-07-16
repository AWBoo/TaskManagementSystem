using backend.DTOS.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Interfaces
{
    public interface ITasksService
    {
        // Retrieves tasks assigned to a specific user, with optional filters.
        Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(Guid userId, string? status, string? sortBy, string? sortOrder, Guid? projectId);
        // Retrieves a specific task by ID for a given user.
        Task<TaskResponseDto?> GetTaskByIdAsync(Guid taskId, Guid userId);
        // Creates a new task assigned to a user.
        Task<TaskResponseDto?> CreateTaskAsync(Guid userId, CreateTaskRequestDto request);
        // Updates an existing task, with authorization checks.
        Task<TaskResponseDto?> UpdateTaskAsync(Guid taskId, Guid userId, string userRole, UpdateTaskRequest request);
        // Deletes a task, with ownership verification.
        Task<bool> DeleteTaskAsync(Guid taskId, Guid userId);
        // Retrieves tasks assigned to a specific user ID, with optional filters.
        Task<IEnumerable<TaskResponseDto>?> GetTasksByAssignedUserIdAsync(Guid userId, string? status, string? sortBy, string? sortOrder);
    }
}