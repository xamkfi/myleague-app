using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Matches.Queries;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Floorball.Matches.Handlers;

public class GetTodaysMatchesByTeamHandler : IRequestHandler<GetTodaysMatchesByTeamQuery, Result<IEnumerable<FloorballMatchDto>>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetTodaysMatchesByTeamHandler> _logger;

    public GetTodaysMatchesByTeamHandler(
        IFloorballMatchRepository matchRepository,
        IClubRepository clubRepository,
        ILogger<GetTodaysMatchesByTeamHandler> logger)
    {
        _matchRepository = matchRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<FloorballMatchDto>>> Handle(GetTodaysMatchesByTeamQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting today's matches for team {teamId}", request.TeamId);

            IEnumerable<FloorballMatch> matches = await _matchRepository.GetTodaysMatchesByTeamAsync(request.TeamId, cancellationToken);
            List<Guid> clubIds = FloorballMatchMapper.CollectClubIds(matches);
            Dictionary<Guid, Club> clubLookup = clubIds.Count == 0
                ? new Dictionary<Guid, Club>()
                : await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);
            IEnumerable<FloorballMatchDto> matchDtos = FloorballMatchMapper.ToDtos(matches, clubLookup);

            return Result<IEnumerable<FloorballMatchDto>>.Success(matchDtos);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error getting today's matches for team {teamId}", request.TeamId);
            return Result<IEnumerable<FloorballMatchDto>>.Failure("Error getting today's matches");
        }
    }
} 
