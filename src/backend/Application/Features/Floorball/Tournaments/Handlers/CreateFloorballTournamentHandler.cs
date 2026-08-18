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
/// Handler for creating a new floorball tournament
/// </summary>
public class CreateFloorballTournamentHandler : IRequestHandler<CreateFloorballTournamentCommand, Result<FloorballTournamentDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballTournamentHandler> _logger;

    public CreateFloorballTournamentHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<CreateFloorballTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTournamentDto>> Handle(CreateFloorballTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
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

            List<PlayoffScheduleSlot>? playoffSchedule = null;
            if (request.PlayoffSchedule != null && request.PlayoffSchedule.Count > 0)
            {
                playoffSchedule = request.PlayoffSchedule
                    .Select(s => new PlayoffScheduleSlot(s.Round, s.Order, s.ScheduledDateTime, s.Venue))
                    .ToList();
            }

            FloorballTournament tournament = new FloorballTournament(
                request.Name,
                startDateUtc,
                endDateUtc,
                request.Venue,
                request.ContentHtml,
                tournamentRules,
                playoffSchedule,
                request.TeamCategory);

            _logger.LogInformation("Creating new floorball tournament: {Name}", request.Name);
            await _tournamentRepository.AddAsync(tournament);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTournamentDto tournamentDto = FloorballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully created floorball tournament with ID: {TournamentId}", tournament.Id);

            return Result<FloorballTournamentDto>.Success(tournamentDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while creating floorball tournament: {Name}", request.Name);
            return Result<FloorballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating floorball tournament: {Name}", request.Name);
            return Result<FloorballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating floorball tournament: {Name}", request.Name);
            return Result<FloorballTournamentDto>.Failure(
                "An error occurred while creating the floorball tournament.",
                ex.Flatten());
        }
    }
}
