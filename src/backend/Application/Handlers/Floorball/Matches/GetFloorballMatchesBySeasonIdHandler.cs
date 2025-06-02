using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for retrieving floorball matches by season ID
/// </summary>
public class GetFloorballMatchesBySeasonIdHandler : IRequestHandler<GetFloorballMatchesBySeasonIdQuery, Result<IEnumerable<FloorballMatchDto>>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly ILogger<GetFloorballMatchesBySeasonIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballMatchesBySeasonIdHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballMatchesBySeasonIdHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballSeasonRepository seasonRepository,
        ILogger<GetFloorballMatchesBySeasonIdHandler> logger)
    {
        _matchRepository = matchRepository;
        _seasonRepository = seasonRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballMatchesBySeasonIdQuery request
    /// </summary>
    /// <param name="request">The query containing the season ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matches in the season as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballMatchDto>>> Handle(GetFloorballMatchesBySeasonIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify season exists
            bool seasonExists = await _seasonRepository.ExistsAsync(request.SeasonId);
            if (!seasonExists)
            {
                _logger.LogWarning("Attempt to get matches for non-existent season with ID: {SeasonId}", request.SeasonId);
                return Result<IEnumerable<FloorballMatchDto>>.NotFound("FloorballSeason", request.SeasonId);
            }

            _logger.LogInformation("Retrieving floorball matches for season: {SeasonId}", request.SeasonId);
            
            IEnumerable<FloorballMatch> matches = await _matchRepository.GetBySeasonIdAsync(request.SeasonId);
            IEnumerable<FloorballMatchDto> matchDtos = FloorballMatchMapper.ToDtos(matches);
            
            _logger.LogInformation("Successfully retrieved {MatchCount} matches for season {SeasonId}", 
                matchDtos.Count(), request.SeasonId);
            
            return Result<IEnumerable<FloorballMatchDto>>.Success(matchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball matches for season: {SeasonId}", request.SeasonId);
            return Result<IEnumerable<FloorballMatchDto>>.Failure("An error occurred while retrieving the season's matches.");
        }
    }
} 