using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Seasons.Queries;

public record GetFloorballSeasonContentBlocksQuery(Guid SeasonId)
    : IRequest<Result<FloorballSeasonContentBlocksDto>>;
