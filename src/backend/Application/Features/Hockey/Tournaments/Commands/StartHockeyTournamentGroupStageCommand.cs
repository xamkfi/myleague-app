using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: StartHockeyTournamentGroupStage.
/// </summary>
public record StartHockeyTournamentGroupStageCommand(
    Guid TournamentId) : IRequest<Result<HockeyTournamentDto>>;
