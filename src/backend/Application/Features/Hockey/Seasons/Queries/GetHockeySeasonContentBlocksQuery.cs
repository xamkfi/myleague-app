using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Queries;

public record GetHockeySeasonContentBlocksQuery(Guid SeasonId)
    : IRequest<Result<HockeySeasonContentBlocksDto>>;
