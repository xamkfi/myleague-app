using Application.Queries.Clubs;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Clubs;

/// <summary>
/// Handler for retrieving all clubs
/// </summary>
public class GetAllClubsHandler : IRequestHandler<GetAllClubsQuery, Result<IEnumerable<ClubDto>>>
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
    /// Handles the GetAllClubsQuery request
    /// </summary>
    /// <param name="request">The query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All clubs as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<ClubDto>>> Handle(GetAllClubsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all clubs");
            
            IEnumerable<Club> clubs = await _clubRepository.GetAllAsync();
            IEnumerable<ClubDto> clubDtos = ClubMapper.ToDtos(clubs);
            
            _logger.LogInformation("Successfully retrieved {ClubCount} clubs", clubDtos.Count());
            
            return Result<IEnumerable<ClubDto>>.Success(clubDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all clubs");
            return Result<IEnumerable<ClubDto>>.Failure("An error occurred while retrieving clubs.");
        }
    }
} 