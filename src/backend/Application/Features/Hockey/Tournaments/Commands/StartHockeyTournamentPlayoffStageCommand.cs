using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: StartHockeyTournamentPlayoffStage.
/// </summary>
public record StartHockeyTournamentPlayoffStageCommand(
    Guid TournamentId) : IRequest<Result<HockeyTournamentDto>>;
