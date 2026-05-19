using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Features.Floorball.Statistics.Queries;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Statistics.Handlers;

/// <summary>
/// Handler for retrieving top scorers for a season
/// </summary>
public class GetTopScorersHandler : IRequestHandler<GetTopScorersQuery, Result<List<FloorballPlayerSeasonStatisticsDto>>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetTopScorersHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetTopScorersHandler class
    /// </summary>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="logger">The logger</param>
    public GetTopScorersHandler(
        IFloorballStatisticsRepository statisticsRepository,
        IPersonRepository personRepository,
        ILogger<GetTopScorersHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetTopScorersQuery request
    /// </summary>
    /// <param name="request">The query containing season ID and top N count</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of top scoring players wrapped in a Result</returns>
    public async Task<Result<List<FloorballPlayerSeasonStatisticsDto>>> Handle(GetTopScorersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving top {TopN} scorers for Season {SeasonId}", request.TopN, request.CompetitionId);

            List<Domain.Entities.Floorball.FloorballPlayerSeasonStatistics>? topScorers = await _statisticsRepository.GetTopScorersAsync(request.CompetitionId, request.TopN, cancellationToken);

            IEnumerable<Guid> PersonIds = topScorers.Select(x => x.Player.PersonId).ToList();
            IEnumerable<Person>? persons = await _personRepository.GetByIdsAsync(PersonIds);

            // Build lookup: PersonId -> FullName
            var personLookup = persons.ToDictionary(p => p.Id, p => p.FullName);

            // Map DTOs
            var dtos = topScorers.Select(stats =>
            {
                string playerName = personLookup.TryGetValue(stats.Player.PersonId, out string? fullName)
                    ? fullName
                    : string.Empty;

                return FloorballStatisticsMapper.ToDto(stats, playerName);
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} top scorers for Season {SeasonId}", dtos.Count, request.CompetitionId);
            return Result<List<FloorballPlayerSeasonStatisticsDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving top scorers for Season {SeasonId}", request.CompetitionId);
            return Result<List<FloorballPlayerSeasonStatisticsDto>>.Failure("An error occurred while retrieving top scorers.");
        }
    }
}
