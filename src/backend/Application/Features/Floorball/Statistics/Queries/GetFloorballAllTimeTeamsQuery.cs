using Application.Common;
using Application.Features.Common.Shared.DTOs;
using Domain.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Floorball.Statistics.Queries;

/// <summary>
/// Floorball teams that have public all-time player statistics.
/// </summary>
public record GetFloorballAllTimeTeamsQuery(
    TeamCategory TeamCategory = TeamCategory.Adult,
    AllTimeCompetitionFilter CompetitionType = AllTimeCompetitionFilter.Season
) : IRequest<Result<List<AllTimeTeamOptionDto>>>;
