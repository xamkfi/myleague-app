using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using Domain.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Football.Seasons.Queries;

public record GetFootballSeasonsPagedQuery(
    int Page,
    int PageSize,
    string? SeasonYear,
    TeamCategory? TeamCategory) : IRequest<Result<PagedResult<FootballSeasonSummaryDto>>>;
