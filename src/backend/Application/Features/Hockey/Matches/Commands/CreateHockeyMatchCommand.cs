using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Creates a hockey match (standalone or within a competition).
/// </summary>
public record CreateHockeyMatchCommand(
    DateTime ScheduledStartTime,
    HockeyMatchType MatchType,
    Guid? CompetitionId = null,
    Guid? CompetitionDivisionId = null,
    Guid? TournamentGroupId = null,
    Guid? PlayoffSeriesId = null,
    string? Venue = null) : IRequest<Result<HockeyMatchDto>>;
