using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Seasons.Queries;

/// <summary>
/// Query for available floorball season years (newest first).
/// </summary>
public record GetFloorballSeasonYearsQuery() : IRequest<Result<IEnumerable<FloorballSeasonYearDto>>>;
