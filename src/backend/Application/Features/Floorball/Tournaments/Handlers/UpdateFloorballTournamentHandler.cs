using Application.Features.Floorball.Tournaments.Commands;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Tournaments.Handlers;

/// <summary>
/// Handler for updating an existing floorball tournament
/// </summary>
public class UpdateFloorballTournamentHandler : IRequestHandler<UpdateFloorballTournamentCommand, Result<FloorballTournamentDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFloorballTournamentHandler> _logger;

    public UpdateFloorballTournamentHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<UpdateFloorballTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTournamentDto>> Handle(UpdateFloorballTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId);
            if (tournament == null)
            {
                _logger.LogWarning("Attempt to update non-existent floorball tournament with ID: {TournamentId}", request.CompetitionId);
                return Result<FloorballTournamentDto>.NotFound("FloorballTournament", request.CompetitionId);
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

            FloorballMatchRules groupStageMatchRules = new FloorballMatchRules(
                request.GroupStageNumberOfPeriods,
                request.GroupStagePeriodDurationMinutes,
                request.GroupStageAllowOvertime,
                request.GroupStageOvertimeDurationMinutes,
                request.GroupStageAllowShootout);

            FloorballMatchRules playoffMatchRules = new FloorballMatchRules(
                request.PlayoffNumberOfPeriods,
                request.PlayoffPeriodDurationMinutes,
                request.PlayoffAllowOvertime,
                request.PlayoffOvertimeDurationMinutes,
                request.PlayoffAllowShootout);

            FloorballTournamentRules tournamentRules = new FloorballTournamentRules(
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
                List<PlayoffScheduleSlot> slots = request.PlayoffSchedule
                    .Select(s => new PlayoffScheduleSlot(s.Round, s.Order, s.ScheduledDateTime, s.Venue))
                    .ToList();
                tournament.SetPlayoffSchedule(slots);
            }

            _logger.LogInformation("Updating floorball tournament: {TournamentId}", tournament.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTournamentDto tournamentDto = FloorballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully updated floorball tournament with ID: {TournamentId}", tournament.Id);

            return Result<FloorballTournamentDto>.Success(tournamentDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while updating tournament: {TournamentId}", request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while updating tournament: {TournamentId}", request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating floorball tournament: {TournamentId}", request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure(
                "An error occurred while updating the floorball tournament.",
                ex.Flatten());
        }
    }
}
