using backend.Models;

namespace backend.Repositories.Interfaces
{
    // Defines operations for accessing and managing Role entities.
    public interface IRoleRepository
    {
        // Retrieves all roles.
        Task<IEnumerable<Role>> GetAllRolesAsync();
        // Retrieves a role by its ID.
        Task<Role?> GetRoleByIdAsync(Guid roleId);
        // Retrieves a role by its name.
        Task<Role?> GetRoleByNameAsync(string roleName);
        // Checks if a role with the given ID exists.
        Task<bool> RoleExistsAsync(Guid roleId);
        // Checks if a role with the given name exists.
        Task<bool> RoleExistsByNameAsync(string roleName);
        // Adds a new role to the repository.
        Task AddRoleAsync(Role role);
        // Updates an existing role in the repository.
        Task UpdateRoleAsync(Role role);
        // Deletes a role from the repository.
        Task DeleteRoleAsync(Role role);
        // Saves all changes made in the current context.
        Task SaveChangesAsync();
    }
}