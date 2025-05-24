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
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            DbContextOptionsBuilder<FloorballDbContext> builder = new DbContextOptionsBuilder<FloorballDbContext>();
            string? connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseNpgsql(connectionString ?? "Host=localhost;Database=myleague;Username=postgres;Password=postgres");

            return new FloorballDbContext(builder.Options);
        }
    }
} 