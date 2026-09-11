using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: ActivateHockeyTournament.
/// </summary>
public record ActivateHockeyTournamentCommand(
    Guid TournamentId) : IRequest<Result<HockeyTournamentDto>>;
