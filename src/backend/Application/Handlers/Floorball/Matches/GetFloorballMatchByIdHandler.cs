using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for retrieving a floorball match by ID
/// </summary>
public class GetFloorballMatchByIdHandler : IRequestHandler<GetFloorballMatchByIdQuery, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly ILogger<GetFloorballMatchByIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballMatchByIdHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballMatchByIdHandler(
        IFloorballMatchRepository matchRepository,
        ILogger<GetFloorballMatchByIdHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballMatchByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The match as a DTO wrapped in a Result, or a not found result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(GetFloorballMatchByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball match with ID: {MatchId}", request.MatchId);
            
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found", request.MatchId);
                return Result<FloorballMatchDto>.NotFound("FloorballMatch", request.MatchId);
            }

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully retrieved floorball match: {MatchId}", match.Id);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while retrieving the floorball match.");
        }
    }
} 