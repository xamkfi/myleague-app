using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command to soft-remove a competition division from a hockey season.
/// </summary>
public record RemoveDivisionFromHockeySeasonCommand(
    Guid SeasonId,
    Guid CompetitionDivisionId) : IRequest<Result<HockeySeasonDto>>;
