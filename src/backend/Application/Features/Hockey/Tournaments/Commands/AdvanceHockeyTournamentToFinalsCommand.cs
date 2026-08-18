using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: AdvanceHockeyTournamentToFinals.
/// </summary>
public record AdvanceHockeyTournamentToFinalsCommand(
    Guid TournamentId) : IRequest<Result<HockeyTournamentDto>>;
