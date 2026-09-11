using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command: ActivateHockeySeason.
/// </summary>
public record ActivateHockeySeasonCommand(Guid SeasonId) : IRequest<Result<HockeySeasonDto>>;
