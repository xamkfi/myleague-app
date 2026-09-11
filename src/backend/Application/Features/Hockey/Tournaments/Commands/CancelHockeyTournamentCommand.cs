using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: CancelHockeyTournament.
/// </summary>
public record CancelHockeyTournamentCommand(
    Guid TournamentId) : IRequest<Result<HockeyTournamentDto>>;
