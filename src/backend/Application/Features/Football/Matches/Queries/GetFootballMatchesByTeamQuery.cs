using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Football.Matches.Queries;

public record GetFootballMatchesByTeamQuery(
    int Page,
    int PageSize,
    Guid TeamId,
    DateTime? StartDate,
    DateTime? EndDate) : IRequest<Result<PagedResult<FootballMatchDto>>>
{
    public const string ResourceKey = "FootballMatches";
}
