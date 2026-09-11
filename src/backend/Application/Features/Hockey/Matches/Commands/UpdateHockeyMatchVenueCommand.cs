using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Updates the venue of a hockey match.
/// </summary>
public record UpdateHockeyMatchVenueCommand(
    Guid MatchId,
    string? Venue) : IRequest<Result<HockeyMatchDto>>;
