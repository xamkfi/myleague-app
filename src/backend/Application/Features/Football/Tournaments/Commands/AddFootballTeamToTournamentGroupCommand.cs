using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// Command to add a team to a tournament group
/// </summary>
public record AddFootballTeamToTournamentGroupCommand(
    Guid CompetitionId,
    Guid GroupId,
    Guid TeamId,
    Domain.Enums.Common.RosterEnrollmentMode RosterMode = Domain.Enums.Common.RosterEnrollmentMode.CopyLatest) : IRequest<Result<FootballTournamentDto>>;
