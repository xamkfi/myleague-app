using Application.Common;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Football.Matches.Queries;
using Domain.Common;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

/// <summary>
/// Handles <see cref="GetFootballMatchStatusCountsQuery"/> with a single grouped count query.
/// </summary>
public class GetFootballMatchStatusCountsHandler
    : IRequestHandler<GetFootballMatchStatusCountsQuery, Result<MatchStatusCountsDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly ILogger<GetFootballMatchStatusCountsHandler> _logger;

    public GetFootballMatchStatusCountsHandler(
        IFootballMatchRepository matchRepository,
        ILogger<GetFootballMatchStatusCountsHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result<MatchStatusCountsDto>> Handle(
        GetFootballMatchStatusCountsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyDictionary<FootballMatchStatus, int> counts = await _matchRepository.GetStatusCountsAsync(
                request.CompetitionId,
                request.SearchQuery,
                request.CompetitionType,
                excludeDraftCompetitions: !request.IncludeDrafts,
                cancellationToken);

            return Result<MatchStatusCountsDto>.Success(new MatchStatusCountsDto(
                counts.Values.Sum(),
                counts.GetValueOrDefault(FootballMatchStatus.Scheduled),
                counts.GetValueOrDefault(FootballMatchStatus.Postponed),
                counts.GetValueOrDefault(FootballMatchStatus.InProgress),
                counts.GetValueOrDefault(FootballMatchStatus.Completed),
                counts.GetValueOrDefault(FootballMatchStatus.Cancelled)));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to count football matches by status");
            return Result<MatchStatusCountsDto>.Failure("An error occurred while counting football matches.");
        }
    }
}
