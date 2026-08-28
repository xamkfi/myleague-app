using Application.Features.Football.Tournaments.Commands;
using Application.Features.Football.Tournaments.DTOs;
using Application.Features.Football.Tournaments.Mappings;
using Application.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Entities.Football.Statistics;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Football.Tournaments.Handlers;

/// <summary>
/// Handler for creating a new football tournament
/// </summary>
public class CreateFootballTournamentHandler : IRequestHandler<CreateFootballTournamentCommand, Result<FootballTournamentDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFootballTournamentHandler> _logger;

    public CreateFootballTournamentHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<CreateFootballTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTournamentDto>> Handle(CreateFootballTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            DateTime startDateUtc = DateTimeUtc.Normalize(request.StartDate);
            DateTime endDateUtc = DateTimeUtc.Normalize(request.EndDate);

            FootballMatchRules groupStageMatchRules = new FootballMatchRules(
                request.GroupStageNumberOfHalves,
                request.GroupStageHalfDurationMinutes,
                request.GroupStagePlayersOnField,
                request.GroupStageRequireGoalkeeper,
                request.GroupStageMaxSubstitutions,
                request.GroupStageRequireOfficialsToStart,
                request.GroupStageAllowExtraTime,
                request.GroupStageExtraTimeHalfCount,
                request.GroupStageExtraTimeHalfDurationMinutes,
                request.GroupStageAllowPenaltyShootout);

            FootballMatchRules playoffMatchRules = new FootballMatchRules(
                request.PlayoffNumberOfHalves,
                request.PlayoffHalfDurationMinutes,
                request.PlayoffPlayersOnField,
                request.PlayoffRequireGoalkeeper,
                request.PlayoffMaxSubstitutions,
                request.PlayoffRequireOfficialsToStart,
                request.PlayoffAllowExtraTime,
                request.PlayoffExtraTimeHalfCount,
                request.PlayoffExtraTimeHalfDurationMinutes,
                request.PlayoffAllowPenaltyShootout);

            FootballTournamentRules tournamentRules = new FootballTournamentRules(
                groupStageMatchRules,
                playoffMatchRules,
                request.TeamsAdvancingPerGroup,
                request.HasPlayoffStage,
                request.HasThirdPlaceMatch);

            List<FootballPlayoffScheduleSlot>? playoffSchedule = null;
            if (request.PlayoffSchedule != null && request.PlayoffSchedule.Count > 0)
            {
                playoffSchedule = request.PlayoffSchedule
                    .Select(s => new FootballPlayoffScheduleSlot(
                        s.Round,
                        s.Order,
                        DateTimeUtc.Normalize(s.ScheduledDateTime),
                        s.Venue))
                    .ToList();
            }

            FootballTournament tournament = new FootballTournament(
                request.Name,
                startDateUtc,
                endDateUtc,
                request.Venue,
                request.ContentHtml,
                tournamentRules,
                playoffSchedule,
                request.TeamCategory);

            _logger.LogInformation("Creating new football tournament: {Name}", request.Name);
            await _tournamentRepository.AddAsync(tournament);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballTournamentDto tournamentDto = FootballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully created football tournament with ID: {TournamentId}", tournament.Id);

            return Result<FootballTournamentDto>.Success(tournamentDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while creating football tournament: {Name}", request.Name);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating football tournament: {Name}", request.Name);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating football tournament: {Name}", request.Name);
            return Result<FootballTournamentDto>.Failure(
                "An error occurred while creating the football tournament.",
                ex.Flatten());
        }
    }
}
