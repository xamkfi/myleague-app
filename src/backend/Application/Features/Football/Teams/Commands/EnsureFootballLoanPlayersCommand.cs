using Application.Common;
using Application.Features.Football.Teams.DTOs;
using MediatR;

namespace Application.Features.Football.Teams.Commands;

/// <summary>
/// Ensures the team has at least <paramref name="Count"/> loan players and a roster row
/// for <paramref name="CompetitionId"/> for each of the first <paramref name="Count"/>.
/// </summary>
public record EnsureFootballLoanPlayersCommand(
    Guid TeamId,
    Guid? CompetitionId,
    int Count) : IRequest<Result<IReadOnlyList<FootballLoanPlayerDto>>>;
