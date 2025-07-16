using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace backend.Repositories
{
    // Repository for managing UserRole data.
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<UserRoleRepository> _logger;

        // Initializes the repository with the application database context.
        public UserRoleRepository(ApplicationDBContext context, ILogger<UserRoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Retrieves a specific user-role association.
        public async Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId)
        {
            _logger.LogInformation("Retrieving user-role association for UserId '{UserId}' and RoleId '{RoleId}'.", userId, roleId);
            try
            {
                var userRole = await _context.UserRoles
                                             .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
                if (userRole == null)
                {
                    _logger.LogWarning("User-role association not found for UserId '{UserId}' and RoleId '{RoleId}'.", userId, roleId);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved user-role association for UserId '{UserId}' and RoleId '{RoleId}'.", userId, roleId);
                }
                return userRole;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user-role association for UserId '{UserId}' and RoleId '{RoleId}'.", userId, roleId);
                throw;
            }
        }

        // Retrieves all role associations for a given user.
        public async Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(Guid userId)
        {
            _logger.LogInformation("Retrieving all user-role associations for UserId '{UserId}'.", userId);
            try
            {
                var userRoles = await _context.UserRoles
                                              .Where(ur => ur.UserId == userId)
                                              .AsNoTracking()
                                              .ToListAsync();
                _logger.LogInformation("Retrieved {Count} user-role associations for UserId '{UserId}'.", userRoles.Count, userId);
                return userRoles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user-role associations for UserId '{UserId}'.", userId);
                throw;
            }
        }

        // Checks if a specific user-role association exists.
        public async Task<bool> UserRoleExistsAsync(Guid userId, Guid roleId)
        {
            _logger.LogDebug("Checking if user-role association exists for UserId '{UserId}' and RoleId '{RoleId}'.", userId, roleId);
            try
            {
                var exists = await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
                _logger.LogDebug("User-role association for UserId '{UserId}' and RoleId '{RoleId}' exists: {Exists}.", userId, roleId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of user-role association for UserId '{UserId}' and RoleId '{RoleId}'.", userId, roleId);
                throw;
            }
        }

        // Adds a new user-role association to the database.
        public async Task AddUserRoleAsync(UserRole userRole)
        {
            _logger.LogInformation("Adding new user-role association: UserId '{UserId}', RoleId '{RoleId}'.", userRole.UserId, userRole.RoleId);
            try
            {
                await _context.UserRoles.AddAsync(userRole);
                await SaveChangesAsync();
                _logger.LogInformation("User-role association successfully added: UserId '{UserId}', RoleId '{RoleId}'.", userRole.UserId, userRole.RoleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user-role association: UserId '{UserId}', RoleId '{RoleId}'.", userRole.UserId, userRole.RoleId);
                throw;
            }
        }

        // Deletes a user-role association from the database.
        public async Task DeleteUserRoleAsync(UserRole userRole)
        {
            _logger.LogInformation("Deleting user-role association: UserId '{UserId}', RoleId '{RoleId}'.", userRole.UserId, userRole.RoleId);
            try
            {
                _context.UserRoles.Remove(userRole);
                await SaveChangesAsync();
                _logger.LogInformation("User-role association successfully deleted: UserId '{UserId}', RoleId '{RoleId}'.", userRole.UserId, userRole.RoleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user-role association: UserId '{UserId}', RoleId '{RoleId}'.", userRole.UserId, userRole.RoleId);
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
    }
}