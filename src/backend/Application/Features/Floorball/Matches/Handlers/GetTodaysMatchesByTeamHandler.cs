using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Queries.Floorball.Match;
using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Matches;

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