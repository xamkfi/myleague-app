using Application.Features.Floorball.Matches.Queries;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Application.Common;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for retrieving floorball matches by season
/// </summary>
public class GetFloorballMatchesBySeasonHandler : IRequestHandler<GetFloorballMatchesBySeasonQuery, Result<IEnumerable<FloorballMatchDto>>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetFloorballMatchesBySeasonHandler> _logger;

    public GetFloorballMatchesBySeasonHandler(
        IFloorballMatchRepository matchRepository,
        IClubRepository clubRepository,
        ILogger<GetFloorballMatchesBySeasonHandler> logger)
    {
        _matchRepository = matchRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<FloorballMatchDto>>> Handle(GetFloorballMatchesBySeasonQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball matches for season: {SeasonId}", request.CompetitionId);
            
            IEnumerable<FloorballMatch> matches = await _matchRepository.GetByCompetitionIdAsync(request.CompetitionId);
            List<Guid> clubIds = FloorballMatchMapper.CollectClubIds(matches);
            Dictionary<Guid, Club> clubLookup = clubIds.Count == 0
                ? new Dictionary<Guid, Club>()
                : await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);
            IEnumerable<FloorballMatchDto> matchDtos = FloorballMatchMapper.ToDtos(matches, clubLookup);
            
            _logger.LogInformation("Successfully retrieved {MatchCount} floorball matches for season: {SeasonId}", matchDtos.Count(), request.CompetitionId);
            
            return Result<IEnumerable<FloorballMatchDto>>.Success(matchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball matches for season: {SeasonId}", request.CompetitionId);
            return Result<IEnumerable<FloorballMatchDto>>.Failure("An error occurred while retrieving floorball matches.");
        }
    }
}
