using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Command for opening tournament registration (Draft -> Registration)
/// </summary>
public record OpenTournamentRegistrationCommand(
    Guid CompetitionId) : IRequest<Result<FloorballTournamentDto>>;
