using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// Command to add a group to a tournament
/// </summary>
public record AddGroupToTournamentCommand(
    Guid CompetitionId,
    string GroupName) : IRequest<Result<FootballTournamentDto>>;
