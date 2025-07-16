using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace backend.Middleware
{
    // Custom middleware for global exception handling.
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        // Initializes the middleware with necessary dependencies.
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        // Invokes the next middleware in the pipeline or handles an exception.
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // Logs the unhandled exception details.
                _logger.LogError(ex, "An unhandled exception occurred during request processing. Path: {RequestPath}, Method: {RequestMethod}",
                    httpContext.Request.Path, httpContext.Request.Method);

                // Handles the exception and sends a consistent error response.
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        // Formats and sends an error response based on the environment.
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            object errorResponse;

            // Exposes detailed error messages in development.
            if (_env.IsDevelopment())
            {
                errorResponse = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = exception.Message,
                    Detail = exception.ToString()
                };
            }
            // Provides generic error messages in production for security.
            else
            {
                errorResponse = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "An unexpected error occurred. Please try again later."
                };
            }

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}