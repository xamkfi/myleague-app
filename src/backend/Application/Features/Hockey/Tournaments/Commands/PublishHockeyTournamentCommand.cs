using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: PublishHockeyTournament.
/// </summary>
public record PublishHockeyTournamentCommand(
    Guid TournamentId) : IRequest<Result<HockeyTournamentDto>>;
