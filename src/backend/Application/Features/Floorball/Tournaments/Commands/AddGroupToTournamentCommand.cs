using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Command to add a group to a tournament
/// </summary>
public record AddGroupToTournamentCommand(
    Guid CompetitionId,
    string GroupName) : IRequest<Result<FloorballTournamentDto>>;
