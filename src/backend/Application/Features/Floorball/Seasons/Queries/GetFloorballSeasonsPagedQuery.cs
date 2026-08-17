using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Domain.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Floorball.Seasons.Queries;

/// <summary>
/// Paginated public listing of floorball seasons, optionally filtered by season year and audience.
/// </summary>
public record GetFloorballSeasonsPagedQuery(
    int Page,
    int PageSize,
    string? SeasonYear,
    TeamCategory? TeamCategory = null
) : IRequest<Result<PagedResult<FloorballSeasonSummaryDto>>>;
