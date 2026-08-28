using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Command: CancelHockeySeason.
/// </summary>
public record CancelHockeySeasonCommand(Guid SeasonId) : IRequest<Result<HockeySeasonDto>>;
