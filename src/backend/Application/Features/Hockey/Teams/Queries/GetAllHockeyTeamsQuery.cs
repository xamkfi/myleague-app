using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using MediatR;

namespace Application.Features.Hockey.Teams.Queries;

/// <summary>
/// Query to retrieve all hockey teams.
/// </summary>
public record GetAllHockeyTeamsQuery() : IRequest<Result<IEnumerable<HockeyTeamDto>>>;
