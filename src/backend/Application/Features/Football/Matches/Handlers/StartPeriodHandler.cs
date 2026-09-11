using Application.Common;
using Application.Features.Common.MatchTimer.Services;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Domain.Entities.Football.Matches;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class StartPeriodHandler : IRequestHandler<StartPeriodCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly IMatchTimerService _timerService;
    private readonly ILogger<StartPeriodHandler> _logger;

    public StartPeriodHandler(
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        IMatchTimerService timerService,
        ILogger<StartPeriodHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _timerService = timerService;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(StartPeriodCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                return Result<FootballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            if (match.Status != FootballMatchStatus.InProgress)
            {
                return Result<FootballMatchDto>.Failure(
                    $"Match must be in progress to start a period. Current status: {match.Status}");
            }

            int maxPeriod = match.MatchRules.MaxPeriodNumber;
            if (request.PeriodNumber < 1 || request.PeriodNumber > maxPeriod)
            {
                return Result<FootballMatchDto>.Failure(
                    $"Period number must be between 1 and {maxPeriod}. Received: {request.PeriodNumber}");
            }

            if (!match.PeriodScores.Any(ps => ps.PeriodNumber == request.PeriodNumber))
            {
                return Result<FootballMatchDto>.Failure(
                    $"Period {request.PeriodNumber} has not been initialized. For extra time or a penalty shootout, call the appropriate record endpoint first.");
            }

            if (!match.MatchRules.IsPenaltyShootoutPeriod(request.PeriodNumber))
            {
                await _timerService.StartTimerAsync(match.Id, request.PeriodNumber);
            }

            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting period {PeriodNumber} for match {MatchId}", request.PeriodNumber, request.MatchId);
            return Result<FootballMatchDto>.Failure("An error occurred while starting the period.");
        }
    }
}
