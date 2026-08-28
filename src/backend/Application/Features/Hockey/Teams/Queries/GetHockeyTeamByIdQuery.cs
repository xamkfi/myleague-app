using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using MediatR;

namespace Application.Features.Hockey.Teams.Queries;

/// <summary>
/// Query for retrieving a hockey team by id.
/// </summary>
/// <param name="Id">Team id</param>
public record GetHockeyTeamByIdQuery(Guid Id) : IRequest<Result<HockeyTeamDto>>;
