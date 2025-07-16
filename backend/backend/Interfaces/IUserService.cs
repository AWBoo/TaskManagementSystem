using backend.DTOS.User;
using backend.DTOS.User.dashboard;
using System;
using System.Threading.Tasks;

namespace backend.Interfaces
{
    public interface IUserService
    {
        // Retrieves an authenticated user's profile.
        Task<UserDto?> GetUserProfileAsync(Guid userId);
        // Updates an authenticated user's profile.
        Task<bool> UpdateUserProfileAsync(Guid userId, UpdateProfileRequestDto request);
        // Allows an authenticated user to change their password.
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
        // Retrieves an authenticated user's account status.
        Task<string?> GetUserStatusAsync(Guid userId);
        // Retrieves dashboard-related data for an authenticated user.
        Task<UserDashboardDto> GetUserDashboardDataAsync(Guid userId);
    }
}