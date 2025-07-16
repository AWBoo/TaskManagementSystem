using backend.DTOS.User;
using backend.DTOS.User.dashboard;
using backend.Interfaces;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    // Service for managing user profiles and dashboard data.
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly ILogger<UserService> _logger; 

        public UserService(IUserRepository userRepository, ITaskItemRepository taskItemRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _taskItemRepository = taskItemRepository;
            _logger = logger; 
        }

        // Retrieves a user's profile information.
        public async Task<UserDto?> GetUserProfileAsync(Guid userId)
        {
            _logger.LogInformation("Attempting to retrieve user profile for user ID '{UserId}'.", userId);
            var userProfile = await _userRepository.GetUserDtoByIdAsync(userId);
            if (userProfile == null)
            {
                _logger.LogWarning("User profile not found for user ID '{UserId}'.", userId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved user profile for user ID '{UserId}'.", userId);
            }
            return userProfile;
        }

        // Updates a user's profile information.
        public async Task<bool> UpdateUserProfileAsync(Guid userId, UpdateProfileRequestDto request)
        {
            _logger.LogInformation("Attempting to update profile for user ID '{UserId}'.", userId);
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Update profile failed: User with ID '{UserId}' not found.", userId);
                return false;
            }

            // Log current values before potential changes
            _logger.LogDebug("Current user profile for ID '{UserId}': Name='{Name}', Email='{Email}'.", user.Id, user.Name, user.Email);


            // Updates user name if provided.
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                if (user.Name != request.Name)
                {
                    _logger.LogInformation("Updating user ID '{UserId}' name from '{OldName}' to '{NewName}'.", userId, user.Name, request.Name);
                    user.Name = request.Name;
                }
            }
            // Updates user email if provided and different, checking for uniqueness.
            if (!string.IsNullOrWhiteSpace(request.Email) && user.Email != request.Email)
            {
                _logger.LogInformation("Attempting to change email for user ID '{UserId}' from '{OldEmail}' to '{NewEmail}'.", userId, user.Email, request.Email);
                var existingUserWithEmail = await _userRepository.UserExistsByEmailExcludingIdAsync(request.Email, userId);
                if (existingUserWithEmail)
                {
                    _logger.LogWarning("Update profile failed: Email '{Email}' is already in use by another user (excluding user ID '{UserId}').", request.Email, userId);
                    return false;
                }
                user.Email = request.Email;
            }

            try
            {
                await _userRepository.UpdateUserAsync(user);
                _logger.LogInformation("User profile for ID '{UserId}' updated successfully.", userId);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict occurred while updating profile for user ID '{UserId}'.", userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating profile for user ID '{UserId}'.", userId);
                return false;
            }
        }

        // Changes a user's password.
        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
        {
            _logger.LogInformation("Attempting to change password for user ID '{UserId}'.", userId);
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Change password failed: User with ID '{UserId}' not found.", userId);
                return false;
            }

            // Verifies the current password.
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Change password failed: Incorrect current password for user ID '{UserId}'.", userId);
                return false;
            }

            // Hashes and updates the new password.
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            try
            {
                await _userRepository.UpdateUserAsync(user);
                _logger.LogInformation("Password for user ID '{UserId}' changed successfully.", userId);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict occurred while changing password for user ID '{UserId}'.", userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while changing password for user ID '{UserId}'.", userId);
                return false;
            }
        }

        // Retrieves a user's current account status.
        public async Task<string?> GetUserStatusAsync(Guid userId)
        {
            _logger.LogInformation("Attempting to retrieve status for user ID '{UserId}'.", userId);
            var status = await _userRepository.GetUserStatusByIdAsync(userId);
            if (status == null)
            {
                _logger.LogWarning("User status not found for user ID '{UserId}'.", userId);
            }
            else
            {
                _logger.LogInformation("Retrieved status '{Status}' for user ID '{UserId}'.", status, userId);
            }
            return status;
        }

        // Gathers all dashboard data for a specific user.
        public async Task<UserDashboardDto> GetUserDashboardDataAsync(Guid userId)
        {
            _logger.LogInformation("Gathering dashboard data for user ID '{UserId}'.", userId);
            var now = DateTime.UtcNow;
            var endOfDayTomorrow = now.Date.AddDays(2).AddMilliseconds(-1);

            var completedStatuses = new[] { "Completed" };

            // Retrieves various task counts and statistics from the repository.
            var myTotalTasks = await _taskItemRepository.GetTotalTasksCountByUserIdAsync(userId);
            _logger.LogDebug("User ID '{UserId}': Total tasks = {TotalTasks}.", userId, myTotalTasks);

            var myTasksDueSoon = await _taskItemRepository.GetTasksDueSoonCountByUserIdAsync(userId, now, endOfDayTomorrow, completedStatuses);
            _logger.LogDebug("User ID '{UserId}': Tasks due soon = {TasksDueSoon}.", userId, myTasksDueSoon);

            var myOverdueTasks = await _taskItemRepository.GetOverdueTasksCountByUserIdAsync(userId, now, completedStatuses);
            _logger.LogDebug("User ID '{UserId}': Overdue tasks = {OverdueTasks}.", userId, myOverdueTasks);

            var myTaskStatusCounts = await _taskItemRepository.GetTaskStatusCountsByUserIdAsync(userId);
            _logger.LogDebug("User ID '{UserId}': Task status counts retrieved.", userId);

            var myProjectTaskCounts = await _taskItemRepository.GetProjectTaskCountsByUserIdWithProjectNameAsync(userId);
            _logger.LogDebug("User ID '{UserId}': Project task counts retrieved.", userId);


            // Returns a DTO containing all dashboard data.
            var dashboardDto = new UserDashboardDto
            {
                MyTotalTasks = myTotalTasks,
                MyTasksDueSoon = myTasksDueSoon,
                MyOverdueTasks = myOverdueTasks,
                MyTaskStatusCounts = myTaskStatusCounts,
                MyProjectTaskCounts = myProjectTaskCounts
            };

            _logger.LogInformation("Dashboard data for user ID '{UserId}' gathered successfully.", userId);
            return dashboardDto;
        }
    }
}