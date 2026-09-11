using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Sets and confirms the match-day roster for one match side.
/// </summary>
public record ConfirmHockeyMatchRosterCommand(
    Guid MatchId,
    Guid MatchTeamId,
    IReadOnlyList<Guid> TeamPlayerIds,
    Guid? ConfirmedByUserId = null,
    HockeyPlayerSelectionSource Source = HockeyPlayerSelectionSource.Manual) : IRequest<Result<HockeyMatchDto>>;
