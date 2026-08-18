using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: RemoveHockeyTournamentGroup.
/// </summary>
public record RemoveHockeyTournamentGroupCommand(
    Guid TournamentId,
    Guid GroupId) : IRequest<Result<HockeyTournamentDto>>;
