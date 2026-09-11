using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Command for starting the tournament group stage (Draft -> GroupStage)
/// </summary>
public record StartTournamentGroupStageCommand(
    Guid CompetitionId) : IRequest<Result<FloorballTournamentDto>>;
