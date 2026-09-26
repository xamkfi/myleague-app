using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Teams.Mappings;
using Application.Features.Hockey.Teams.Queries;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Teams.Handlers;

/// <summary>
/// Handles paginated hockey team listing without roster graphs.
/// </summary>
public class GetPagedHockeyTeamsHandler
    : IRequestHandler<GetPagedHockeyTeamsQuery, Result<PagedResult<HockeyTeamDto>>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IPaginationService _paginationService;
    private readonly ILogger<GetPagedHockeyTeamsHandler> _logger;

    public GetPagedHockeyTeamsHandler(
        IHockeyTeamRepository teamRepository,
        IPaginationService paginationService,
        ILogger<GetPagedHockeyTeamsHandler> logger)
    {
        _teamRepository = teamRepository;
        _paginationService = paginationService;
        _logger = logger;
    }

    public async Task<Result<PagedResult<HockeyTeamDto>>> Handle(
        GetPagedHockeyTeamsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            int pageSize = _paginationService.ResolvePageSize(
                GetPagedHockeyTeamsQuery.ResourceKey,
                request.PageSize);

            PagedResult<HockeyTeam> pagedTeams = await _teamRepository.GetPagedAsync(
                request.Page,
                pageSize,
                request.SearchTerm ?? string.Empty,
                request.ClubId,
                request.TeamCategories,
                request.CompetitionId,
                request.CompetitionDivisionId,
                request.DivisionId,
                cancellationToken);

            Dictionary<Guid, int> rosterCounts = new Dictionary<Guid, int>();
            if (request.CompetitionId is Guid competitionId)
            {
                IReadOnlyDictionary<Guid, int> counts = await _teamRepository.CountCompetitionRostersAsync(
                    pagedTeams.Items.Select(team => team.Id).ToArray(),
                    competitionId,
                    cancellationToken);
                foreach (KeyValuePair<Guid, int> entry in counts)
                {
                    rosterCounts[entry.Key] = entry.Value;
                }
            }

            IReadOnlyCollection<Guid> teamsWithRoster = await _teamRepository.GetTeamIdsWithOpenRosterAsync(
                pagedTeams.Items.Select(team => team.Id).ToArray(),
                cancellationToken);
            HashSet<Guid> openRosterIds = teamsWithRoster.ToHashSet();

            IReadOnlyList<HockeyTeamDto> items = pagedTeams.Items
                .Select(team =>
                {
                    HockeyTeamDto dto = HockeyTeamMapper.ToDto(team) with
                    {
                        HasActiveRoster = openRosterIds.Contains(team.Id),
                    };
                    if (request.CompetitionId is null)
                    {
                        return dto;
                    }

                    int count = rosterCounts.TryGetValue(team.Id, out int rosterCount) ? rosterCount : 0;
                    return dto with { CompetitionRosterCount = count };
                })
                .ToList();
            return Result<PagedResult<HockeyTeamDto>>.Success(
                PagedResult.Create(items, pagedTeams.TotalCount, pagedTeams.Page, pagedTeams.PageSize));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Paged hockey team retrieval was cancelled");
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid paged hockey team query");
            return Result<PagedResult<HockeyTeamDto>>.Failure(
                "An error occurred while retrieving hockey teams.",
                ex.Flatten());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get paged hockey teams");
            return Result<PagedResult<HockeyTeamDto>>.Failure(
                "An error occurred while retrieving hockey teams.",
                ex.Flatten());
        }
    }
}
