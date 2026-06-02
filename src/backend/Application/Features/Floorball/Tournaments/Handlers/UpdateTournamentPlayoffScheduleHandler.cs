using Application.Common;
using Application.Features.Floorball.Tournaments.Commands;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Mappings;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Floorball;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Tournaments.Handlers;

/// <summary>
/// Replaces the tournament's <see cref="FloorballTournament.PlayoffSchedule"/> in a single
/// targeted update. The domain entity enforces the lifecycle rule (no edits once the bracket
/// has been generated) — see <see cref="FloorballTournament.SetPlayoffSchedule"/>.
/// </summary>
public class UpdateTournamentPlayoffScheduleHandler : IRequestHandler<UpdateTournamentPlayoffScheduleCommand, Result<FloorballTournamentDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTournamentPlayoffScheduleHandler> _logger;

    public UpdateTournamentPlayoffScheduleHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<UpdateTournamentPlayoffScheduleHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTournamentDto>> Handle(UpdateTournamentPlayoffScheduleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Need the tracked entity + groups so the response DTO renders fully (group standings
            // etc.) the moment the admin saves.
            FloorballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId, cancellationToken);
            if (tournament == null)
            {
                _logger.LogWarning("Attempt to update playoff schedule for non-existent tournament {TournamentId}", request.CompetitionId);
                return Result<FloorballTournamentDto>.NotFound("FloorballTournament", request.CompetitionId);
            }

            // SetPlayoffSchedule itself rejects duplicate (round, order) pairs and throws when the
            // tournament has already advanced past the planning window. Converting the inputs to
            // value objects first lets the domain run its own validation before we touch EF state.
            List<PlayoffScheduleSlot> slots = (request.Slots ?? Array.Empty<PlayoffScheduleSlotInput>())
                .Select(s => new PlayoffScheduleSlot(s.Round, s.Order, s.ScheduledDateTime, s.Venue))
                .ToList();

            tournament.SetPlayoffSchedule(slots);

            _logger.LogInformation(
                "Updating playoff schedule for tournament {TournamentId} to {SlotCount} slot(s).",
                tournament.Id,
                slots.Count);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTournamentDto dto = FloorballTournamentMapper.ToDto(tournament);
            return Result<FloorballTournamentDto>.Success(dto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid playoff schedule for tournament {TournamentId}", request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while updating playoff schedule for tournament {TournamentId}", request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while updating playoff schedule for tournament {TournamentId}", request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure(
                "A database error occurred while updating the tournament playoff schedule.",
                ex.Flatten());
        }
    }
}
