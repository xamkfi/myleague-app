using Application.Commands.Clubs;
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
    /// <param name="command">The command containing the ID of the club to delete</param>
    /// <returns>True if the club was deleted, false if it wasn't found</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    /// <exception cref="ArgumentException">Thrown when the clubId is empty</exception>
    public async Task<bool> ExecuteAsync(DeleteClubCommand command)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        if (command.ClubId == Guid.Empty)
        {
            _logger.LogError("Club ID cannot be empty");
            throw new ArgumentException("Club ID cannot be empty", nameof(command.ClubId));
        }

        bool exists = await _clubRepository.ExistsAsync(command.ClubId);
        if (!exists)
        {
            _logger.LogWarning("Club with ID {ClubId} not found", command.ClubId);
            return false;
        }

        _logger.LogInformation("Deleting club with ID: {ClubId}", command.ClubId);
        await _clubRepository.DeleteAsync(command.ClubId);
        return true;
    }
} 
