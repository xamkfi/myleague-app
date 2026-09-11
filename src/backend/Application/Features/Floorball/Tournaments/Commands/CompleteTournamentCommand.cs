using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Command for completing a tournament (GroupStage/PlayoffStage -> Completed)
/// </summary>
public record CompleteTournamentCommand(
    Guid CompetitionId) : IRequest<Result<FloorballTournamentDto>>;
