using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Replaces the tournament's pre-defined playoff schedule slots in one shot. Passing an empty
/// list clears the schedule entirely. Used by the admin "Playoff schedule" editor so admins can
/// plan kickoffs/venues ahead of the bracket being generated, without having to round-trip every
/// other tournament field through the generic PUT endpoint.
/// </summary>
public record UpdateTournamentPlayoffScheduleCommand(
    Guid CompetitionId,
    IReadOnlyList<PlayoffScheduleSlotInput> Slots) : IRequest<Result<FloorballTournamentDto>>;
