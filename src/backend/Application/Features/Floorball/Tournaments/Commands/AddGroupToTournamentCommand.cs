using Application.Features.Floorball.Tournaments.DTOs;
using Application.Common;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

public record AddGroupToTournamentCommand(
    Guid TournamentId,
    string Name,
    string Phase = "GroupStage",
    int SortOrder = 0) : IRequest<Result<FloorballTournamentGroupDto>>;
