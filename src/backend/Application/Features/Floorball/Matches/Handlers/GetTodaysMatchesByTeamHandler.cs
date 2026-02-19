using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Features.Floorball.Matches.Queries;
using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

public class GetTodaysMatchesByTeamHandler : IRequestHandler<GetTodaysMatchesByTeamQuery, Result<IEnumerable<FloorballMatchDto>>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly ILogger<GetTodaysMatchesByTeamHandler> _logger;

    public GetTodaysMatchesByTeamHandler(IFloorballMatchRepository matchRepository, ILogger<GetTodaysMatchesByTeamHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<FloorballMatchDto>>> Handle(GetTodaysMatchesByTeamQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting today's matches for team {teamId}", request.TeamId);

            IEnumerable<FloorballMatch> matches = await _matchRepository.GetTodaysMatchesByTeamAsync(request.TeamId, cancellationToken);

            IEnumerable<FloorballMatchDto> matchDtos = FloorballMatchMapper.ToDtos(matches);

            return Result<IEnumerable<FloorballMatchDto>>.Success(matchDtos);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error getting today's matches for team {teamId}", request.TeamId);
            return Result<IEnumerable<FloorballMatchDto>>.Failure("Error getting today's matches");
        }
    }
} 
