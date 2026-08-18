using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command: CompleteHockeySeason.
/// </summary>
public record CompleteHockeySeasonCommand(Guid SeasonId) : IRequest<Result<HockeySeasonDto>>;
