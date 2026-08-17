using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command: PublishHockeySeason.
/// </summary>
public record PublishHockeySeasonCommand(Guid SeasonId) : IRequest<Result<HockeySeasonDto>>;
