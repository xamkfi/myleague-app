using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command: OpenHockeySeasonRegistration.
/// </summary>
public record OpenHockeySeasonRegistrationCommand(Guid SeasonId) : IRequest<Result<HockeySeasonDto>>;
