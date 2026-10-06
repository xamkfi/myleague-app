using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Features.Football.Matches.Queries;
using Domain.Common;
using Domain.Entities.Football.Matches;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

/// <summary>
/// Loads upcoming scheduled matches for several teams in one query.
/// </summary>
public class GetFootballUpcomingMatchesForTeamsHandler
    : IRequestHandler<GetFootballUpcomingMatchesForTeamsQuery, Result<IEnumerable<FootballMatchDto>>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly ILogger<GetFootballUpcomingMatchesForTeamsHandler> _logger;

    public GetFootballUpcomingMatchesForTeamsHandler(
        IFootballMatchRepository matchRepository,
        ILogger<GetFootballUpcomingMatchesForTeamsHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<FootballMatchDto>>> Handle(
        GetFootballUpcomingMatchesForTeamsQuery request,
        CancellationToken cancellationToken)
    {
        List<Guid> teamIds = request.TeamIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (teamIds.Count == 0)
        {
            return Result<IEnumerable<FootballMatchDto>>.Success(Array.Empty<FootballMatchDto>());
        }

        try
        {
            PagedResult<FootballMatch> matches = await _matchRepository.GetPagedAsync(
                page: 1,
                pageSize: GetFootballUpcomingMatchesForTeamsQuery.MaxMatchesPerTeam * teamIds.Count,
                startDate: request.From,
                status: FootballMatchStatus.Scheduled,
                sortOrder: "asc",
                excludeDraftCompetitions: true,
                teamIds: teamIds,
                cancellationToken: cancellationToken);

            return Result<IEnumerable<FootballMatchDto>>.Success(FootballMatchMapper.ToDtos(matches.Items).ToList());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to load upcoming football matches for {TeamCount} teams", teamIds.Count);
            return Result<IEnumerable<FootballMatchDto>>.Failure("Error occurred while retrieving upcoming football matches");
        }
    }
}
