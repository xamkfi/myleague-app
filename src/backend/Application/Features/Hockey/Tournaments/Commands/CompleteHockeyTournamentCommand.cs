using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: CompleteHockeyTournament.
/// </summary>
public record CompleteHockeyTournamentCommand(
    Guid TournamentId) : IRequest<Result<HockeyTournamentDto>>;
