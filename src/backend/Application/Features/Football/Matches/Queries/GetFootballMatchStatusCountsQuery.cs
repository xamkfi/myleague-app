using Application.Common;
using Application.Features.Common.Shared.DTOs;
using Domain.Common;
using Domain.Enums.Football;
using MediatR;

namespace Application.Features.Football.Matches.Queries;

/// <summary>
/// Counts football matches per status for the admin match list tabs.
/// </summary>
public record GetFootballMatchStatusCountsQuery(
    Guid? CompetitionId = null,
    string? SearchQuery = null,
    FootballCompetitionType? CompetitionType = null,
    bool IncludeDrafts = false) : IRequest<Result<MatchStatusCountsDto>>;
