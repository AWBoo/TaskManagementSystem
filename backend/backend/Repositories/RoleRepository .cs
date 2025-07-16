using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace backend.Repositories
{
    // Repository for managing Role data.
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(ApplicationDBContext context, ILogger<RoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Retrieves all roles from the database.
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            _logger.LogInformation("Retrieving all roles from the database.");
            try
            {
                var roles = await _context.Roles.AsNoTracking().ToListAsync();
                _logger.LogInformation("Retrieved {RoleCount} roles.", roles.Count);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all roles.");
                throw;
            }
        }

        // Retrieves a role by its ID.
        public async Task<Role?> GetRoleByIdAsync(Guid roleId)
        {
            _logger.LogInformation("Retrieving role with ID '{RoleId}'.", roleId);
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
                if (role == null)
                {
                    _logger.LogWarning("Role with ID '{RoleId}' not found.", roleId);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved role '{RoleName}' (ID: '{RoleId}').", role.Name, roleId);
                }
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role with ID '{RoleId}'.", roleId);
                throw;
            }
        }

        // Retrieves a role by its name.
        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            _logger.LogInformation("Retrieving role with name '{RoleName}'.", roleName);
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                {
                    _logger.LogWarning("Role with name '{RoleName}' not found.", roleName);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved role '{RoleName}' (ID: '{RoleId}').", role.Name, role.Id);
                }
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role with name '{RoleName}'.", roleName);
                throw;
            }
        }

        // Checks if a role with the given ID exists.
        public async Task<bool> RoleExistsAsync(Guid roleId)
        {
            _logger.LogDebug("Checking if role with ID '{RoleId}' exists.", roleId);
            try
            {
                var exists = await _context.Roles.AnyAsync(r => r.Id == roleId);
                _logger.LogDebug("Role with ID '{RoleId}' exists: {Exists}.", roleId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of role with ID '{RoleId}'.", roleId);
                throw;
            }
        }

        // Checks if a role with the given name exists.
        public async Task<bool> RoleExistsByNameAsync(string roleName)
        {
            _logger.LogDebug("Checking if role with name '{RoleName}' exists.", roleName);
            try
            {
                var exists = await _context.Roles.AnyAsync(r => r.Name == roleName);
                _logger.LogDebug("Role with name '{RoleName}' exists: {Exists}.", roleName, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of role with name '{RoleName}'.", roleName);
                throw;
            }
        }

        // Adds a new role to the database.
        public async Task AddRoleAsync(Role role)
        {
            _logger.LogInformation("Adding new role '{RoleName}' (ID: '{RoleId}') to the database.", role.Name, role.Id);
            try
            {
                await _context.Roles.AddAsync(role);
                await SaveChangesAsync();
                _logger.LogInformation("Role '{RoleName}' (ID: '{RoleId}') successfully added.", role.Name, role.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding role '{RoleName}' (ID: '{RoleId}').", role.Name, role.Id);
                throw;
            }
        }

        // Updates an existing role in the database.
        public async Task UpdateRoleAsync(Role role)
        {
            _logger.LogInformation("Updating role '{RoleName}' (ID: '{RoleId}').", role.Name, role.Id);
            try
            {
                _context.Roles.Update(role);
                await SaveChangesAsync();
                _logger.LogInformation("Role '{RoleName}' (ID: '{RoleId}') successfully updated.", role.Name, role.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role '{RoleName}' (ID: '{RoleId}').", role.Name, role.Id);
                throw;
            }
        }

        // Deletes a role from the database.
        public async Task DeleteRoleAsync(Role role)
        {
            _logger.LogInformation("Deleting role '{RoleName}' (ID: '{RoleId}') from the database.", role.Name, role.Id);
            try
            {
                _context.Roles.Remove(role);
                await SaveChangesAsync();
                _logger.LogInformation("Role '{RoleName}' (ID: '{RoleId}') successfully deleted.", role.Name, role.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role '{RoleName}' (ID: '{RoleId}').", role.Name, role.Id);
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