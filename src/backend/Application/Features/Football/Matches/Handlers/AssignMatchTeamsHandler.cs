using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class AssignMatchTeamsHandler : IRequestHandler<AssignMatchTeamsCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<AssignMatchTeamsHandler> _logger;

    public AssignMatchTeamsHandler(
        IFootballMatchRepository matchRepository,
        IFootballTeamRepository teamRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<AssignMatchTeamsHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(AssignMatchTeamsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                return Result<FootballMatchDto>.NotFound("FootballMatch", request.MatchId);
            }

            if (match.Status != FootballMatchStatus.Scheduled && match.Status != FootballMatchStatus.Postponed)
            {
                return Result<FootballMatchDto>.Failure(
                    $"Teams can only be changed for scheduled or postponed matches. Current status: {match.Status}.");
            }

            FootballTeam? homeTeam = null;
            if (request.HomeTeamId.HasValue)
            {
                homeTeam = await _teamRepository.GetByIdAsync(request.HomeTeamId.Value);
                if (homeTeam == null)
                {
                    return Result<FootballMatchDto>.NotFound("FootballTeam", request.HomeTeamId.Value);
                }
            }

            FootballTeam? awayTeam = null;
            if (request.AwayTeamId.HasValue)
            {
                awayTeam = await _teamRepository.GetByIdAsync(request.AwayTeamId.Value);
                if (awayTeam == null)
                {
                    return Result<FootballMatchDto>.NotFound("FootballTeam", request.AwayTeamId.Value);
                }
            }

            Guid? previousHomeTeamId = match.HomeTeamId;

            match.AssignTeam(FootballPlayoffSlot.Home, homeTeam);
            match.AssignTeam(FootballPlayoffSlot.Away, awayTeam);

            if (match.NextMatchId.HasValue
                && match.NextMatchSlot.HasValue
                && previousHomeTeamId != match.HomeTeamId)
            {
                await PropagateProjectedHomeTeamAsync(match);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rule rejected AssignMatchTeams for match {MatchId}", request.MatchId);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument in AssignMatchTeams for match {MatchId}", request.MatchId);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AssignMatchTeams for match {MatchId}", request.MatchId);
            return Result<FootballMatchDto>.Failure("An error occurred while updating match teams.");
        }
    }

    private async Task PropagateProjectedHomeTeamAsync(FootballMatch source)
    {
        FootballMatch? cursor = source;

        while (cursor != null && cursor.NextMatchId.HasValue && cursor.NextMatchSlot.HasValue)
        {
            FootballMatch? nextMatch = await _matchRepository.GetByIdAsync(cursor.NextMatchId.Value);
            if (nextMatch == null)
            {
                return;
            }

            if (nextMatch.Status != FootballMatchStatus.Scheduled
                && nextMatch.Status != FootballMatchStatus.Postponed)
            {
                return;
            }

            nextMatch.AssignTeam(cursor.NextMatchSlot.Value, cursor.HomeTeam);
            cursor = nextMatch;
        }
    }
}
