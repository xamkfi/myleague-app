using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Clubs;

/// <summary>
/// Use case for creating a new club
/// </summary>
public class CreateClubUseCase
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<CreateClubUseCase> _logger;

    public CreateClubUseCase(IClubRepository clubRepository, ILogger<CreateClubUseCase> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executes the use case to create a new club
    /// </summary>
    /// <param name="name">The name of the club</param>
    /// <param name="city">The city where the club is based</param>
    /// <param name="country">The country where the club is based</param>
    /// <param name="foundingDate">The founding date of the club</param>
    /// <param name="websiteUrl">The club's official website URL</param>
    /// <param name="logoUrl">The club's logo URL</param>
    /// <param name="contactEmail">The primary contact email for the club</param>
    /// <returns>The newly created club</returns>
    /// <exception cref="InvalidOperationException">Thrown when a club with the same name already exists</exception>
    public async Task<Club> ExecuteAsync(
        string name,
        string city,
        string country,
        DateTime? foundingDate = null,
        Uri? websiteUrl = null,
        Uri? logoUrl = null,
        string? contactEmail = null)
    {
        if (await _clubRepository.ExistsByNameAsync(name))
        {
            _logger.LogError("A club with the name {ClubName} already exists", name);
            throw new InvalidOperationException($"A club with the name '{name}' already exists.");
        }

        var club = new Club(
            name: name,
            city: city,
            country: country,
            foundingDate: foundingDate,
            websiteUrl: websiteUrl,
            logoUrl: logoUrl,
            contactEmail: contactEmail);

        _logger.LogInformation("Creating new club: {ClubName}", club.Name);
        await _clubRepository.AddAsync(club);

        return club;
    }
}
