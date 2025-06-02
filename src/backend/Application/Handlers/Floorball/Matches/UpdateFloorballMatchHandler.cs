using Application.Commands.Floorball;
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
/// Handler for updating an existing floorball match
/// </summary>
public class UpdateFloorballMatchHandler : IRequestHandler<UpdateFloorballMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFloorballMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFloorballMatchHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFloorballMatchCommand request
    /// </summary>
    /// <param name="request">The command containing updated match information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(UpdateFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the existing match
            FloorballMatch? existingMatch = await _matchRepository.GetByIdAsync(request.MatchId);
            if (existingMatch == null)
            {
                _logger.LogWarning("Attempt to update non-existent floorball match with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.NotFound("FloorballMatch", request.MatchId);
            }

            // Verify teams exist if they are being updated
            if (request.HomeTeamId != existingMatch.HomeTeamId)
            {
                bool homeTeamExists = await _teamRepository.ExistsAsync(request.HomeTeamId);
                if (!homeTeamExists)
                {
                    _logger.LogWarning("Attempt to update match with non-existent home team ID: {TeamId}", request.HomeTeamId);
                    return Result<FloorballMatchDto>.NotFound("FloorballTeam", request.HomeTeamId);
                }
            }

            if (request.AwayTeamId != existingMatch.AwayTeamId)
            {
                bool awayTeamExists = await _teamRepository.ExistsAsync(request.AwayTeamId);
                if (!awayTeamExists)
                {
                    _logger.LogWarning("Attempt to update match with non-existent away team ID: {TeamId}", request.AwayTeamId);
                    return Result<FloorballMatchDto>.NotFound("FloorballTeam", request.AwayTeamId);
                }
            }

            // Update the match
            FloorballMatchMapper.UpdateFromCommand(existingMatch, request);
            
            _logger.LogInformation("Updating floorball match: {MatchId}", existingMatch.Id);
            await _matchRepository.UpdateAsync(existingMatch);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(existingMatch);
            _logger.LogInformation("Successfully updated floorball match with ID: {MatchId}", existingMatch.Id);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating floorball match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while updating the floorball match.");
        }
    }
} 