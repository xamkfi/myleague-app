using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// Command for completing a tournament (GroupStage/PlayoffStage -> Completed)
/// </summary>
public record CompleteTournamentCommand(
    Guid CompetitionId) : IRequest<Result<FootballTournamentDto>>;
