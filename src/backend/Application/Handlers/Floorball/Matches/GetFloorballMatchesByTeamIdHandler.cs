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
/// Handler for retrieving floorball matches by team ID
/// </summary>
public class GetFloorballMatchesByTeamIdHandler : IRequestHandler<GetFloorballMatchesByTeamIdQuery, Result<IEnumerable<FloorballMatchDto>>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly ILogger<GetFloorballMatchesByTeamIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballMatchesByTeamIdHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballMatchesByTeamIdHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        ILogger<GetFloorballMatchesByTeamIdHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballMatchesByTeamIdQuery request
    /// </summary>
    /// <param name="request">The query containing the team ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matches for the team as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballMatchDto>>> Handle(GetFloorballMatchesByTeamIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify team exists
            bool teamExists = await _teamRepository.ExistsAsync(request.TeamId);
            if (!teamExists)
            {
                _logger.LogWarning("Attempt to get matches for non-existent team with ID: {TeamId}", request.TeamId);
                return Result<IEnumerable<FloorballMatchDto>>.NotFound("FloorballTeam", request.TeamId);
            }

            _logger.LogInformation("Retrieving floorball matches for team: {TeamId}", request.TeamId);
            
            IEnumerable<FloorballMatch> matches = await _matchRepository.GetByTeamIdAsync(request.TeamId);
            IEnumerable<FloorballMatchDto> matchDtos = FloorballMatchMapper.ToDtos(matches);
            
            _logger.LogInformation("Successfully retrieved {MatchCount} matches for team {TeamId}", 
                matchDtos.Count(), request.TeamId);
            
            return Result<IEnumerable<FloorballMatchDto>>.Success(matchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball matches for team: {TeamId}", request.TeamId);
            return Result<IEnumerable<FloorballMatchDto>>.Failure("An error occurred while retrieving the team's matches.");
        }
    }
} 