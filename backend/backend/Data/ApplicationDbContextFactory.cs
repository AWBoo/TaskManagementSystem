// backend/Data/ApplicationDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


namespace backend.Data
{
    // This factory is only used by design-time tools 
    public class ApplicationDBContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
    {
        public ApplicationDBContext CreateDbContext(string[] args)
        {
            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json") // Ensure this path is correct relative to where you run `dotnet ef`
                .Build();

            // Configure DbContextOptions
            var builder = new DbContextOptionsBuilder<ApplicationDBContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.UseSqlServer(connectionString);

            var httpContextAccessor = new HttpContextAccessor();

            return new ApplicationDBContext(builder.Options, httpContextAccessor);
        }
    }
}