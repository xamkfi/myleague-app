using Application.Commands.Floorball;
using Application.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for deleting a floorball season
/// </summary>
public class DeleteFloorballSeasonHandler : IRequestHandler<DeleteFloorballSeasonCommand, Result>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFloorballSeasonHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteFloorballSeasonHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public DeleteFloorballSeasonHandler(
        IFloorballSeasonRepository seasonRepository,
        IFloorballMatchRepository matchRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteFloorballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteFloorballSeasonCommand request
    /// </summary>
    /// <param name="request">The command containing the season ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result> Handle(DeleteFloorballSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the season exists
            bool seasonExists = await _seasonRepository.ExistsAsync(request.SeasonId);
            if (!seasonExists)
            {
                _logger.LogWarning("Attempt to delete non-existent floorball season with ID: {SeasonId}", request.SeasonId);
                return Result.NotFound("FloorballSeason", request.SeasonId);
            }

            // Check if there are any matches in this season
            bool hasMatches = await _matchRepository.HasMatchesInSeasonAsync(request.SeasonId);
            if (hasMatches)
            {
                _logger.LogWarning("Attempt to delete season with existing matches: {SeasonId}", request.SeasonId);
                return Result.Failure("Cannot delete a season that has matches. Delete the matches first.");
            }

            _logger.LogInformation("Deleting floorball season with ID: {SeasonId}", request.SeasonId);
            await _seasonRepository.DeleteAsync(request.SeasonId);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted floorball season with ID: {SeasonId}", request.SeasonId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting floorball season: {SeasonId}", request.SeasonId);
            return Result.Failure("An error occurred while deleting the floorball season.");
        }
    }
} 