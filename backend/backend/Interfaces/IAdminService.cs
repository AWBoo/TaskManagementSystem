using backend.DTOS.Dashboard;
using backend.DTOS.Projects;
using backend.DTOS.Tasks;
using backend.DTOS.User;
using backend.Models;

namespace backend.Interfaces
{
    public interface IAdminService
    {
        // Retrieves a list of all users.
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        // Retrieves key statistics for the dashboard.
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        // Retrieves task counts by status.
        Task<IEnumerable<TaskStatusCountDto>> GetTasksByStatusAsync();
        // Retrieves task counts per user.
        Task<IEnumerable<UserTaskCountDto>> GetUsersTaskCountsAsync();
        // Retrieves task counts per project.
        Task<IEnumerable<ProjectTaskCountDto>> GetProjectsTaskCountsAsync();
        // Assigns a role to a user.
        Task<bool> AssignRoleToUserAsync(Guid userId, string roleName);
        // Removes a role from a user.
        Task<bool> RemoveRoleFromUserAsync(Guid userId, string roleName);
        // Retrieves all available roles.
        Task<IEnumerable<backend.Models.Role>> GetAllRolesAsync();
        // Updates a user's account status.
        Task<bool> UpdateUserStatusAsync(Guid userId, string newStatus);
        // Deletes a user account.
        Task<bool> DeleteUserAsync(Guid userId);
        // Retrieves a user's profile by ID.
        Task<UserDto?> GetUserByIdAsync(Guid userId);
        // Allows an admin to update a user's profile.
        Task<bool> AdminUpdateUserProfileAsync(Guid userId, UpdateProfileRequestDto request);
        // Allows an admin to change a user's password.
        Task<bool> AdminChangeUserPasswordAsync(Guid userId, AdminChangePasswordRequestDto request);
        // Retrieves a user's audit history.
        Task<IEnumerable<AuditEntry>> GetUserAuditHistoryAsync(Guid userId);
        // Retrieves the latest global audit entries.
        Task<IEnumerable<AuditEntry>> GetLatestAuditEntriesAsync(int count = 10);
    }
}