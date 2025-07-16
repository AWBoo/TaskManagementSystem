using backend.DTOS.Auth;
using backend.DTOS.User;
using backend.Interfaces;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace backend.Services
{
    // Service for user authentication and registration.
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IJwtService _jwt;
        private readonly ILogger<AuthService> _logger;

        // Injects repository dependencies, JwtService, and ILogger.
        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IJwtService jwt,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _jwt = jwt;
            _logger = logger;
        }

        // Registers a new user and assigns a default role.
        public async Task<AuthResponseDto?> RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation("AuthService: Attempting to register new user with email: {Email}", request.Email);

            // Checks if the email is already in use.
            if (await _userRepository.UserExistsByEmailAsync(request.Email))
            {
                _logger.LogWarning("AuthService: Registration failed - Email '{Email}' is already in use.", request.Email);
                return new AuthResponseDto { Message = "Email already in use" };
            }

            // Creates a new user with a hashed password.
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Status = "Active",
            };

            try
            {
                // Adds the user to the database.
                await _userRepository.AddUserAsync(user);
                await _userRepository.SaveChangesAsync();
                _logger.LogInformation("AuthService: User '{Email}' added to database.", user.Email);

                // Assigns the default "User" role.
                var userRoleModel = await _roleRepository.GetRoleByNameAsync("User");
                if (userRoleModel != null)
                {
                    var newUserRole = new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        RoleId = userRoleModel.Id,
                        AssignedAt = DateTime.UtcNow,
                        User = user,
                        Role = userRoleModel
                    };
                    await _userRoleRepository.AddUserRoleAsync(newUserRole);
                    await _userRoleRepository.SaveChangesAsync();
                    _logger.LogInformation("AuthService: Default 'User' role assigned to '{Email}'.", user.Email);
                }
                else
                {
                    _logger.LogError("AuthService: Default 'User' role not found in the database. Role assignment skipped for new user '{Email}'.", user.Email);
                }

                // Retrieves user with roles for token generation.
                var userWithRoles = await _userRepository.GetUserByIdWithRolesAsync(user.Id);

                if (userWithRoles == null)
                {
                    _logger.LogError("AuthService: After registration, failed to retrieve user '{Email}' with roles for token generation. Data inconsistency.", user.Email);
                    return new AuthResponseDto { Message = "An unexpected error occurred after registration." };
                }

                // Generates JWT token for the new user.
                var token = await _jwt.GenerateToken(userWithRoles);

                // Maps user data to DTO for response.
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    // Ensure roles are correctly populated, fallback to "User" if none found (should not happen if role assignment worked)
                    Roles = userWithRoles?.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string> { "User" },
                };

                _logger.LogInformation("AuthService: User '{Email}' registered successfully and token issued.", user.Email);
                // Returns success response.
                return new AuthResponseDto
                {
                    User = userDto,
                    Token = token,
                    Message = "User created successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthService: An error occurred during user registration for email '{Email}'.", request.Email);
                return new AuthResponseDto { Message = "An unexpected error occurred during registration." };
            }
        }

        // Authenticates a user based on provided credentials.
        public async Task<AuthResponseDto?> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("AuthService: Login attempt for email: {Email}", request.Email);

            // Finds the user by email.
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            // Handles user not found.
            if (user == null)
            {
                _logger.LogWarning("AuthService: Login failed - User with email '{Email}' not found.", request.Email);
                return new AuthResponseDto { Message = "Invalid credentials" };
            }

            // Verifies password.
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("AuthService: Login failed - Incorrect password for user '{Email}'.", request.Email);
                return new AuthResponseDto { Message = "Invalid credentials" };
            }

            // Checks if the account is deactivated.
            if (user.Status == "Deactivated")
            {
                _logger.LogWarning("AuthService: Login BLOCKED - User '{Email}' is DEACTIVATED.", user.Email);
                return new AuthResponseDto { Message = "Your account has been deactivated. Please contact support." };
            }

            // Checks if the account is suspended.
            if (user.Status == "Suspended")
            {
                _logger.LogWarning("AuthService: Login BLOCKED - User '{Email}' is SUSPENDED.", user.Email);
                return new AuthResponseDto { Message = "Your account has been suspended. Please contact support for more information." };
            }

            // Retrieves user with roles for token generation.
            var userWithRoles = await _userRepository.GetUserByIdWithRolesAsync(user.Id);

            // Handles data inconsistency if roles cannot be retrieved.
            if (userWithRoles == null)
            {
                _logger.LogError("AuthService: Login failed - User '{Email}' found initially but could not retrieve with roles. Data inconsistency, user might be corrupted.", user.Email);
                return new AuthResponseDto { Message = "An unexpected error occurred during login." };
            }

            // Generates JWT token.
            var token = await _jwt.GenerateToken(userWithRoles);

            // Maps user data to DTO for response.
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Roles = userWithRoles.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string> { "User" },
            };

            _logger.LogInformation("AuthService: Login SUCCESS - User '{Email}' logged in and token issued.", user.Email);
            return new AuthResponseDto
            {
                User = userDto,
                Token = token,
                Message = "Logged in successfully."
            };
        }
    }
}