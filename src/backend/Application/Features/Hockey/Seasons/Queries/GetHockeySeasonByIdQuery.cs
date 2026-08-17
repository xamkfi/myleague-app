using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Queries;

/// <summary>
/// Query for retrieving a hockey season by id.
/// </summary>
/// <param name="Id">Season id</param>
public record GetHockeySeasonByIdQuery(Guid Id) : IRequest<Result<HockeySeasonDto>>;
