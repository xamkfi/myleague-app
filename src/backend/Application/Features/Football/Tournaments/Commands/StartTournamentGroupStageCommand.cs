using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// Command for starting the tournament group stage (Draft -> GroupStage)
/// </summary>
public record StartTournamentGroupStageCommand(
    Guid CompetitionId) : IRequest<Result<FootballTournamentDto>>;
