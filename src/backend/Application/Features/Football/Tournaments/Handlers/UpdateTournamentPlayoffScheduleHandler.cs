using Application.Common;
using Application.Features.Football.Tournaments.Commands;
using Application.Features.Football.Tournaments.DTOs;
using Application.Features.Football.Tournaments.Mappings;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Entities.Football.Statistics;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Tournaments.Handlers;

/// <summary>
/// Replaces the tournament's <see cref="FootballTournament.PlayoffSchedule"/> in a single
/// targeted update. The domain entity enforces the lifecycle rule (no edits once the bracket
/// has been generated) — see <see cref="FootballTournament.SetPlayoffSchedule"/>.
/// </summary>
public class UpdateTournamentPlayoffScheduleHandler : IRequestHandler<UpdateTournamentPlayoffScheduleCommand, Result<FootballTournamentDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTournamentPlayoffScheduleHandler> _logger;

    public UpdateTournamentPlayoffScheduleHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<UpdateTournamentPlayoffScheduleHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTournamentDto>> Handle(UpdateTournamentPlayoffScheduleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Need the tracked entity + groups so the response DTO renders fully (group standings
            // etc.) the moment the admin saves.
            FootballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId, cancellationToken);
            if (tournament == null)
            {
                _logger.LogWarning("Attempt to update playoff schedule for non-existent tournament {TournamentId}", request.CompetitionId);
                return Result<FootballTournamentDto>.NotFound("FootballTournament", request.CompetitionId);
            }

            // SetPlayoffSchedule itself rejects duplicate (round, order) pairs and throws when the
            // tournament has already advanced past the planning window. Converting the inputs to
            // value objects first lets the domain run its own validation before we touch EF state.
            List<FootballPlayoffScheduleSlot> slots = (request.Slots ?? Array.Empty<FootballPlayoffScheduleSlotInput>())
                .Select(s => new FootballPlayoffScheduleSlot(s.Round, s.Order, s.ScheduledDateTime, s.Venue))
                .ToList();

            tournament.SetPlayoffSchedule(slots);

            _logger.LogInformation(
                "Updating playoff schedule for tournament {TournamentId} to {SlotCount} slot(s).",
                tournament.Id,
                slots.Count);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballTournamentDto dto = FootballTournamentMapper.ToDto(tournament);
            return Result<FootballTournamentDto>.Success(dto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid playoff schedule for tournament {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while updating playoff schedule for tournament {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while updating playoff schedule for tournament {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(
                "A database error occurred while updating the tournament playoff schedule.",
                ex.Flatten());
        }
    }
}
