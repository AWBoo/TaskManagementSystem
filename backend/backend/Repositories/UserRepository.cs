using backend.Data;
using backend.DTOS.User;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace backend.Repositories
{
    // Repository for managing User data.
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<UserRepository> _logger; 

        // Initializes the repository with the application database context.
        public UserRepository(ApplicationDBContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger; 
        }

        // Retrieves all users, including their roles and assigned tasks.
        public async Task<IEnumerable<User>> GetAllUsersWithRolesAndTasksAsync()
        {
            _logger.LogInformation("Retrieving all users with roles and tasks.");
            try
            {
                var users = await _context.Users
                                         .Include(u => u.UserRoles) 
                                         .ThenInclude(ur => ur.Role) 
                                         .Include(u => u.Tasks) 
                                         .AsNoTracking() 
                                         .ToListAsync();
                _logger.LogInformation("Retrieved {UserCount} users with roles and tasks.", users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users with roles and tasks.");
                throw;
            }
        }

        // Retrieves a user by their ID, including their roles and assigned tasks.
        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            _logger.LogInformation("Retrieving user with ID '{UserId}', including roles and tasks.", userId);
            try
            {
                var user = await _context.Users
                                         .Include(u => u.UserRoles)
                                             .ThenInclude(ur => ur.Role)
                                         .Include(u => u.Tasks)
                                         .FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID '{UserId}' not found (with roles and tasks).", userId);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved user '{UserName}' (ID: '{UserId}') with {RoleCount} roles and {TaskCount} tasks.",
                        user.Name, userId, user.UserRoles?.Count ?? 0, user.Tasks?.Count ?? 0);
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID '{UserId}'.", userId);
                throw;
            }
        }

        // Retrieves a user by their email address, including their roles and assigned tasks.
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            _logger.LogInformation("Retrieving user with email '{Email}', including roles and tasks.", email);
            try
            {
                var user = await _context.Users
                                         .Include(u => u.UserRoles)
                                             .ThenInclude(ur => ur.Role)
                                         .Include(u => u.Tasks)
                                         .FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    _logger.LogWarning("User with email '{Email}' not found.", email);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved user '{UserName}' (Email: '{Email}') with {RoleCount} roles and {TaskCount} tasks.",
                        user.Name, email, user.UserRoles?.Count ?? 0, user.Tasks?.Count ?? 0);
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with email '{Email}'.", email);
                throw;
            }
        }

        // Checks if a user with the given ID exists.
        public async Task<bool> UserExistsAsync(Guid userId)
        {
            _logger.LogDebug("Checking if user with ID '{UserId}' exists.", userId);
            try
            {
                var exists = await _context.Users.AnyAsync(u => u.Id == userId);
                _logger.LogDebug("User with ID '{UserId}' exists: {Exists}.", userId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of user with ID '{UserId}'.", userId);
                throw;
            }
        }

        // Checks if a user with the given email exists.
        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            _logger.LogDebug("Checking if user with email '{Email}' exists.", email);
            try
            {
                var exists = await _context.Users.AnyAsync(u => u.Email == email);
                _logger.LogDebug("User with email '{Email}' exists: {Exists}.", email, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of user with email '{Email}'.", email);
                throw;
            }
        }

        // Adds a new user to the database.
        public async Task AddUserAsync(User user)
        {
            _logger.LogInformation("Adding new user '{UserName}' (ID: '{UserId}', Email: '{Email}') to the database.", user.Name, user.Id, user.Email);
            try
            {
                await _context.Users.AddAsync(user);
                await SaveChangesAsync(); // Save changes immediately after adding
                _logger.LogInformation("User '{UserName}' (ID: '{UserId}') successfully added.", user.Name, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user '{UserName}' (ID: '{UserId}').", user.Name, user.Id);
                throw;
            }
        }

        // Updates an existing user in the database.
        public async Task UpdateUserAsync(User user)
        {
            _logger.LogInformation("Updating user '{UserName}' (ID: '{UserId}', Email: '{Email}').", user.Name, user.Id, user.Email);
            try
            {
                _context.Users.Update(user);
                await SaveChangesAsync(); // Save changes immediately after updating
                _logger.LogInformation("User '{UserName}' (ID: '{UserId}') successfully updated.", user.Name, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user '{UserName}' (ID: '{UserId}').", user.Name, user.Id);
                throw;
            }
        }

        // Deletes a user from the database.
        public async Task DeleteUserAsync(User user)
        {
            _logger.LogInformation("Deleting user '{UserName}' (ID: '{UserId}') from the database.", user.Name, user.Id);
            try
            {
                _context.Users.Remove(user);
                await SaveChangesAsync(); // Save changes immediately after deleting
                _logger.LogInformation("User '{UserName}' (ID: '{UserId}') successfully deleted.", user.Name, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user '{UserName}' (ID: '{UserId}').", user.Name, user.Id);
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

        // Retrieves a user by ID, including their roles.
        public async Task<User?> GetUserByIdWithRolesAsync(Guid id)
        {
            _logger.LogInformation("Retrieving user with ID '{UserId}', including roles.", id);
            try
            {
                var user = await _context.Users
                                         .Include(u => u.UserRoles)
                                             .ThenInclude(ur => ur.Role)
                                         .FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID '{UserId}' not found (with roles).", id);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved user '{UserName}' (ID: '{UserId}') with {RoleCount} roles.", user.Name, id, user.UserRoles?.Count ?? 0);
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID '{UserId}' with roles.", id);
                throw;
            }
        }

        // Methods specifically tailored for the Task Service.
        // Checks if a user with the given ID exists (for task assignment validation).
        public async Task<bool> UserExistsByIdAsync(Guid id)
        {
            _logger.LogDebug("Checking if user with ID '{UserId}' exists for task assignment validation.", id);
            try
            {
                var exists = await _context.Users.AnyAsync(u => u.Id == id);
                _logger.LogDebug("User with ID '{UserId}' for task assignment exists: {Exists}.", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of user with ID '{UserId}' for task assignment validation.", id);
                throw;
            }
        }

        // Methods specifically tailored for the User Service.
        // Retrieves a UserDto by user ID.
        public async Task<UserDto?> GetUserDtoByIdAsync(Guid userId)
        {
            _logger.LogInformation("Retrieving UserDto for user ID '{UserId}'.", userId);
            try
            {
                var userDto = await _context.Users
                                         .Where(u => u.Id == userId)
                                         .Select(u => new UserDto
                                         {
                                             Id = u.Id,
                                             Email = u.Email,
                                         })
                                         .FirstOrDefaultAsync();
                if (userDto == null)
                {
                    _logger.LogWarning("UserDto not found for user ID '{UserId}'.", userId);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved UserDto for user ID '{UserId}'.", userId);
                }
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving UserDto for user ID '{UserId}'.", userId);
                throw;
            }
        }

        // Checks if a user with the given email exists, excluding a specific user ID.
        public async Task<bool> UserExistsByEmailExcludingIdAsync(string email, Guid userId)
        {
            _logger.LogDebug("Checking if email '{Email}' exists for another user, excluding user ID '{UserId}'.", email, userId);
            try
            {
                var exists = await _context.Users.AnyAsync(u => u.Email == email && u.Id != userId);
                _logger.LogDebug("Email '{Email}' exists for another user (excluding '{UserId}'): {Exists}.", email, userId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email '{Email}' exists for another user, excluding user ID '{UserId}'.", email, userId);
                throw;
            }
        }

        // Retrieves the status of a user by their ID.
        public async Task<string?> GetUserStatusByIdAsync(Guid userId)
        {
            _logger.LogInformation("Retrieving status for user ID '{UserId}'.", userId);
            try
            {
                var status = await _context.Users
                                         .Where(u => u.Id == userId)
                                         .Select(u => u.Status)
                                         .FirstOrDefaultAsync();
                if (status == null)
                {
                    _logger.LogWarning("Status not found for user ID '{UserId}'.", userId);
                }
                else
                {
                    _logger.LogInformation("Retrieved status '{Status}' for user ID '{UserId}'.", status, userId);
                }
                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving status for user ID '{UserId}'.", userId);
                throw;
            }
        }
    }
}