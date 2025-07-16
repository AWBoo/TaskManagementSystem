using backend.DTOS.Dashboard;
using backend.DTOS.Projects;
using backend.DTOS.Tasks;
using backend.DTOS.User;
using backend.Interfaces;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.Services
{
    // Service for administrative operations related to users, projects, and tasks.
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IAuditEntryRepository _auditEntryRepository;
        private readonly ILogger<AdminService> _logger;

        // Injects repository dependencies for data access.
        public AdminService(
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            ITaskItemRepository taskItemRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IAuditEntryRepository auditEntryRepository,
            ILogger<AdminService> logger)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _taskItemRepository = taskItemRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _auditEntryRepository = auditEntryRepository;
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
                UserId = task.UserId,
                AssignedUserEmail = task.User?.Email
            };
        }

        // Retrieves all users with their roles and task/project counts.
        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            _logger.LogInformation("AdminService: Fetching all users with roles and tasks.");
            var users = await _userRepository.GetAllUsersWithRolesAndTasksAsync();
            _logger.LogInformation("AdminService: Retrieved {UserCount} users.", users.Count());
            return users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                Roles = u.UserRoles?.Select(ur => ur.Role?.Name)?.ToList() ?? new List<string>(),
                ProjectCount = u.Tasks?.Select(t => t.ProjectId).Distinct().Count() ?? 0,
                TaskCount = u.Tasks?.Count() ?? 0,
                Name = u.Name,
                Status = u.Status
            }).ToList();
        }

        // Retrieves overall dashboard statistics.
        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            _logger.LogInformation("AdminService: Calculating overall dashboard statistics.");
            var totalUsers = await _userRepository.GetAllUsersWithRolesAndTasksAsync();
            var totalProjects = await _projectRepository.GetAllProjectsAsync();
            var totalTasks = await _taskItemRepository.GetAllTasksAsync();

            _logger.LogInformation("AdminService: Dashboard stats calculated - Users: {TotalUsers}, Projects: {TotalProjects}, Tasks: {TotalTasks}.",
                totalUsers.Count(), totalProjects.Count(), totalTasks.Count());

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers.Count(),
                TotalProjects = totalProjects.Count(),
                TotalTasks = totalTasks.Count()
            };
        }

        // Retrieves task counts grouped by status.
        public async Task<IEnumerable<TaskStatusCountDto>> GetTasksByStatusAsync()
        {
            _logger.LogInformation("AdminService: Fetching task counts by status.");
            var statusCountsDictionary = await _taskItemRepository.GetTaskCountsByStatusAsync();
            _logger.LogInformation("AdminService: Retrieved task counts for {Count} statuses.", statusCountsDictionary.Count);
            return statusCountsDictionary.Select(kvp => new TaskStatusCountDto
            {
                Status = kvp.Key,
                Count = kvp.Value
            }).ToList();
        }

        // Retrieves task counts for each user.
        public async Task<IEnumerable<UserTaskCountDto>> GetUsersTaskCountsAsync()
        {
            _logger.LogInformation("AdminService: Fetching task counts per user.");
            var users = await _userRepository.GetAllUsersWithRolesAndTasksAsync();
            _logger.LogInformation("AdminService: Retrieved task counts for {UserCount} users.", users.Count());
            return users.Select(u => new UserTaskCountDto
            {
                UserId = u.Id,
                UserEmail = u.Email,
                TaskCount = u.Tasks?.Count() ?? 0
            }).OrderByDescending(u => u.TaskCount).ToList();
        }

        // Retrieves task counts for each project.
        public async Task<IEnumerable<ProjectTaskCountDto>> GetProjectsTaskCountsAsync()
        {
            _logger.LogInformation("AdminService: Fetching task counts per project.");
            var projectTaskCounts = await _projectRepository.GetProjectTaskCountsAsync();
            _logger.LogInformation("AdminService: Retrieved task counts for {ProjectCount} projects.", projectTaskCounts.Count());
            return projectTaskCounts;
        }

        // Assigns a specific role to a user.
        public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName)
        {
            _logger.LogInformation("AdminService: Attempting to assign role '{RoleName}' to user ID '{UserId}'.", roleName, userId);

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("AdminService: AssignRoleToUser failed. User ID '{UserId}' not found.", userId);
                return false;
            }

            var role = await _roleRepository.GetRoleByNameAsync(roleName);
            if (role == null)
            {
                _logger.LogWarning("AdminService: AssignRoleToUser failed. Role '{RoleName}' not found.", roleName);
                return false;
            }

            var existingUserRole = await _userRoleRepository.UserRoleExistsAsync(userId, role.Id);
            if (existingUserRole)
            {
                _logger.LogWarning("AdminService: AssignRoleToUser failed. User ID '{UserId}' already has role '{RoleName}'.", userId, roleName);
                return false;
            }

            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = role.Id,
                AssignedAt = DateTime.UtcNow,
                User = user,
                Role = role
            };

            await _userRoleRepository.AddUserRoleAsync(userRole);
            await _userRoleRepository.SaveChangesAsync();
            _logger.LogInformation("AdminService: Successfully assigned role '{RoleName}' to user ID '{UserId}'.", roleName, userId);
            return true;
        }

        // Removes a specific role from a user.
        public async Task<bool> RemoveRoleFromUserAsync(Guid userId, string roleName)
        {
            _logger.LogInformation("AdminService: Attempting to remove role '{RoleName}' from user ID '{UserId}'.", roleName, userId);

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("AdminService: RemoveRoleFromUser failed. User ID '{UserId}' not found.", userId);
                return false;
            }

            var role = await _roleRepository.GetRoleByNameAsync(roleName);
            if (role == null)
            {
                _logger.LogWarning("AdminService: RemoveRoleFromUser failed. Role '{RoleName}' not found.", roleName);
                return false;
            }

            var userRole = await _userRoleRepository.GetUserRoleAsync(userId, role.Id);
            if (userRole == null)
            {
                _logger.LogWarning("AdminService: RemoveRoleFromUser failed. User ID '{UserId}' does not have role '{RoleName}'.", userId, roleName);
                return false;
            }

            await _userRoleRepository.DeleteUserRoleAsync(userRole);
            await _userRoleRepository.SaveChangesAsync();
            _logger.LogInformation("AdminService: Successfully removed role '{RoleName}' from user ID '{UserId}'.", roleName, userId);
            return true;
        }

        // Retrieves all available roles.
        public async Task<IEnumerable<backend.Models.Role>> GetAllRolesAsync()
        {
            _logger.LogInformation("AdminService: Fetching all available roles.");
            var roles = await _roleRepository.GetAllRolesAsync();
            _logger.LogInformation("AdminService: Retrieved {RoleCount} roles.", roles.Count());
            return roles;
        }

        // Updates a user's account status.
        public async Task<bool> UpdateUserStatusAsync(Guid userId, string newStatus)
        {
            _logger.LogInformation("AdminService: Attempting to update status for user ID '{UserId}' to '{NewStatus}'.", userId, newStatus);

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("AdminService: UpdateUserStatus failed. User ID '{UserId}' not found.", userId);
                return false;
            }

            user.Status = newStatus;
            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();
            _logger.LogInformation("AdminService: User ID '{UserId}' status successfully updated to '{NewStatus}'.", userId, newStatus);
            return true;
        }

        // Deletes a user account.
        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            _logger.LogInformation("AdminService: Attempting to delete user ID '{UserId}'.", userId);
            var user = await _userRepository.GetUserByIdWithRolesAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("AdminService: DeleteUser failed. User ID '{UserId}' not found.", userId);
                return false;
            }

            await _userRepository.DeleteUserAsync(user);
            await _userRepository.SaveChangesAsync();
            _logger.LogInformation("AdminService: User ID '{UserId}' successfully deleted.", userId);
            return true;
        }

        // Retrieves a UserDto by user ID.
        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            _logger.LogInformation("AdminService: Fetching user DTO for user ID '{UserId}'.", userId);
            var userDto = await _userRepository.GetUserDtoByIdAsync(userId);
            if (userDto == null)
            {
                _logger.LogWarning("AdminService: GetUserById failed. User DTO for ID '{UserId}' not found.", userId);
            }
            else
            {
                _logger.LogInformation("AdminService: User DTO for ID '{UserId}' retrieved.", userId);
            }
            return userDto;
        }

        // Allows an admin to update a user's profile.
        public async Task<bool> AdminUpdateUserProfileAsync(Guid userId, UpdateProfileRequestDto request)
        {
            _logger.LogInformation("AdminService: Attempting to update profile for user ID '{UserId}'.", userId);

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("AdminService: AdminUpdateUserProfile failed. User ID '{UserId}' not found.", userId);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                user.Name = request.Name;
            }
            if (!string.IsNullOrWhiteSpace(request.Email) && user.Email != request.Email)
            {
                var existingUserWithEmail = await _userRepository.UserExistsByEmailExcludingIdAsync(request.Email, userId);
                if (existingUserWithEmail)
                {
                    _logger.LogWarning("AdminService: AdminUpdateUserProfile failed. Email '{Email}' already in use by another user.", request.Email);
                    return false;
                }
                user.Email = request.Email;
            }

            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();
            _logger.LogInformation("AdminService: Profile for user ID '{UserId}' successfully updated.", userId);
            return true;
        }

        // Allows an admin to change a user's password.
        public async Task<bool> AdminChangeUserPasswordAsync(Guid userId, AdminChangePasswordRequestDto request)
        {
            _logger.LogInformation("AdminService: Attempting to change password for user ID '{UserId}'.", userId);

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("AdminService: AdminChangeUserPassword failed. User ID '{UserId}' not found.", userId);
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            try
            {
                await _userRepository.UpdateUserAsync(user);
                await _userRepository.SaveChangesAsync();
                _logger.LogInformation("AdminService: Password for user ID '{UserId}' successfully changed.", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminService: An error occurred while changing password for user ID '{UserId}'.", userId);
                return false;
            }
        }

        // Retrieves a user's audit history.
        public async Task<IEnumerable<AuditEntry>> GetUserAuditHistoryAsync(Guid userId)
        {
            _logger.LogInformation("AdminService: Fetching audit history for user ID '{UserId}'.", userId);
            var history = await _auditEntryRepository.GetUserAuditHistoryAsync(userId, 10);
            _logger.LogInformation("AdminService: Retrieved {EntryCount} audit entries for user ID '{UserId}'.", history.Count(), userId);
            return history;
        }

        // Retrieves the latest global audit entries.
        public async Task<IEnumerable<AuditEntry>> GetLatestAuditEntriesAsync(int count = 10)
        {
            _logger.LogInformation("AdminService: Fetching latest {Count} global audit entries.", count);
            var latestEntries = await _auditEntryRepository.GetLatestAuditEntriesAsync(count);
            _logger.LogInformation("AdminService: Retrieved {EntryCount} latest global audit entries.", latestEntries.Count());
            return latestEntries;
        }
    }
}