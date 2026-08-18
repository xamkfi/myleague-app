using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command: SetHockeySeasonChampion.
/// </summary>
public record SetHockeySeasonChampionCommand(
    Guid SeasonId,
    Guid ChampionCompetitionTeamId) : IRequest<Result<HockeySeasonDto>>;
