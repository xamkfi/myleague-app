using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace MyLeague.Infrastructure.Persistence.Contexts
{
    /// <summary>
    /// A factory for creating CommonDbContext instances at design time.
    /// </summary>
    public class CommonDbContextFactory : IDesignTimeDbContextFactory<CommonDbContext>
    {
        /// <summary>
        /// Creates a new instance of CommonDbContext.
        /// </summary>
        /// <param name="args">Arguments provided by the design-time service.</param>
        /// <returns>An instance of CommonDbContext.</returns>
        public CommonDbContext CreateDbContext(string[] args)
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

            DbContextOptionsBuilder<CommonDbContext> builder = new DbContextOptionsBuilder<CommonDbContext>();
            string? connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseNpgsql(connectionString ?? "Host=localhost;Database=myleague;Username=postgres;Password=postgres");

            return new CommonDbContext(builder.Options);
        }
    }
} 