using Application.Common;
using Application.Features.Common.Shared.DTOs;
using Domain.Common;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Features.Floorball.Matches.Queries;

/// <summary>
/// Counts floorball matches per status for the admin match list tabs.
/// </summary>
public record GetFloorballMatchStatusCountsQuery(
    Guid? CompetitionId = null,
    string? SearchQuery = null,
    FloorballCompetitionType? CompetitionType = null,
    bool IncludeDrafts = false) : IRequest<Result<MatchStatusCountsDto>>;
