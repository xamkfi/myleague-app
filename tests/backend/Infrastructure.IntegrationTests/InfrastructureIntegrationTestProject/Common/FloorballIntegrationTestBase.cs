using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories.Floorball;

namespace InfrastructureIntegrationTestProject.Common;

/// <summary>
/// InMemory harness for FloorballDbContext + repositories.
/// Note: InMemory does not exercise PostgreSQL-specific mapping edge cases.
/// </summary>
public abstract class FloorballIntegrationTestBase : IDisposable
{
    protected readonly FloorballDbContext DbContext;
    protected readonly FloorballCompetitionRepository CompetitionRepository;
    protected readonly FloorballTeamRepository TeamRepository;
    protected readonly FloorballMatchRepository MatchRepository;
    private readonly ServiceProvider _serviceProvider;
    private bool _disposed;

    protected FloorballIntegrationTestBase()
    {
        ServiceCollection services = new();
        string dbName = $"FloorballTestDb_{Guid.NewGuid()}";
        services.AddDbContext<FloorballDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        _serviceProvider = services.BuildServiceProvider();
        DbContext = _serviceProvider.GetRequiredService<FloorballDbContext>();
        DbContext.Database.EnsureCreated();

        CompetitionRepository = new FloorballCompetitionRepository(DbContext);
        TeamRepository = new FloorballTeamRepository(DbContext);
        MatchRepository = new FloorballMatchRepository(DbContext);
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
