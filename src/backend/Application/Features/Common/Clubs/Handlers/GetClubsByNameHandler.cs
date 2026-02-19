using Application.Common;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Application.Features.Common.Clubs.Queries;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Clubs.Handlers;

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
