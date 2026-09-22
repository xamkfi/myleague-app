using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Football.Seasons.Queries;

/// <summary>
/// Query for available football season years (newest first), optionally limited to one audience.
/// </summary>
public record GetFootballSeasonYearsQuery(TeamCategory? TeamCategory = null)
    : IRequest<Result<IEnumerable<FootballSeasonYearDto>>>;
