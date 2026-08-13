using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Features.Football.Matches.Queries;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class GetAllFootballMatchesHandler : BasePagedQueryHandler<GetAllFootballMatchesQuery, FootballMatchDto>,
    IRequestHandler<GetAllFootballMatchesQuery, Result<PagedResult<FootballMatchDto>>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IClubRepository _clubRepository;

    public GetAllFootballMatchesHandler(
        IFootballMatchRepository matchRepository,
        IClubRepository clubRepository,
        IPaginationService paginationService,
        ILogger<GetAllFootballMatchesHandler> logger) : base(paginationService, logger)
    {
        _matchRepository = matchRepository;
        _clubRepository = clubRepository;
    }

    public async Task<Result<PagedResult<FootballMatchDto>>> Handle(
        GetAllFootballMatchesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page,
                request.PageSize,
                GetAllFootballMatchesQuery.ResourceKey);

            if (validationResult.IsFailure)
            {
                return Result<PagedResult<FootballMatchDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            PagedResult<FootballMatch> pagedMatches = await _matchRepository.GetPagedAsync(
                page: validationResult.Data.Page,
                pageSize: actualPageSize,
                competitionId: request.CompetitionId,
                teamId: request.TeamId,
                startDate: request.StartDate,
                endDate: request.EndDate,
                status: request.Status,
                sortOrder: request.SortOrder,
                searchQuery: request.SearchQuery,
                tournamentGroupId: request.TournamentGroupId,
                competitionType: request.CompetitionType,
                teamCategory: request.TeamCategory,
                cancellationToken: cancellationToken);

            IEnumerable<FootballMatch> matches = pagedMatches.Items ?? Enumerable.Empty<FootballMatch>();

            Dictionary<Guid, Club> clubLookup;
            if (!matches.Any())
            {
                clubLookup = new Dictionary<Guid, Club>();
            }
            else
            {
                List<Guid> clubIds = matches
                    .SelectMany(m => new[] { m.HomeTeam?.ClubId, m.AwayTeam?.ClubId })
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToList();

                clubLookup = await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);
            }

            IEnumerable<FootballMatchDto> matchDtos = FootballMatchMapper.ToDtos(matches, clubLookup);
            PagedResult<FootballMatchDto> pagedResult = CreatePagedResult(
                matchDtos,
                pagedMatches.TotalCount,
                pagedMatches.Page,
                pagedMatches.PageSize);

            return Result<PagedResult<FootballMatchDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "Football matches retrieval was cancelled - Page: {Page}, PageSize: {PageSize}",
                request.Page,
                request.PageSize);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football matches");
            return Result<PagedResult<FootballMatchDto>>.Failure("An error occurred while retrieving football matches.");
        }
    }
}
