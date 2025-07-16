using backend.DTOS.User;
using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


// Defines the Admin API controller.
// This controller handles administrative operations and requires an 'Admin' role.
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger; 

    // Initializes a new instance of the AdminController.
    public AdminController(IAdminService adminService, ILogger<AdminController> logger) 
    {
        _adminService = adminService;
        _logger = logger; 
    }

    // GET /api/admin/users
    // Retrieves a list of all users for admin management.
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("AdminController: Attempting to retrieve all users.");
        var users = await _adminService.GetAllUsersAsync();
        _logger.LogInformation("AdminController: Successfully retrieved {UserCount} users.", users.Count());
        return Ok(users);
    }

    // GET /api/admin/dashboard/stats
    // Fetches overall system dashboard statistics.
    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        _logger.LogInformation("AdminController: Fetching overall dashboard statistics.");
        var stats = await _adminService.GetDashboardStatsAsync();
        _logger.LogInformation("AdminController: Overall dashboard statistics retrieved.");
        return Ok(stats);
    }

    // GET /api/admin/dashboard/tasks-by-status
    // Retrieves task counts grouped by their status.
    [HttpGet("dashboard/tasks-by-status")]
    public async Task<IActionResult> GetTasksByStatus()
    {
        _logger.LogInformation("AdminController: Retrieving task counts by status for dashboard.");
        var statusCounts = await _adminService.GetTasksByStatusAsync();
        _logger.LogInformation("AdminController: Task counts by status retrieved.");
        return Ok(statusCounts);
    }

    // GET /api/admin/dashboard/users-task-counts
    // Provides task counts per user for dashboard display.
    [HttpGet("dashboard/users-task-counts")]
    public async Task<IActionResult> GetUsersTaskCounts()
    {
        _logger.LogInformation("AdminController: Retrieving task counts per user for dashboard.");
        var userTaskCounts = await _adminService.GetUsersTaskCountsAsync();
        _logger.LogInformation("AdminController: User task counts retrieved.");
        return Ok(userTaskCounts);
    }

    // GET /api/admin/dashboard/projects-task-counts
    // Retrieves task counts associated with each project.
    [HttpGet("dashboard/projects-task-counts")]
    public async Task<IActionResult> GetProjectsTaskCounts()
    {
        _logger.LogInformation("AdminController: Retrieving task counts per project for dashboard.");
        var projectTaskCounts = await _adminService.GetProjectsTaskCountsAsync();
        _logger.LogInformation("AdminController: Project task counts retrieved.");
        return Ok(projectTaskCounts);
    }

    // POST /api/admin/users/{userId}/roles/assign
    // Assigns a specified role to a user.
    [HttpPost("users/{userId}/roles/assign")]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, [FromBody] AssignRoleRequest request)
    {
        _logger.LogInformation("AdminController: Received request to assign role '{RoleName}' to user ID '{UserId}'.", request.RoleName, userId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("AdminController: AssignRoleToUser failed due to invalid model state for user ID '{UserId}'. Errors: {ModelStateErrors}", userId, ModelState);
            return BadRequest(ModelState);
        }

        var result = await _adminService.AssignRoleToUserAsync(userId, request.RoleName);
        if (!result)
        {
            // Differentiates HTTP response based on reason for assignment failure.
            // Note: Calling GetAllUsersAsync and GetAllRolesAsync here
            // could be inefficient. In a real-world scenario, your service
            // layer should return a more specific error code/enum/result object
            // to avoid redundant database calls in the controller.
            var userExists = await _adminService.GetUserByIdAsync(userId); // Better to use GetUserById
            if (userExists == null)
            {
                _logger.LogWarning("AdminController: AssignRoleToUser failed - User ID '{UserId}' not found.", userId);
                return NotFound("User not found.");
            }

            var roles = await _adminService.GetAllRolesAsync();
            if (!roles.Any(r => r.Name == request.RoleName))
            {
                _logger.LogWarning("AdminController: AssignRoleToUser failed - Role '{RoleName}' not found.", request.RoleName);
                return NotFound("Role not found.");
            }

            _logger.LogWarning("AdminController: AssignRoleToUser failed - User ID '{UserId}' already has role '{RoleName}'.", userId, request.RoleName);
            return BadRequest("User already has this role.");
        }

        _logger.LogInformation("AdminController: Role '{RoleName}' assigned successfully to user ID '{UserId}'.", request.RoleName, userId);
        return Ok(new { message = $"Role '{request.RoleName}' assigned successfully." });
    }

    // POST /api/admin/users/{userId}/roles/remove
    // Removes a specified role from a user.
    [HttpPost("users/{userId}/roles/remove")]
    public async Task<IActionResult> RemoveRoleFromUser(Guid userId, [FromBody] AssignRoleRequest request)
    {
        _logger.LogInformation("AdminController: Received request to remove role '{RoleName}' from user ID '{UserId}'.", request.RoleName, userId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("AdminController: RemoveRoleFromUser failed due to invalid model state for user ID '{UserId}'. Errors: {ModelStateErrors}", userId, ModelState);
            return BadRequest(ModelState);
        }

        var result = await _adminService.RemoveRoleFromUserAsync(userId, request.RoleName);
        if (!result)
        {
            // Differentiates HTTP response based on reason for removal failure.
            var userExists = await _adminService.GetUserByIdAsync(userId); // Better to use GetUserById
            if (userExists == null)
            {
                _logger.LogWarning("AdminController: RemoveRoleFromUser failed - User ID '{UserId}' not found.", userId);
                return NotFound("User not found.");
            }

            var roles = await _adminService.GetAllRolesAsync();
            if (!roles.Any(r => r.Name == request.RoleName))
            {
                _logger.LogWarning("AdminController: RemoveRoleFromUser failed - Role '{RoleName}' not found.", request.RoleName);
                return NotFound("Role not found.");
            }

            _logger.LogWarning("AdminController: RemoveRoleFromUser failed - User ID '{UserId}' does not have role '{RoleName}'.", userId, request.RoleName);
            return BadRequest("User does not have this role.");
        }

        _logger.LogInformation("AdminController: Role '{RoleName}' removed successfully from user ID '{UserId}'.", request.RoleName, userId);
        return Ok(new { message = $"Role '{request.RoleName}' removed successfully." });
    }

    // GET /api/admin/roles
    // Retrieves all available user roles in the system.
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        _logger.LogInformation("AdminController: Attempting to retrieve all roles.");
        var roles = await _adminService.GetAllRolesAsync();
        _logger.LogInformation("AdminController: Successfully retrieved {RoleCount} roles.", roles.Count());
        return Ok(roles);
    }

    // PUT /api/admin/users/{userId}/status
    // Activates or deactivates a user's account.
    [HttpPut("users/{userId}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusRequest request)
    {
        _logger.LogInformation("AdminController: Received request to update status for user ID '{UserId}' to '{NewStatus}'.", userId, request.NewStatus);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("AdminController: UpdateUserStatus failed due to invalid model state for user ID '{UserId}'. Errors: {ModelStateErrors}", userId, ModelState);
            return BadRequest(ModelState);
        }

        var result = await _adminService.UpdateUserStatusAsync(userId, request.NewStatus);
        if (!result)
        {
            _logger.LogWarning("AdminController: UpdateUserStatus failed - User ID '{UserId}' not found.", userId);
            return NotFound("User not found.");
        }
        _logger.LogInformation("AdminController: User ID '{UserId}' status updated to '{NewStatus}' successfully.", userId, request.NewStatus);
        return Ok(new { message = $"User status updated to '{request.NewStatus}'." });
    }

    // DELETE /api/admin/users/{userId}
    // Deletes a user account from the system.
    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        _logger.LogInformation("AdminController: Received request to delete user ID '{UserId}'.", userId);
        var result = await _adminService.DeleteUserAsync(userId);
        if (!result)
        {
            _logger.LogWarning("AdminController: DeleteUser failed - User ID '{UserId}' not found.", userId);
            return NotFound("User not found.");
        }
        _logger.LogInformation("AdminController: User ID '{UserId}' deleted successfully.", userId);
        return NoContent(); // 204 No Content typically for successful deletion
    }

    // GET /api/admin/users/{userId}
    // Retrieves a specific user's profile details by ID.
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        _logger.LogInformation("AdminController: Received request to retrieve user details for ID '{UserId}'.", userId);
        var user = await _adminService.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("AdminController: GetUserById failed - User ID '{UserId}' not found.", userId);
            return NotFound("User not found.");
        }
        _logger.LogInformation("AdminController: Successfully retrieved user details for ID '{UserId}'.", userId);
        return Ok(user);
    }

    // PUT /api/admin/users/{userId}/profile
    // Updates a specific user's profile details by admin.
    [HttpPut("users/{userId}/profile")]
    public async Task<IActionResult> AdminUpdateUserProfile(Guid userId, [FromBody] UpdateProfileRequestDto request)
    {
        _logger.LogInformation("AdminController: Received request to update profile for user ID '{UserId}'.", userId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("AdminController: AdminUpdateUserProfile failed due to invalid model state for user ID '{UserId}'. Errors: {ModelStateErrors}", userId, ModelState);
            return BadRequest(ModelState);
        }

        var result = await _adminService.AdminUpdateUserProfileAsync(userId, request);

        if (!result)
        {
            // Similar to role assignment, better error handling from service could streamline this.
            var userExists = await _adminService.GetUserByIdAsync(userId);
            if (userExists == null)
            {
                _logger.LogWarning("AdminController: AdminUpdateUserProfile failed - User ID '{UserId}' not found.", userId);
                return NotFound("User not found.");
            }
            // This implies the email might be in use, or another business rule was violated
            _logger.LogWarning("AdminController: AdminUpdateUserProfile failed for user ID '{UserId}'. New email '{NewEmail}' might already be in use or another business rule violated.", userId, request.Email);
            return BadRequest("Failed to update user profile. New email might already be in use.");
        }

        _logger.LogInformation("AdminController: User profile for ID '{UserId}' updated successfully.", userId);
        return Ok(new { message = $"User profile for ID {userId} updated successfully." });
    }

    // POST /api/admin/users/{userId}/change-password
    // Changes a user's password as an administrator.
    [HttpPost("users/{userId}/change-password")]
    public async Task<IActionResult> AdminChangeUserPassword(Guid userId, [FromBody] AdminChangePasswordRequestDto request)
    {
        _logger.LogInformation("AdminController: Received request to change password for user ID '{UserId}'.", userId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("AdminController: AdminChangeUserPassword failed due to invalid model state for user ID '{UserId}'. Errors: {ModelStateErrors}", userId, ModelState);
            return BadRequest(ModelState);
        }

        // The 'request' is already the DTO, no need to map to another new DTO here.
        var result = await _adminService.AdminChangeUserPasswordAsync(userId, request);

        if (!result)
        {
            _logger.LogWarning("AdminController: AdminChangeUserPassword failed - User ID '{UserId}' not found or password change failed (e.g., new passwords don't match, or internal error).", userId);
            return NotFound("User not found or password change failed."); // This could also be BadRequest if passwords don't match etc.
        }

        _logger.LogInformation("AdminController: Password for user ID '{UserId}' changed successfully.", userId);
        return Ok(new { message = $"Password for user ID {userId} changed successfully." });
    }

    // GET /api/admin/users/{id}/history
    // Retrieves the audit history for a specific user.
    [HttpGet("users/{id}/history")]
    public async Task<ActionResult<IEnumerable<AuditEntry>>> GetUserHistory(Guid id)
    {
        _logger.LogInformation("AdminController: Received request to retrieve audit history for user ID '{UserId}'.", id);
        var history = await _adminService.GetUserAuditHistoryAsync(id);
        _logger.LogInformation("AdminController: Audit history for user ID '{UserId}' retrieved, found {EntryCount} entries.", id, history.Count());
        return Ok(history);
    }

    // GET /api/admin/audit/latest
    // Retrieves a specified count of the latest global audit entries.
    [HttpGet("audit/latest")]
    public async Task<ActionResult<IEnumerable<AuditEntry>>> GetLatestAuditEntries([FromQuery] int count = 10)
    {
        _logger.LogInformation("AdminController: Received request to retrieve latest {Count} global audit entries.", count);

        if (count <= 0)
        {
            _logger.LogWarning("AdminController: GetLatestAuditEntries failed - invalid count '{Count}' provided.", count);
            return BadRequest("Count must be a positive number.");
        }

        var latestEntries = await _adminService.GetLatestAuditEntriesAsync(count);

        _logger.LogInformation("AdminController: Retrieved {EntryCount} latest global audit entries.", latestEntries.Count());
        return Ok(latestEntries);
    }
}