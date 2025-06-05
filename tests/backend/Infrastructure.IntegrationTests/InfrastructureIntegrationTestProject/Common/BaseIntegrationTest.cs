using MyLeague.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InfrastructureIntegrationTestProject.Common;

public abstract class BaseIntegrationTest : IDisposable
{
    protected readonly CommonDbContext _dbContext;
    protected readonly IServiceProvider _serviceProvider;
    private bool _disposed = false;

    protected BaseIntegrationTest()
    {
        var services = new ServiceCollection();

        // Use InMemory database for faster tests while still testing EF Core behavior
        services.AddDbContext<CommonDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<CommonDbContext>();

        // Ensure database is created
        _dbContext.Database.EnsureCreated();
    }

    protected async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            T result = await operation();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    protected async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            await operation();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    protected void ClearDatabase()
    {
        _dbContext.NewsArticles.RemoveRange(_dbContext.NewsArticles);
        _dbContext.Clubs.RemoveRange(_dbContext.Clubs);
        _dbContext.Persons.RemoveRange(_dbContext.Persons);
        _dbContext.SaveChanges();
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
            _dbContext?.Dispose();
            _serviceProvider?.GetService<IServiceScope>()?.Dispose();
            _disposed = true;
        }
    }
} 