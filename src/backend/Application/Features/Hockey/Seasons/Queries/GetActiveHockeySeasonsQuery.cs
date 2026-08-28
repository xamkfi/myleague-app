using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Seasons.Queries;

/// <summary>
/// Query to retrieve active hockey seasons.
/// </summary>
public record GetActiveHockeySeasonsQuery(TeamCategory? TeamCategory = null)
    : IRequest<Result<IEnumerable<HockeySeasonDto>>>;
