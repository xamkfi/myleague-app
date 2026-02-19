using Application.Common;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Queries.Clubs;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Clubs;

/// <summary>
/// Handler for retrieving clubs by name search
/// </summary>
public class GetClubsByNameHandler : IRequestHandler<GetClubsByNameQuery, Result<IEnumerable<ClubDto>>>
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetClubsByNameHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetClubsByNameHandler class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetClubsByNameHandler(IClubRepository clubRepository, ILogger<GetClubsByNameHandler> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetClubsByNameQuery request
    /// </summary>
    /// <param name="request">The query containing the search name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of clubs matching the search term wrapped in a Result</returns>
    public async Task<Result<IEnumerable<ClubDto>>> Handle(GetClubsByNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            const int defaultSearchResultCount = 25;
            _logger.LogInformation("Searching clubs by name: {Name}", request.name);

            IEnumerable<Club> clubs = await _clubRepository.SearchByNameAsync(request.name, defaultSearchResultCount, cancellationToken);
            
            if (!clubs.Any())
            {
                _logger.LogWarning("Clubs with name {Name} not found", request.name);
                return Result<IEnumerable<ClubDto>>.NotFound("Club", request.name);
            }

            IEnumerable<ClubDto> clubDtos = ClubMapper.ToDtos(clubs);

            return Result<IEnumerable<ClubDto>>.Success(clubDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching clubs by name: {Name}", request.name);
            return Result<IEnumerable<ClubDto>>.Failure("An error occurred while retrieving the clubs.");
        }
    }
}
