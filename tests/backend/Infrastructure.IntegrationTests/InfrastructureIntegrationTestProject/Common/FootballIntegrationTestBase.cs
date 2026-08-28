using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories.Football;

namespace InfrastructureIntegrationTestProject.Common;

/// <summary>
/// InMemory harness for FootballDbContext + repositories.
/// Note: InMemory does not exercise PostgreSQL-specific mapping edge cases.
/// </summary>
public abstract class FootballIntegrationTestBase : IDisposable
{
    protected readonly FootballDbContext DbContext;
    protected readonly FootballCompetitionRepository CompetitionRepository;
    protected readonly FootballTeamRepository TeamRepository;
    protected readonly FootballMatchRepository MatchRepository;
    private readonly ServiceProvider _serviceProvider;
    private bool _disposed;

    protected FootballIntegrationTestBase()
    {
        ServiceCollection services = new();
        string dbName = $"FootballTestDb_{Guid.NewGuid()}";
        services.AddDbContext<FootballDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        _serviceProvider = services.BuildServiceProvider();
        DbContext = _serviceProvider.GetRequiredService<FootballDbContext>();
        DbContext.Database.EnsureCreated();

        CompetitionRepository = new FootballCompetitionRepository(DbContext);
        TeamRepository = new FootballTeamRepository(DbContext);
        MatchRepository = new FootballMatchRepository(DbContext);
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
