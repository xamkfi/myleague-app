using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command to replace the playoff schedule slots on a hockey season.
/// </summary>
public record SetHockeySeasonPlayoffScheduleCommand(
    Guid SeasonId,
    IReadOnlyList<HockeyPlayoffScheduleSlotDto> Slots) : IRequest<Result<HockeySeasonDto>>;
