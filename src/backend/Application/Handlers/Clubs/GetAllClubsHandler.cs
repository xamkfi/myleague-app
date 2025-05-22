using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Handlers.Clubs;

/// <summary>
/// Handler for retrieving all clubs
/// </summary>
public class GetAllClubsHandler
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetAllClubsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllClubsHandler class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetAllClubsHandler(IClubRepository clubRepository, ILogger<GetAllClubsHandler> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executes the handler to retrieve all clubs
    /// </summary>
    /// <returns>A collection of all clubs as DTOs</returns>
    public async Task<IEnumerable<ClubDto>> ExecuteAsync()
    {
        _logger.LogInformation("Retrieving all clubs");
        IEnumerable<Club> clubs = await _clubRepository.GetAllAsync();
        return ClubMapper.ToDtos(clubs);
    }
} 