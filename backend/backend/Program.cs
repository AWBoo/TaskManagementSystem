using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using backend.Middleware;
using backend.Interfaces;
using backend.Services;
using backend.Repositories;
using backend.Repositories.Interfaces;
using Microsoft.Extensions.Logging; // Add this using directive for ILogger

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register HttpContextAccessor for accessing HttpContext throughout the application.
builder.Services.AddHttpContextAccessor();

// Configure the ApplicationDBContext to use SQL Server with the connection string from configuration.
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add controllers to the service collection.
builder.Services.AddControllers();

// Define a CORS policy name.
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Repository Implementations for Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskItemRepository, TaskItemRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IAuditEntryRepository, AuditEntryRepository>();

// Register JWT Service for Dependency Injection
// Correctly inject ILogger<JwtService> into the JwtService constructor
builder.Services.AddScoped<IJwtService, JwtService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var userRepository = provider.GetRequiredService<IUserRepository>();
    var logger = provider.GetRequiredService<ILogger<JwtService>>(); 
    return new JwtService(config, userRepository, logger); 
});

// Configure JWT Bearer authentication.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Retrieve JWT signing key from configuration.
    var key = builder.Configuration["JwtSettings:Key"];
    if (string.IsNullOrEmpty(key))
    {
        // Throw an exception if the JWT key is not configured, as it's critical.
        throw new InvalidOperationException("JWT Key is not configured in appsettings.json.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validate the issuer of the token.
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        // Validate the audience of the token.
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        // Validate the signing key of the token.
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        // Validate the token's lifetime (expiration).
        ValidateLifetime = true
    };
});

// Configure CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
    policy =>
    {
        // Allow requests from the specified origin (e.g., your frontend application. This needs to change depending on what port you frontend is running.).
        policy.WithOrigins("http://localhost:5173")
            // Allow any header in the requests.
            .AllowAnyHeader()
            // Allow any HTTP method (GET, POST, PUT, DELETE, etc.).
            .AllowAnyMethod();
    });
});

//Configure Logging
builder.Logging.ClearProviders(); // Clear default logging providers.
builder.Logging.AddConsole(); // Add console logger.
builder.Logging.AddDebug(); // Add debug output logger.

// Register Application Services for Dependency Injection
// These services will automatically get ILogger<T> injected because they are registered
// using the simpler AddScoped<Interface, Implementation>() pattern, and ILogger is
// provided by the default logging setup.
builder.Services.AddScoped<IProjectsService, ProjectsService>();
builder.Services.AddScoped<ITasksService, TasksService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Register the custom Exception Handling Middleware to catch and handle exceptions globally.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable Swagger UI in development environment.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Apply the defined CORS policy.
app.UseCors(MyAllowSpecificOrigins);

// Enable routing for incoming requests.
app.UseRouting();

// Enable authentication middleware.
app.UseAuthentication();
// Enable authorization middleware.
app.UseAuthorization();

// Register the custom User Status Check Middleware to enforce user status.
app.UseMiddleware<UserStatusCheckMiddleware>();

// Map controller endpoints.
app.MapControllers();

// This block ensures the database is up-to-date and seeded with initial data on application startup.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDBContext>();
        // Ensure the database is created and migrations are applied.
        context.Database.Migrate();

        // Seed initial roles if they do not exist.
        if (!context.Roles.Any())
        {
            context.Roles.Add(new Role { Id = Guid.NewGuid(), Name = "Admin" });
            context.Roles.Add(new Role { Id = Guid.NewGuid(), Name = "User" });
            await context.SaveChangesAsync();
        }

        // Seed a default Admin user and assign the Admin role if they do not exist.
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole != null && !context.Users.Any(u => u.Email == "ash@ash.ash"))
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "Admin@Admin.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin!"),
                Status = "Active" // Set default status for the seeded admin user.
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            // Assign Admin role to the newly created admin user.
            context.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedAt = DateTime.UtcNow,
                User = adminUser,
                Role = adminRole
            });
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        // Log any errors that occur during database migration or seeding.
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
