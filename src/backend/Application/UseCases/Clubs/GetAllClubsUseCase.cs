using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Clubs;

/// <summary>
/// Use case for retrieving all clubs
/// </summary>
public class GetAllClubsUseCase
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetAllClubsUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllClubsUseCase class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetAllClubsUseCase(IClubRepository clubRepository, ILogger<GetAllClubsUseCase> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executes the use case to retrieve all clubs
    /// </summary>
    /// <returns>A collection of all clubs</returns>
    public async Task<IEnumerable<Club>> ExecuteAsync()
    {
        _logger.LogInformation("Retrieving all clubs");
        return await _clubRepository.GetAllAsync();
    }
} 