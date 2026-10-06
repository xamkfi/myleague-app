using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Matches.Queries;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Matches;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Loads upcoming scheduled matches for several teams in one query.
/// </summary>
public class GetFloorballUpcomingMatchesForTeamsHandler
    : IRequestHandler<GetFloorballUpcomingMatchesForTeamsQuery, Result<IEnumerable<FloorballMatchDto>>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetFloorballUpcomingMatchesForTeamsHandler> _logger;

    public GetFloorballUpcomingMatchesForTeamsHandler(
        IFloorballMatchRepository matchRepository,
        IClubRepository clubRepository,
        ILogger<GetFloorballUpcomingMatchesForTeamsHandler> logger)
    {
        _matchRepository = matchRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<FloorballMatchDto>>> Handle(
        GetFloorballUpcomingMatchesForTeamsQuery request,
        CancellationToken cancellationToken)
    {
        List<Guid> teamIds = request.TeamIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (teamIds.Count == 0)
        {
            return Result<IEnumerable<FloorballMatchDto>>.Success(Array.Empty<FloorballMatchDto>());
        }

        try
        {
            PagedResult<FloorballMatch> matches = await _matchRepository.GetPagedAsync(
                page: 1,
                pageSize: GetFloorballUpcomingMatchesForTeamsQuery.MaxMatchesPerTeam * teamIds.Count,
                startDate: request.From,
                status: FloorballMatchStatus.Scheduled,
                sortOrder: "asc",
                excludeDraftCompetitions: true,
                teamIds: teamIds,
                cancellationToken: cancellationToken);

            List<FloorballMatch> items = matches.Items.ToList();
            List<Guid> clubIds = FloorballMatchMapper.CollectClubIds(items);
            Dictionary<Guid, Club> clubLookup = clubIds.Count == 0
                ? new Dictionary<Guid, Club>()
                : await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);

            return Result<IEnumerable<FloorballMatchDto>>.Success(FloorballMatchMapper.ToDtos(items, clubLookup).ToList());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to load upcoming floorball matches for {TeamCount} teams", teamIds.Count);
            return Result<IEnumerable<FloorballMatchDto>>.Failure("Error occurred while retrieving upcoming floorball matches");
        }
    }
}
