using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: OpenHockeyTournamentRegistration.
/// </summary>
public record OpenHockeyTournamentRegistrationCommand(
    Guid TournamentId) : IRequest<Result<HockeyTournamentDto>>;
