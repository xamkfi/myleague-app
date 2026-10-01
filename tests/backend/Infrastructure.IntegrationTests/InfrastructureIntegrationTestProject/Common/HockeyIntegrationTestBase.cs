using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories.Hockey;

namespace InfrastructureIntegrationTestProject.Common;

/// <summary>
/// InMemory harness for HockeyDbContext + repositories.
/// Note: InMemory does not exercise PostgreSQL-specific mapping edge cases.
/// </summary>
public abstract class HockeyIntegrationTestBase : IDisposable
{
    protected readonly HockeyDbContext DbContext;
    protected readonly HockeyMatchRepository MatchRepository;
    private readonly ServiceProvider _serviceProvider;
    private bool _disposed;

    protected HockeyIntegrationTestBase()
    {
        ServiceCollection services = new();
        string dbName = $"HockeyTestDb_{Guid.NewGuid()}";
        services.AddDbContext<HockeyDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        _serviceProvider = services.BuildServiceProvider();
        DbContext = _serviceProvider.GetRequiredService<HockeyDbContext>();
        DbContext.Database.EnsureCreated();

        MatchRepository = new HockeyMatchRepository(DbContext);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            DbContext.Dispose();
            _serviceProvider.Dispose();
            _disposed = true;
        }
    }
}
