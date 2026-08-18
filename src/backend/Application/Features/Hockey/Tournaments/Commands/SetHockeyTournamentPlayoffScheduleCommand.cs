using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command to replace the playoff schedule slots on a hockey tournament.
/// </summary>
public record SetHockeyTournamentPlayoffScheduleCommand(
    Guid TournamentId,
    IReadOnlyList<HockeyPlayoffScheduleSlotDto> Slots) : IRequest<Result<HockeyTournamentDto>>;
