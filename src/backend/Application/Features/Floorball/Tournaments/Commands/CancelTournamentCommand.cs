using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Command for cancelling a tournament (Draft/GroupStage/PlayoffStage -> Cancelled)
/// </summary>
public record CancelTournamentCommand(
    Guid CompetitionId) : IRequest<Result<FloorballTournamentDto>>;
