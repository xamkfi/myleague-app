using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Teams.Queries;

/// <summary>
/// Query to retrieve hockey teams for a club.
/// </summary>
public record GetHockeyTeamsByClubQuery(Guid ClubId, TeamCategory? TeamCategory = null)
    : IRequest<Result<IEnumerable<HockeyTeamDto>>>;
