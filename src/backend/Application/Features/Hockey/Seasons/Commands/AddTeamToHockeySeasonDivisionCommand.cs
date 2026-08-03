using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command to place a competition team into a season division.
/// </summary>
public record AddTeamToHockeySeasonDivisionCommand(
    Guid SeasonId,
    Guid CompetitionDivisionId,
    Guid CompetitionTeamId,
    int? Seed = null) : IRequest<Result<HockeySeasonDto>>;
