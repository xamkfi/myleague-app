using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Command to add a team to a tournament group
/// </summary>
public record AddFloorballTeamToTournamentGroupCommand(
    Guid CompetitionId,
    Guid GroupId,
    Guid TeamId,
    Domain.Enums.Common.RosterEnrollmentMode RosterMode = Domain.Enums.Common.RosterEnrollmentMode.CopyLatest) : IRequest<Result<FloorballTournamentDto>>;
