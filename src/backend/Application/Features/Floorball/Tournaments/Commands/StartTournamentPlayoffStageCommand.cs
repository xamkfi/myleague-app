using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Command for starting the tournament playoff stage (GroupStage -> PlayoffStage)
/// </summary>
public record StartTournamentPlayoffStageCommand(
    Guid CompetitionId) : IRequest<Result<FloorballTournamentDto>>;
