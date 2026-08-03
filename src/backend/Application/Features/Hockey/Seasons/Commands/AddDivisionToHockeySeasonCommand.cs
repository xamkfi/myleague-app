using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command to add a Common Division link to a hockey season.
/// </summary>
public record AddDivisionToHockeySeasonCommand(
    Guid SeasonId,
    Guid DivisionId,
    string Name,
    int SortOrder) : IRequest<Result<HockeySeasonDto>>;
