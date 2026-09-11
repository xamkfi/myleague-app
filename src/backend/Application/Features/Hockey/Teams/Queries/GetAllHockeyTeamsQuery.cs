using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Teams.Queries;

/// <summary>
/// Query to retrieve all hockey teams.
/// </summary>
public record GetAllHockeyTeamsQuery(TeamCategory? TeamCategory = null)
    : IRequest<Result<IEnumerable<HockeyTeamDto>>>;
