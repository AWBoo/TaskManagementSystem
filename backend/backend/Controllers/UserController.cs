using backend.DTOS.User;
using backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace backend.Controllers
{
    // API controller for authenticated user operations.
    // Handles user profile management and dashboard data.
    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger; 

        // Initializes a new instance of the UserController.
        public UserController(IUserService userService, ILogger<UserController> logger) 
        {
            _userService = userService;
            _logger = logger; 
        }

        // Helper to retrieve the current authenticated user's ID.
        // Relies on the 'NameIdentifier' claim from the user's principal.
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                // This indicates a critical issue with authentication setup if [Authorize] allows this.
                _logger.LogCritical("UserController: GetUserId failed - User authenticated but NameIdentifier claim is missing or empty.");
                throw new UnauthorizedAccessException("User ID claim not found. User not authenticated or claim is missing.");
            }
            return Guid.Parse(userIdClaim);
        }

        // GET /api/user/profile
        // Retrieves the profile details for the authenticated user.
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = GetUserId();
            _logger.LogInformation("UserController: Received request to get profile for user ID '{UserId}'.", userId);

            var userProfile = await _userService.GetUserProfileAsync(userId);

            if (userProfile == null)
            {
                _logger.LogWarning("UserController: User profile not found for user ID '{UserId}'.", userId);
                return NotFound("User profile not found.");
            }

            _logger.LogInformation("UserController: Successfully retrieved profile for user ID '{UserId}'.", userId);
            return Ok(userProfile);
        }

        // PUT /api/user/profile
        // Allows the authenticated user to update their profile details.
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileRequestDto request)
        {
            var userId = GetUserId();
            _logger.LogInformation("UserController: Received request to update profile for user ID '{UserId}'.", userId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UserController: UpdateUserProfile failed for user ID '{UserId}' due to invalid model state. Errors: {ModelStateErrors}", userId, ModelState);
                return BadRequest(ModelState);
            }

            var result = await _userService.UpdateUserProfileAsync(userId, request);

            if (!result)
            {
                // This indicates a business logic error (e.g., email already in use).
                _logger.LogWarning("UserController: UpdateUserProfile failed for user ID '{UserId}'. Email '{Email}' might be in use or another issue occurred.", userId, request.Email);
                return BadRequest("Failed to update profile. Email might be in use or other issue.");
            }

            _logger.LogInformation("UserController: Profile updated successfully for user ID '{UserId}'.", userId);
            return Ok(new { message = "Profile updated successfully." });
        }

        // POST /api/user/change-password
        // Allows the authenticated user to change their password.
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var userId = GetUserId();
            _logger.LogInformation("UserController: Received request to change password for user ID '{UserId}'.", userId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UserController: ChangePassword failed for user ID '{UserId}' due to invalid model state. Errors: {ModelStateErrors}", userId, ModelState);
                return BadRequest(ModelState);
            }

            var result = await _userService.ChangePasswordAsync(userId, request);

            if (!result)
            {
                // This indicates an incorrect current password or other service-side failure.
                _logger.LogWarning("UserController: ChangePassword failed for user ID '{UserId}'. Incorrect current password or other failure.", userId);
                return BadRequest("Failed to change password. Please check your current password.");
            }

            _logger.LogInformation("UserController: Password changed successfully for user ID '{UserId}'.", userId);
            return Ok(new { message = "Password changed successfully." });
        }

        // GET /api/user/dashboard-summary
        // Retrieves a summary of tasks and project data for the user's dashboard.
        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetUserDashboardSummary()
        {
            var userId = GetUserId(); // GetUserId might throw UnauthorizedAccessException
            _logger.LogInformation("UserController: Received request to get dashboard summary for user ID '{UserId}'.", userId);

            try
            {
                var dashboardData = await _userService.GetUserDashboardDataAsync(userId);

                if (dashboardData == null)
                {
                    _logger.LogWarning("UserController: Dashboard data not found for user ID '{UserId}'.", userId);
                    return NotFound("Dashboard data could not be retrieved.");
                }

                _logger.LogInformation("UserController: Successfully retrieved dashboard data for user ID '{UserId}'.", userId);
                return Ok(dashboardData);
            }
            catch (UnauthorizedAccessException ex)
            {
                // This specific catch block for UnauthorizedAccessException from GetUserId is good.
                _logger.LogError(ex, "UserController: Unauthorized access attempt to get dashboard summary for user ID '{UserId}'.", userId);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Catch-all for any other unexpected errors in this method.
                _logger.LogError(ex, "UserController: An unexpected error occurred while fetching dashboard data for user ID '{UserId}'.", userId);
                return StatusCode(500, new { message = "An error occurred while fetching dashboard data.", error = ex.Message });
            }
        }
    }
}