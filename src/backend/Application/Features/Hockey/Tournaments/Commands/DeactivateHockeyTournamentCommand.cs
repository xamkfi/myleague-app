using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: DeactivateHockeyTournament.
/// </summary>
public record DeactivateHockeyTournamentCommand(
    Guid TournamentId) : IRequest<Result<HockeyTournamentDto>>;
