using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Creates a period score row for a hockey match.
/// </summary>
public record AddHockeyPeriodScoreCommand(
    Guid MatchId,
    int PeriodNumber,
    HockeyPeriodType PeriodType) : IRequest<Result<HockeyMatchDto>>;
