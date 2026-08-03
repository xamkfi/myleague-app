using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Queries;

/// <summary>
/// Query to retrieve active hockey seasons.
/// </summary>
public record GetActiveHockeySeasonsQuery() : IRequest<Result<IEnumerable<HockeySeasonDto>>>;
