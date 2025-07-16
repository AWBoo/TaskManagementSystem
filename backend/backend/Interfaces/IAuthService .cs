using backend.DTOS.Auth;
using backend.DTOS.User;
using System.Threading.Tasks;

namespace backend.Interfaces
{
    public interface IAuthService
    {
        // Returns AuthResponseDto on success, null or specific message on failure
        Task<AuthResponseDto?> RegisterAsync(RegisterRequest request);

        // Returns AuthResponseDto on success, null or specific message on failure
        Task<AuthResponseDto?> LoginAsync(LoginRequest request);
    }
}