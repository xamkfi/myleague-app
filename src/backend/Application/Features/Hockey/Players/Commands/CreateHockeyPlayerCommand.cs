using Application.Common;
using Application.Features.Hockey.Players.DTOs;
using Domain.Enums.Hockey.Teams;
using MediatR;

namespace Application.Features.Hockey.Players.Commands;

/// <summary>
/// Command for creating a hockey player profile.
/// </summary>
public record CreateHockeyPlayerCommand(
    Guid PersonId,
    HockeyPosition PrimaryPosition,
    HockeyShoots Shoots = HockeyShoots.Unknown,
    HockeyCatches? Catches = null,
    string? LicenseNumber = null) : IRequest<Result<HockeyPlayerDto>>;
