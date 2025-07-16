using System.Security.Claims;
using System.Net;
using backend.Interfaces;

namespace backend.Middleware
{
    // Middleware to check user account status for access control.
    public class UserStatusCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserStatusCheckMiddleware> _logger;

        // Initializes the middleware with the next request delegate and logger.
        public UserStatusCheckMiddleware(RequestDelegate next, ILogger<UserStatusCheckMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Invokes the middleware to check user status before proceeding.
        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Allows access to authentication endpoints regardless of user status.
            if (path != null && (path.Contains("/api/auth/login") || path.Contains("/api/auth/register")))
            {
                await _next(context);
                return;
            }

            // Checks user status only if the user is authenticated.
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    // Logs warning if user ID is missing or invalid despite authentication.
                    _logger.LogWarning("User authenticated but User ID claim not found or invalid format.");
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { message = "Authentication failed: User ID not found." });
                    return;
                }

                var userStatus = await userService.GetUserStatusAsync(userId);

                // Blocks access for deactivated users.
                if (userStatus == "deactivated")
                {
                    _logger.LogInformation($"Deactivated user attempted access: {userId}");
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { message = "Your account has been deactivated. Please contact support." });
                    return;
                }
                // Blocks access for suspended users.
                else if (userStatus == "suspended")
                {
                    _logger.LogInformation($"Suspended user attempted access: {userId}");
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { message = "Your account has been suspended. Please contact support for more information." });
                    return;
                }
            }

            await _next(context);
        }
    }
}