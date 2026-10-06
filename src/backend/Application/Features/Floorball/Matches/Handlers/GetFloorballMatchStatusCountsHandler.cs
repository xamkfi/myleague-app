using Application.Common;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Floorball.Matches.Queries;
using Domain.Common;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handles <see cref="GetFloorballMatchStatusCountsQuery"/> with a single grouped count query.
/// </summary>
public class GetFloorballMatchStatusCountsHandler
    : IRequestHandler<GetFloorballMatchStatusCountsQuery, Result<MatchStatusCountsDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly ILogger<GetFloorballMatchStatusCountsHandler> _logger;

    public GetFloorballMatchStatusCountsHandler(
        IFloorballMatchRepository matchRepository,
        ILogger<GetFloorballMatchStatusCountsHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result<MatchStatusCountsDto>> Handle(
        GetFloorballMatchStatusCountsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyDictionary<FloorballMatchStatus, int> counts = await _matchRepository.GetStatusCountsAsync(
                request.CompetitionId,
                request.SearchQuery,
                request.CompetitionType,
                excludeDraftCompetitions: !request.IncludeDrafts,
                cancellationToken);

            return Result<MatchStatusCountsDto>.Success(new MatchStatusCountsDto(
                counts.Values.Sum(),
                counts.GetValueOrDefault(FloorballMatchStatus.Scheduled),
                counts.GetValueOrDefault(FloorballMatchStatus.Postponed),
                counts.GetValueOrDefault(FloorballMatchStatus.InProgress),
                counts.GetValueOrDefault(FloorballMatchStatus.Completed),
                counts.GetValueOrDefault(FloorballMatchStatus.Cancelled)));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to count floorball matches by status");
            return Result<MatchStatusCountsDto>.Failure("An error occurred while counting floorball matches.");
        }
    }
}
