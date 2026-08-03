using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command: RemoveTeamFromHockeySeason.
/// </summary>
public record RemoveTeamFromHockeySeasonCommand(
    Guid SeasonId,
    Guid TeamId) : IRequest<Result<HockeySeasonDto>>;
