using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: RemoveTeamFromHockeyTournament.
/// </summary>
public record RemoveTeamFromHockeyTournamentCommand(
    Guid TournamentId,
    Guid TeamId) : IRequest<Result<HockeyTournamentDto>>;
