using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Records a video review event on a hockey match.
/// </summary>
public record RecordHockeyVideoReviewCommand(
    Guid MatchId,
    int PeriodNumber,
    int TimeInSeconds,
    HockeyVideoReviewType ReviewType,
    HockeyReviewDecision OriginalDecision,
    HockeyReviewDecision FinalDecision,
    bool IsCoachChallenge,
    bool WasSuccessful,
    Guid? RequestedByMatchTeamId = null,
    string? Description = null) : IRequest<Result<HockeyMatchDto>>;
