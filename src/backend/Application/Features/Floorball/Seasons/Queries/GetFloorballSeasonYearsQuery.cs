using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Floorball.Seasons.Queries;

/// <summary>
/// Query for available floorball season years (newest first), optionally limited to one audience.
/// </summary>
public record GetFloorballSeasonYearsQuery(TeamCategory? TeamCategory = null, bool IncludeDrafts = false)
    : IRequest<Result<IEnumerable<FloorballSeasonYearDto>>>;
