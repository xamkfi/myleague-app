using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// Command for cancelling a tournament (Draft/GroupStage/PlayoffStage -> Cancelled)
/// </summary>
public record CancelFootballTournamentCommand(
    Guid CompetitionId) : IRequest<Result<FootballTournamentDto>>;
