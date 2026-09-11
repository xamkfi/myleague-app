using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command: DeactivateHockeySeason.
/// </summary>
public record DeactivateHockeySeasonCommand(Guid SeasonId) : IRequest<Result<HockeySeasonDto>>;
