using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command to soft-remove a competition team from a season division.
/// </summary>
public record RemoveTeamFromHockeySeasonDivisionCommand(
    Guid SeasonId,
    Guid CompetitionDivisionId,
    Guid CompetitionTeamId) : IRequest<Result<HockeySeasonDto>>;
