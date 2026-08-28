using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Domain.Common;
using Domain.Enums.Common;
using Domain.Enums.Football;
using MediatR;

namespace Application.Features.Football.Matches.Queries;

public record GetAllFootballMatchesQuery(
    int Page = 1,
    int PageSize = 0,
    Guid? CompetitionId = null,
    Guid? TeamId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string SortOrder = "desc",
    string? SearchQuery = null,
    FootballMatchStatus? Status = null,
    Guid? TournamentGroupId = null,
    FootballCompetitionType? CompetitionType = null,
    TeamCategory? TeamCategory = null) : IRequest<Result<PagedResult<FootballMatchDto>>>
{
    public const string ResourceKey = "FootballMatches";
}
