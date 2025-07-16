using backend.Models;
using backend.Interfaces;
using backend.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services
{
    // Service for generating JSON Web Tokens (JWTs).
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<JwtService> _logger; 

        // Injects IConfiguration for JWT settings, IUserRepository for user data, and ILogger.
        public JwtService(IConfiguration config, IUserRepository userRepository, ILogger<JwtService> logger) 
        {
            _config = config;
            _userRepository = userRepository;
            _logger = logger; 
        }

        // Generates a JWT for the provided user, including roles as claims.
        public async Task<string> GenerateToken(User user)
        {
            _logger.LogInformation("JwtService: Generating token for user ID '{UserId}' ({UserEmail}).", user.Id, user.Email);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            // Retrieves the user with their associated roles.
            var userWithRoles = await _userRepository.GetUserByIdWithRolesAsync(user.Id);

            // Adds role claims to the token.
            if (userWithRoles != null && userWithRoles.UserRoles != null && userWithRoles.UserRoles.Any())
            {
                foreach (var userRole in userWithRoles.UserRoles)
                {
                    if (userRole.Role != null) // Ensure Role is not null before accessing Name
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                        _logger.LogDebug("JwtService: Added role '{RoleName}' to claims for user ID '{UserId}'.", userRole.Role.Name, user.Id);
                    }
                    else
                    {
                        _logger.LogWarning("JwtService: UserRole found for user ID '{UserId}' but Role object was null. Skipping role claim.", user.Id);
                    }
                }
            }
            else
            {
                // Assigns a default "User" role if no explicit roles are found.
                claims.Add(new Claim(ClaimTypes.Role, "User"));
                _logger.LogInformation("JwtService: No explicit roles found for user ID '{UserId}'. Assigning default 'User' role.", user.Id);
            }

            // Retrieves JWT key from configuration and creates signing credentials.
            string? jwtKey = _config["JwtSettings:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogCritical("JwtService: JWT Key is not configured in appsettings. This is a critical security configuration error.");
                throw new InvalidOperationException("JWT Key not configured.");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Parses token expiry minutes from configuration, defaults to 60 if invalid.
            double expiryMinutes = 60; // Default value
            if (double.TryParse(_config["JwtSettings:ExpiryMinutes"], out double parsedExpiryMinutes))
            {
                expiryMinutes = parsedExpiryMinutes;
                _logger.LogDebug("JwtService: Using configured JWT expiry minutes: {ExpiryMinutes}.", expiryMinutes);
            }
            else
            {
                _logger.LogWarning("JwtService: 'JwtSettings:ExpiryMinutes' is missing or invalid in configuration. Defaulting to {DefaultExpiry} minutes.", expiryMinutes);
            }

            string? issuer = _config["JwtSettings:Issuer"];
            string? audience = _config["JwtSettings:Audience"];

            if (string.IsNullOrEmpty(issuer))
            {
                _logger.LogWarning("JwtService: 'JwtSettings:Issuer' is not configured. Token issuer will be empty.");
            }
            if (string.IsNullOrEmpty(audience))
            {
                _logger.LogWarning("JwtService: 'JwtSettings:Audience' is not configured. Token audience will be empty.");
            }


            // Constructs the JWT token with claims, issuer, audience, and expiry.
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("JwtService: Successfully generated token for user ID '{UserId}'. Token length: {TokenLength} characters.", user.Id, tokenString.Length);

            // Returns the serialized JWT string.
            return tokenString;
        }
    }
}