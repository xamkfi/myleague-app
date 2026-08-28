using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command: UpdateHockeySeason.
/// </summary>
public record UpdateHockeySeasonCommand(
    Guid SeasonId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? SeasonCode,
    TeamCategory TeamCategory) : IRequest<Result<HockeySeasonDto>>;
