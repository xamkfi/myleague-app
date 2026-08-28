using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Seasons.Queries;

/// <summary>
/// Query for retrieving all hockey seasons.
/// </summary>
public record GetAllHockeySeasonsQuery(TeamCategory? TeamCategory = null)
    : IRequest<Result<IEnumerable<HockeySeasonDto>>>;
