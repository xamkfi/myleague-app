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
using Application.Commands.Floorball.Match;
using Domain.Repositories.Common;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for updating an existing floorball match
/// </summary>
public class UpdateFloorballMatchHandler : IRequestHandler<UpdateFloorballMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFloorballMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFloorballMatchHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
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
            FloorballMatch? existingMatch = await _matchRepository.GetByIdAsync(request.Id);
            if (existingMatch == null)
            {
                _logger.LogWarning("Attempt to update non-existent floorball match with ID: {MatchId}", request.Id);
                return Result<FloorballMatchDto>.NotFound("FloorballMatch", request.Id);
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
            _logger.LogError(ex, "Error occurred while updating floorball match: {MatchId}", request.Id);
            return Result<FloorballMatchDto>.Failure("An error occurred while updating the floorball match.");
        }
    }
} 
