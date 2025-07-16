using backend.Models;

namespace backend.Repositories.Interfaces
{
    // Defines operations for accessing and managing UserRole entities.
    public interface IUserRoleRepository
    {
        // Retrieves a specific user-role association.
        Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId);
        // Retrieves all role associations for a given user.
        Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(Guid userId);
        // Checks if a specific user-role association exists.
        Task<bool> UserRoleExistsAsync(Guid userId, Guid roleId);
        // Adds a new user-role association to the repository.
        Task AddUserRoleAsync(UserRole userRole);
        // Deletes a user-role association from the repository.
        Task DeleteUserRoleAsync(UserRole userRole);
        // Saves all changes made in the current context.
        Task SaveChangesAsync();
    }
}