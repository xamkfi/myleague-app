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
/// Handler for retrieving all floorball matches
/// </summary>
public class GetAllFloorballMatchesHandler : IRequestHandler<GetAllFloorballMatchesQuery, Result<IEnumerable<FloorballMatchDto>>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly ILogger<GetAllFloorballMatchesHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballMatchesHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballMatchesHandler(
        IFloorballMatchRepository matchRepository,
        ILogger<GetAllFloorballMatchesHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetAllFloorballMatchesQuery request
    /// </summary>
    /// <param name="request">The query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All floorball matches as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballMatchDto>>> Handle(GetAllFloorballMatchesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all floorball matches");
            
            IEnumerable<FloorballMatch> matches = await _matchRepository.GetAllAsync();
            IEnumerable<FloorballMatchDto> matchDtos = FloorballMatchMapper.ToDtos(matches);
            
            _logger.LogInformation("Successfully retrieved {MatchCount} floorball matches", matchDtos.Count());
            
            return Result<IEnumerable<FloorballMatchDto>>.Success(matchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all floorball matches");
            return Result<IEnumerable<FloorballMatchDto>>.Failure("An error occurred while retrieving floorball matches.");
        }
    }
} 