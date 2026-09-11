using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using MediatR;

namespace Application.Features.Football.Seasons.Queries;

public record GetFootballSeasonContentBlocksByYearQuery(string? SeasonYear)
    : IRequest<Result<FootballSeasonContentBlocksDto>>;
