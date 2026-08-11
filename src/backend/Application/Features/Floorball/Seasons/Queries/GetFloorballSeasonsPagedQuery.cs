using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Floorball.Seasons.Queries;

/// <summary>
/// Paginated public listing of floorball seasons, optionally filtered by season year.
/// </summary>
public record GetFloorballSeasonsPagedQuery(
    int Page,
    int PageSize,
    string? SeasonYear
) : IRequest<Result<PagedResult<FloorballSeasonSummaryDto>>>;
