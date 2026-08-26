using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: UpdateHockeyTournament.
/// </summary>
public record UpdateHockeyTournamentCommand(
    Guid TournamentId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? Venue,
    string? ContentHtml,
    TeamCategory TeamCategory) : IRequest<Result<HockeyTournamentDto>>;
