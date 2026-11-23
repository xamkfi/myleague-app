using Application.Common;
using Application.Features.Common.CrossCutting.MatchTimer.Services;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Matches;
using Domain.Enums.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Deletes a hockey match and recalculates the competition statistics it counted towards.
/// A playoff match can only be deleted before it starts.
/// </summary>
public class DeleteHockeyMatchHandler : IRequestHandler<DeleteHockeyMatchCommand, Result>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IMediator _mediator;
    private readonly IMatchTimerService _timerService;
    private readonly ILogger<DeleteHockeyMatchHandler> _logger;

    public DeleteHockeyMatchHandler(
        IHockeyMatchRepository matchRepository,
        IMediator mediator,
        IMatchTimerService timerService,
        ILogger<DeleteHockeyMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _mediator = mediator;
        _timerService = timerService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteHockeyMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
                return Result.NotFound("HockeyMatch", request.MatchId);

            bool notStarted = match.Status is HockeyMatchStatus.Scheduled or HockeyMatchStatus.Warmup;

            // Bracket advancement is not rolled back, so a playoff match can only be removed before it starts.
            if (match.IsPlayoffBracketMatch && !notStarted)
                return Result.Failure("A playoff match that has been started cannot be deleted.");

            HockeyMatchDto deletedMatch = HockeyMatchMapper.ToDto(match);
            await _matchRepository.DeleteMatchAsync(match.Id, cancellationToken);
            _logger.LogInformation(
                "Deleted hockey match {MatchId} with status {Status}", request.MatchId, deletedMatch.Status);

            if (notStarted)
                return Result.Success();

            await _timerService.DestroyTimerAsync(match.Id);
            await HockeyMatchHandlerSupport.RecalculateCompetitionStatisticsAsync(
                _mediator, _logger, deletedMatch, cancellationToken);

            return Result.Success();
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
