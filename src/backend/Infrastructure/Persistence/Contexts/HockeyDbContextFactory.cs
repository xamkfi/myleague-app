using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MyLeague.Infrastructure.Persistence.Contexts;

/// <summary>
/// Design-time factory for <see cref="HockeyDbContext"/>.
/// </summary>
public class HockeyDbContextFactory : IDesignTimeDbContextFactory<HockeyDbContext>
{
    public HockeyDbContext CreateDbContext(string[] args)
    {
        string basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..");
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
        {
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

        DbContextOptionsBuilder<HockeyDbContext> builder = new();
        builder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

        return new HockeyDbContext(builder.Options);
    }
}
