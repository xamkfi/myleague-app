using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command for creating a hockey season.
/// </summary>
/// <param name="Name">Name of the season</param>
/// <param name="StartDate">Season start date</param>
/// <param name="EndDate">Season end date</param>
/// <param name="SeasonCode">Optional short season code</param>
public record CreateHockeySeasonCommand(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? SeasonCode = null) : IRequest<Result<HockeySeasonDto>>;
