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
/// Handler for updating an existing football tournament
/// </summary>
public class UpdateFootballTournamentHandler : IRequestHandler<UpdateFootballTournamentCommand, Result<FootballTournamentDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFootballTournamentHandler> _logger;

    public UpdateFootballTournamentHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<UpdateFootballTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTournamentDto>> Handle(UpdateFootballTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId);
            if (tournament == null)
            {
                _logger.LogWarning("Attempt to update non-existent football tournament with ID: {TournamentId}", request.CompetitionId);
                return Result<FootballTournamentDto>.NotFound("FootballTournament", request.CompetitionId);
            }

            DateTime startDateUtc = request.StartDate.Kind switch
            {
                DateTimeKind.Utc => request.StartDate,
                DateTimeKind.Local => request.StartDate.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc)
            };

            DateTime endDateUtc = request.EndDate.Kind switch
            {
                DateTimeKind.Utc => request.EndDate,
                DateTimeKind.Local => request.EndDate.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc)
            };

            tournament.UpdateDetails(request.Name, startDateUtc, endDateUtc);
            tournament.UpdateContent(request.ContentHtml);
            tournament.UpdateVenue(request.Venue);
            if (request.TeamCategory.HasValue)
            {
                tournament.UpdateTeamCategory(request.TeamCategory.Value);
            }

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

            tournament.UpdateTournamentRules(tournamentRules);

            // Only touch the playoff schedule when the caller explicitly sends one. A null value
            // means "do not change" so that update flows that don't know/care about the schedule
            // (legacy admin form, etc.) won't accidentally wipe the slots imported earlier.
            if (request.PlayoffSchedule != null)
            {
                List<FootballPlayoffScheduleSlot> slots = request.PlayoffSchedule
                    .Select(s => new FootballPlayoffScheduleSlot(s.Round, s.Order, s.ScheduledDateTime, s.Venue))
                    .ToList();
                tournament.SetPlayoffSchedule(slots);
            }

            _logger.LogInformation("Updating football tournament: {TournamentId}", tournament.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballTournamentDto tournamentDto = FootballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully updated football tournament with ID: {TournamentId}", tournament.Id);

            return Result<FootballTournamentDto>.Success(tournamentDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while updating tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while updating tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating football tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(
                "An error occurred while updating the football tournament.",
                ex.Flatten());
        }
    }
}
