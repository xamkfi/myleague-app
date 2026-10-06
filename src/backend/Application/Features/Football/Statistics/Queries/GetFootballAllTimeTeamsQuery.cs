using Application.Common;
using Application.Features.Common.Shared.DTOs;
using Domain.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Football teams that have public all-time player statistics.
/// </summary>
public record GetFootballAllTimeTeamsQuery(
    TeamCategory TeamCategory = TeamCategory.Adult,
    AllTimeCompetitionFilter CompetitionType = AllTimeCompetitionFilter.Season
) : IRequest<Result<List<AllTimeTeamOptionDto>>>;
