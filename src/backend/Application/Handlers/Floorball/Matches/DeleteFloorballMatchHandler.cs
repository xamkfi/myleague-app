using Application.Commands.Floorball;
using Application.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for deleting a floorball match
/// </summary>
public class DeleteFloorballMatchHandler : IRequestHandler<DeleteFloorballMatchCommand, Result>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFloorballMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteFloorballMatchHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public DeleteFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteFloorballMatchCommand request
    /// </summary>
    /// <param name="request">The command containing the match ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result> Handle(DeleteFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the match exists
            bool matchExists = await _matchRepository.ExistsAsync(request.MatchId);
            if (!matchExists)
            {
                _logger.LogWarning("Attempt to delete non-existent floorball match with ID: {MatchId}", request.MatchId);
                return Result.NotFound("FloorballMatch", request.MatchId);
            }

            _logger.LogInformation("Deleting floorball match with ID: {MatchId}", request.MatchId);
            await _matchRepository.DeleteAsync(request.MatchId);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted floorball match with ID: {MatchId}", request.MatchId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting floorball match: {MatchId}", request.MatchId);
            return Result.Failure("An error occurred while deleting the floorball match.");
        }
    }
} 