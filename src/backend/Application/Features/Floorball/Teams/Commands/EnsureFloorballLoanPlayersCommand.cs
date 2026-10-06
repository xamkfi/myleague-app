using Application.Common;
using Application.Features.Floorball.Teams.DTOs;
using MediatR;

namespace Application.Features.Floorball.Teams.Commands;

/// <summary>
/// Ensures the team has at least <paramref name="Count"/> loan players and a roster row
/// for <paramref name="CompetitionId"/> for each of the first <paramref name="Count"/>.
/// </summary>
public record EnsureFloorballLoanPlayersCommand(
    Guid TeamId,
    Guid? CompetitionId,
    int Count) : IRequest<Result<IReadOnlyList<FloorballLoanPlayerDto>>>;
