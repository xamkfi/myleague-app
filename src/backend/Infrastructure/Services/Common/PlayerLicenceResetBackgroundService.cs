using Application.Features.Common.PlayerLicences.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyLeague.Infrastructure.Services.Common;

/// <summary>
/// Checks the yearly player-licence cutoff on startup and about every six hours.
/// </summary>
public class PlayerLicenceResetBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PlayerLicenceResetBackgroundService> _logger;

    public PlayerLicenceResetBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<PlayerLicenceResetBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
                IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new ResetExpiredPlayerLicencesCommand(Force: false), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Player licence reset check failed");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Player licence reset check failed");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Player licence reset check failed");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
        }
    }
}
