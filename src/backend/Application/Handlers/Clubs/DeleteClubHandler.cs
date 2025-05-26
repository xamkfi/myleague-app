using Application.Commands.Clubs;
using Application.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Clubs;

/// <summary>
/// Handler for deleting a club
/// </summary>
public class DeleteClubHandler : IRequestHandler<DeleteClubCommand, Result>
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteClubHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteClubHandler class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public DeleteClubHandler(IClubRepository clubRepository, IUnitOfWork unitOfWork, ILogger<DeleteClubHandler> logger)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteClubCommand request
    /// </summary>
    /// <param name="request">The command containing the club ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result> Handle(DeleteClubCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the club exists
            bool clubExists = await _clubRepository.ExistsAsync(request.ClubId);
            if (!clubExists)
            {
                _logger.LogWarning("Attempt to delete non-existent club with ID: {ClubId}", request.ClubId);
                return Result.NotFound("Club", request.ClubId);
            }

            _logger.LogInformation("Deleting club with ID: {ClubId}", request.ClubId);
            await _clubRepository.DeleteAsync(request.ClubId);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted club with ID: {ClubId}", request.ClubId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting club: {ClubId}", request.ClubId);
            return Result.Failure("An error occurred while deleting the club.");
        }
    }
} 