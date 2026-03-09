using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using Domain.Enums.Floorball.Tournament;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Command for creating a match within a tournament
/// </summary>
public record CreateFloorballTournamentMatchCommand(
    Guid TournamentId,
    Guid HomeTeamId,
    Guid AwayTeamId,
    DateTime ScheduledDateTime,
    string? Venue,
    Guid? GroupId = null,
    string? TournamentRound = null,
    Guid? RefereeId = null) : IRequest<Result<FloorballMatchDto>>;
