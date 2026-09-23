using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Application.Features.Hockey.Matches.Queries;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles listing matches for a competition.
/// </summary>
public class GetHockeyMatchesByCompetitionHandler
    : IRequestHandler<GetHockeyMatchesByCompetitionQuery, Result<IEnumerable<HockeyMatchDto>>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly ILogger<GetHockeyMatchesByCompetitionHandler> _logger;

    public GetHockeyMatchesByCompetitionHandler(
        IHockeyMatchRepository matchRepository,
        ILogger<GetHockeyMatchesByCompetitionHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<HockeyMatchDto>>> Handle(
        GetHockeyMatchesByCompetitionQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeyMatch> matches =
                await _matchRepository.GetByCompetitionIdAsync(request.CompetitionId);
            IEnumerable<HockeyMatch> visibleMatches = request.IncludeDrafts
                ? matches
                : matches.Where(match => match.Competition is null || PublicCompetitionVisibility.IsPublic(match.Competition));
            return Result<IEnumerable<HockeyMatchDto>>.Success(
                visibleMatches.Select(HockeyMatchMapper.ToDto).ToList());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Failed GetHockeyMatchesByCompetition for {CompetitionId}",
                request.CompetitionId);
            return Result<IEnumerable<HockeyMatchDto>>.Failure(
                "An error occurred while retrieving hockey matches.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Handles listing matches for a career team.
/// </summary>
public class GetHockeyMatchesByTeamHandler
    : IRequestHandler<GetHockeyMatchesByTeamQuery, Result<IEnumerable<HockeyMatchDto>>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly ILogger<GetHockeyMatchesByTeamHandler> _logger;

    public GetHockeyMatchesByTeamHandler(
        IHockeyMatchRepository matchRepository,
        ILogger<GetHockeyMatchesByTeamHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<HockeyMatchDto>>> Handle(
        GetHockeyMatchesByTeamQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeyMatch> matches =
                await _matchRepository.GetByTeamIdAsync(request.TeamId);
            IEnumerable<HockeyMatch> visibleMatches = matches.Where(match =>
                match.Competition is null || PublicCompetitionVisibility.IsPublic(match.Competition));
            return Result<IEnumerable<HockeyMatchDto>>.Success(
                visibleMatches.Select(HockeyMatchMapper.ToDto).ToList());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed GetHockeyMatchesByTeam for {TeamId}", request.TeamId);
            return Result<IEnumerable<HockeyMatchDto>>.Failure(
                "An error occurred while retrieving hockey matches.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Handles paginated hockey match listing without event or on-ice graphs.
/// </summary>
public class GetPagedHockeyMatchesHandler
    : IRequestHandler<GetPagedHockeyMatchesQuery, Result<PagedResult<HockeyMatchDto>>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IPaginationService _paginationService;
    private readonly ILogger<GetPagedHockeyMatchesHandler> _logger;

    public GetPagedHockeyMatchesHandler(
        IHockeyMatchRepository matchRepository,
        IPaginationService paginationService,
        ILogger<GetPagedHockeyMatchesHandler> logger)
    {
        _matchRepository = matchRepository;
        _paginationService = paginationService;
        _logger = logger;
    }

    public async Task<Result<PagedResult<HockeyMatchDto>>> Handle(
        GetPagedHockeyMatchesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            int pageSize = _paginationService.ResolvePageSize(
                GetPagedHockeyMatchesQuery.ResourceKey,
                request.PageSize);

            PagedResult<HockeyMatch> pagedMatches = await _matchRepository.GetPagedAsync(
                request.Page,
                pageSize,
                request.CompetitionId,
                request.TeamId,
                request.StartDate,
                request.EndDate,
                request.Status,
                request.SortOrder,
                request.SearchQuery,
                teamCategory: null,
                excludeDraftCompetitions: !request.IncludeDrafts,
                cancellationToken: cancellationToken);

            IReadOnlyList<HockeyMatchDto> items = pagedMatches.Items.Select(HockeyMatchMapper.ToDto).ToList();
            return Result<PagedResult<HockeyMatchDto>>.Success(
                PagedResult.Create(items, pagedMatches.TotalCount, pagedMatches.Page, pagedMatches.PageSize));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Paged hockey match retrieval was cancelled");
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid paged hockey match query");
            return Result<PagedResult<HockeyMatchDto>>.Failure(
                "An error occurred while retrieving hockey matches.",
                ex.Flatten());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get paged hockey matches");
            return Result<PagedResult<HockeyMatchDto>>.Failure(
                "An error occurred while retrieving hockey matches.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Handles the public hockey match list used by the event calendar.
/// </summary>
public class GetHockeyMatchesHandler
    : IRequestHandler<GetHockeyMatchesQuery, Result<PagedResult<HockeyMatchListDto>>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IPaginationService _paginationService;
    private readonly ILogger<GetHockeyMatchesHandler> _logger;

    public GetHockeyMatchesHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyTeamRepository teamRepository,
        IPaginationService paginationService,
        ILogger<GetHockeyMatchesHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _paginationService = paginationService;
        _logger = logger;
    }

    public async Task<Result<PagedResult<HockeyMatchListDto>>> Handle(
        GetHockeyMatchesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            int pageSize = _paginationService.ResolvePageSize(
                GetHockeyMatchesQuery.ResourceKey,
                request.PageSize);

            PagedResult<HockeyMatch> pagedMatches = await _matchRepository.GetPagedAsync(
                request.Page,
                pageSize,
                competitionId: null,
                teamId: null,
                request.StartDate,
                request.EndDate,
                status: null,
                request.SortOrder,
                searchQuery: null,
                request.TeamCategory,
                excludeDraftCompetitions: !request.IncludeDrafts,
                cancellationToken: cancellationToken);

            IReadOnlyList<Guid> teamIds = pagedMatches.Items
                .SelectMany(match => match.MatchTeams.Select(team => team.TeamId))
                .Distinct()
                .ToList();

            IReadOnlyDictionary<Guid, string> teamNames = teamIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _teamRepository.GetNamesByIdsAsync(teamIds, cancellationToken);

            IReadOnlyList<HockeyMatchListDto> items =
                HockeyMatchListMapper.ToDtos(pagedMatches.Items, teamNames);

            return Result<PagedResult<HockeyMatchListDto>>.Success(
                PagedResult.Create(items, pagedMatches.TotalCount, pagedMatches.Page, pagedMatches.PageSize));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Public hockey match retrieval was cancelled");
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid public hockey match query");
            return Result<PagedResult<HockeyMatchListDto>>.Failure(
                "An error occurred while retrieving hockey matches.",
                ex.Flatten());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get public hockey matches");
            return Result<PagedResult<HockeyMatchListDto>>.Failure(
                "An error occurred while retrieving hockey matches.",
                ex.Flatten());
        }
    }
}
