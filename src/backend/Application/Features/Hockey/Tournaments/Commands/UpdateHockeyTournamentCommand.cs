using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
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
    string? ContentHtml) : IRequest<Result<HockeyTournamentDto>>;
