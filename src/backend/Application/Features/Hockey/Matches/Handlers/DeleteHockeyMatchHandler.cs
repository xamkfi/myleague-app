using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Domain.Entities.Hockey.Matches;
using Domain.Enums.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Deletes a hockey match only while it is still scheduled.
/// </summary>
public class DeleteHockeyMatchHandler : IRequestHandler<DeleteHockeyMatchCommand, Result>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly ILogger<DeleteHockeyMatchHandler> _logger;

    public DeleteHockeyMatchHandler(
        IHockeyMatchRepository matchRepository,
        ILogger<DeleteHockeyMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteHockeyMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool deleted = await _matchRepository.DeleteIfScheduledAsync(request.MatchId, cancellationToken);
            if (deleted)
            {
                _logger.LogInformation("Deleted scheduled hockey match {MatchId}", request.MatchId);
                return Result.Success();
            }

            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
                return Result.NotFound("HockeyMatch", request.MatchId);

            return Result.Failure(
                $"Only matches in the Scheduled state can be deleted (current status: {match.Status}).");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database rejected deleting hockey match {MatchId}", request.MatchId);
            return Result.Failure("Saving the match deletion conflicted with existing data.");
        }
    }
}
