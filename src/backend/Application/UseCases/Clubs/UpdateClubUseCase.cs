using Application.Commands.Clubs;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Application.UseCases.Clubs;

/// <summary>
/// Use case for updating an existing club
/// </summary>
public class UpdateClubUseCase
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<UpdateClubUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateClubUseCase class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public UpdateClubUseCase(IClubRepository clubRepository, ILogger<UpdateClubUseCase> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executes the use case to update an existing club
    /// </summary>
    /// <param name="command">The command containing updated club information</param>
    /// <returns>The updated club as a DTO if found, null otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    /// <exception cref="ArgumentException">Thrown when the clubId is empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when a club with the same name already exists</exception>
    public async Task<ClubDto?> ExecuteAsync(UpdateClubCommand command)
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

        Club? club = await _clubRepository.GetByIdAsync(command.ClubId);
        if (club == null)
        {
            _logger.LogWarning("Club with ID {ClubId} not found", command.ClubId);
            return null;
        }

        // Check if name is being changed and if the new name is already taken
        if (club.Name != command.Name && await _clubRepository.ExistsByNameAsync(command.Name))
        {
            _logger.LogError("A club with the name {ClubName} already exists", command.Name);
            throw new InvalidOperationException($"A club with the name '{command.Name}' already exists.");
        }

        // Update club from command
        ClubMapper.UpdateFromCommand(club, command);

        _logger.LogInformation("Updating club with ID: {ClubId}", command.ClubId);
        await _clubRepository.UpdateAsync(club);

        return ClubMapper.ToDto(club);
    }
} 
