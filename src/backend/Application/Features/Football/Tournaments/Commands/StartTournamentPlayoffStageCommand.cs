using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// Command for starting the tournament playoff stage (GroupStage -> PlayoffStage)
/// </summary>
public record StartTournamentPlayoffStageCommand(
    Guid CompetitionId) : IRequest<Result<FootballTournamentDto>>;
