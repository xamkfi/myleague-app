using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Queries;

/// <summary>
/// Query for retrieving all hockey seasons.
/// </summary>
public record GetAllHockeySeasonsQuery() : IRequest<Result<IEnumerable<HockeySeasonDto>>>;
