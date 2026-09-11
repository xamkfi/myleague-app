using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using MediatR;

namespace Application.Features.Football.Seasons.Queries;

public record GetFootballSeasonContentBlocksQuery(Guid SeasonId)
    : IRequest<Result<FootballSeasonContentBlocksDto>>;
