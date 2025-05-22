using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;

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
    /// <param name="name">The new name of the club</param>
    /// <param name="city">The new city where the club is based</param>
    /// <param name="country">The new country where the club is based</param>
    /// <param name="websiteUrl">The club's new official website URL</param>
    /// <param name="logoUrl">The club's new logo URL</param>
    /// <param name="contactEmail">The new primary contact email for the club</param>
    /// <returns>The updated club if found, null otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when the clubId is empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when a club with the same name already exists</exception>
    public async Task<Club?> ExecuteAsync(
        Guid clubId,
        string name,
        string city,
        string country,
        Uri? websiteUrl = null,
        Uri? logoUrl = null,
        string? contactEmail = null)
    {
        if (clubId == Guid.Empty)
        {
            _logger.LogError("Club ID cannot be empty");
            throw new ArgumentException("Club ID cannot be empty", nameof(clubId));
        }

        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club == null)
        {
            _logger.LogWarning("Club with ID {ClubId} not found", clubId);
            return null;
        }

        // Check if name is being changed and if the new name is already taken
        if (club.Name != name && await _clubRepository.ExistsByNameAsync(name))
        {
            _logger.LogError("A club with the name {ClubName} already exists", name);
            throw new InvalidOperationException($"A club with the name '{name}' already exists.");
        }

        // Update basic info
        club.UpdateBasicInfo(name, city, country);
        
        // Update online presence
        club.UpdateOnlinePresence(websiteUrl, logoUrl, contactEmail);

        _logger.LogInformation("Updating club with ID: {ClubId}", clubId);
        await _clubRepository.UpdateAsync(club);

        return club;
    }
} 