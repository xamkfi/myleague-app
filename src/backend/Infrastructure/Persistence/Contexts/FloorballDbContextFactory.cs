using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace MyLeague.Infrastructure.Persistence.Contexts
{
    /// <summary>
    /// A factory for creating FloorballDbContext instances at design time.
    /// </summary>
    public class FloorballDbContextFactory : IDesignTimeDbContextFactory<FloorballDbContext>
    {
        /// <summary>
        /// Creates a new instance of FloorballDbContext.
        /// </summary>
        /// <param name="args">Arguments provided by the design-time service.</param>
        /// <returns>An instance of FloorballDbContext.</returns>
        public FloorballDbContext CreateDbContext(string[] args)
        {
            // Navigate to the parent directory (src/backend) where appsettings.json is located
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..");
            if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
            {
                // Fallback: try different path combinations
                basePath = Directory.GetCurrentDirectory();
                while (!File.Exists(Path.Combine(basePath, "appsettings.json")) && Directory.GetParent(basePath) != null)
                {
                    basePath = Directory.GetParent(basePath)!.FullName;
                }
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            DbContextOptionsBuilder<FloorballDbContext> builder = new DbContextOptionsBuilder<FloorballDbContext>();
            string? connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseNpgsql(connectionString);

            return new FloorballDbContext(builder.Options);
        }
    }
} 
