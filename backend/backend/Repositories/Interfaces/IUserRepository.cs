using backend.DTOS.User;
using backend.Models;
using System; // Added for Guid
using System.Collections.Generic; // Added for IEnumerable
using System.Threading.Tasks; // Added for Task

namespace backend.Repositories.Interfaces
{
    // Defines operations for accessing and managing User entities.
    public interface IUserRepository
    {
        // Retrieves all users including their roles and tasks.
        Task<IEnumerable<User>> GetAllUsersWithRolesAndTasksAsync();
        // Retrieves a user by their ID.
        Task<User?> GetUserByIdAsync(Guid userId);
        // Retrieves a user by their email address.
        Task<User?> GetUserByEmailAsync(string email);
        // Checks if a user with the given ID exists.
        Task<bool> UserExistsAsync(Guid userId);
        // Checks if a user with the given email exists.
        Task<bool> UserExistsByEmailAsync(string email);
        // Adds a new user to the repository.
        Task AddUserAsync(User user);
        // Updates an existing user in the repository.
        Task UpdateUserAsync(User user);
        // Deletes a user from the repository.
        Task DeleteUserAsync(User user);
        // Saves all changes made in the current context.
        Task SaveChangesAsync();
        // Retrieves a user by ID including their roles.
        Task<User?> GetUserByIdWithRolesAsync(Guid id);


        // Methods specifically tailored for the Task Service.
        Task<bool> UserExistsByIdAsync(Guid id);


        // Methods specifically tailored for the User Service.
        Task<UserDto?> GetUserDtoByIdAsync(Guid userId);
        Task<bool> UserExistsByEmailExcludingIdAsync(string email, Guid userId);
        Task<string?> GetUserStatusByIdAsync(Guid userId);
    }
}