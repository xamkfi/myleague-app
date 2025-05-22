using Application.DTOs;
using Application.DTOs.Common;
using Application.Mappings;
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
    /// <param name="clubId">The ID of the club to update</param>
    /// <param name="request">The request containing updated club information</param>
    /// <returns>The updated club as a DTO if found, null otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when the clubId is empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when a club with the same name already exists</exception>
    public async Task<ClubDto?> ExecuteAsync(Guid clubId, UpdateClubRequest request)
    {
        if (clubId == Guid.Empty)
        {
            _logger.LogError("Club ID cannot be empty");
            throw new ArgumentException("Club ID cannot be empty", nameof(clubId));
        }

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        Club? club = await _clubRepository.GetByIdAsync(clubId);
        if (club == null)
        {
            _logger.LogWarning("Club with ID {ClubId} not found", clubId);
            return null;
        }

        // Check if name is being changed and if the new name is already taken
        if (club.Name != request.Name && await _clubRepository.ExistsByNameAsync(request.Name))
        {
            _logger.LogError("A club with the name {ClubName} already exists", request.Name);
            throw new InvalidOperationException($"A club with the name '{request.Name}' already exists.");
        }

        // Update club from request
        ClubMapper.UpdateFromRequest(club, request);

        _logger.LogInformation("Updating club with ID: {ClubId}", clubId);
        await _clubRepository.UpdateAsync(club);

        return ClubMapper.ToDto(club);
    }
} 