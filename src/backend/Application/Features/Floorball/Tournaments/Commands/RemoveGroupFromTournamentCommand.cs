using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Command to remove a group from a tournament
/// </summary>
public record RemoveGroupFromTournamentCommand(
    Guid CompetitionId,
    Guid GroupId) : IRequest<Result<FloorballTournamentDto>>;
