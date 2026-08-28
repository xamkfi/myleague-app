using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command to create a group (lohko) within a hockey tournament.
/// </summary>
/// <param name="TournamentId">Tournament id</param>
/// <param name="Name">Group display name</param>
public record CreateHockeyTournamentGroupCommand(
    Guid TournamentId,
    string Name) : IRequest<Result<HockeyTournamentDto>>;
