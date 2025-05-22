using Application.DTOs.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Application.UseCases.Clubs;

/// <summary>
/// Use case for deleting a club
/// </summary>
public class DeleteClubUseCase
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<DeleteClubUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteClubUseCase class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public DeleteClubUseCase(IClubRepository clubRepository, ILogger<DeleteClubUseCase> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executes the use case to delete a club
    /// </summary>
    /// <param name="request">The request containing the ID of the club to delete</param>
    /// <returns>True if the club was deleted, false if it wasn't found</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="ArgumentException">Thrown when the clubId is empty</exception>
    public async Task<bool> ExecuteAsync(DeleteClubRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.ClubId == Guid.Empty)
        {
            _logger.LogError("Club ID cannot be empty");
            throw new ArgumentException("Club ID cannot be empty", nameof(request.ClubId));
        }

        bool exists = await _clubRepository.ExistsAsync(request.ClubId);
        if (!exists)
        {
            _logger.LogWarning("Club with ID {ClubId} not found", request.ClubId);
            return false;
        }

        _logger.LogInformation("Deleting club with ID: {ClubId}", request.ClubId);
        await _clubRepository.DeleteAsync(request.ClubId);
        return true;
    }
} 
