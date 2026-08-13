using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// Command to remove a group from a tournament
/// </summary>
public record RemoveGroupFromTournamentCommand(
    Guid CompetitionId,
    Guid GroupId) : IRequest<Result<FootballTournamentDto>>;
