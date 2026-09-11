using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// Replaces the tournament's pre-defined playoff schedule slots in one shot. Passing an empty
/// list clears the schedule entirely. Used by the admin "Playoff schedule" editor so admins can
/// plan kickoffs/venues ahead of the bracket being generated, without having to round-trip every
/// other tournament field through the generic PUT endpoint.
/// </summary>
public record UpdateTournamentPlayoffScheduleCommand(
    Guid CompetitionId,
    IReadOnlyList<FootballPlayoffScheduleSlotInput> Slots) : IRequest<Result<FootballTournamentDto>>;
