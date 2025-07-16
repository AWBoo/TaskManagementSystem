using backend.Models;
using System.Security.Claims;

namespace backend.Interfaces
{
    public interface IJwtService
    {
        //Generates TOKEN
        Task<string> GenerateToken(User user);
    }
}