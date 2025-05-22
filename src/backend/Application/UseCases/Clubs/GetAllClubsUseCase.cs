using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    /// <returns>A collection of all clubs as DTOs</returns>
    public async Task<IEnumerable<ClubDto>> ExecuteAsync()
    {
        _logger.LogInformation("Retrieving all clubs");
        IEnumerable<Club> clubs = await _clubRepository.GetAllAsync();
        return ClubMapper.ToDtos(clubs);
    }
} 