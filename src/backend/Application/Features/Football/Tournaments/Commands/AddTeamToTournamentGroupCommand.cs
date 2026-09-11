using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// Command to add a team to a tournament group
/// </summary>
public record AddTeamToTournamentGroupCommand(
    Guid CompetitionId,
    Guid GroupId,
    Guid TeamId) : IRequest<Result<FootballTournamentDto>>;
