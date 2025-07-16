using backend.DTOS.Auth;
using backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Add this using directive
using System; // For Task

namespace backend.Controllers
{
    // API controller for user authentication.
    // Handles registration and login.
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger; 

        // Initializes a new instance of the AuthController.
        public AuthController(IAuthService authService, ILogger<AuthController> logger) 
        {
            _authService = authService;
            _logger = logger; // Assign the injected logger
        }

        // POST /api/auth/register
        // Registers a new user account.
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            _logger.LogInformation("AuthController: Received registration request for email '{Email}'.", request.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("AuthController: Registration failed for email '{Email}' due to invalid model state. Errors: {ModelStateErrors}", request.Email, ModelState);
                return BadRequest(ModelState);
            }

            var response = await _authService.RegisterAsync(request);

            if (response?.Message == "Email already in use")
            {
                _logger.LogWarning("AuthController: Registration failed for email '{Email}' - Email already in use.", request.Email);
                return BadRequest(new { message = response.Message });
            }
            else if (response == null)
            {
                // Handles general errors during registration.
                // This indicates an unexpected problem in the service layer.
                _logger.LogError("AuthController: An unexpected error occurred during registration for email '{Email}'. Service returned null.", request.Email);
                return StatusCode(500, new { message = "An error occurred during registration." });
            }

            return Ok(response);
        }

        // POST /api/auth/login
        // Authenticates a user and returns a JWT token.
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            _logger.LogInformation("AuthController: Received login request for email '{Email}'.", request.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("AuthController: Login failed for email '{Email}' due to invalid model state. Errors: {ModelStateErrors}", request.Email, ModelState);
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(request);

            if (response?.Message == "Invalid credentials")
            {
                _logger.LogWarning("AuthController: Login failed for email '{Email}' - Invalid credentials.", request.Email);
                return Unauthorized(new { message = response.Message });
            }
            else if (response == null)
            {
                // This indicates an unexpected problem in the service layer.
                _logger.LogError("AuthController: An unexpected error occurred during login for email '{Email}'. Service returned null.", request.Email);
                return StatusCode(500, new { message = "An error occurred during login." });
            }

            return Ok(response);
        }
    }
}