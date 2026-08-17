using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Features.Football.Matches.Queries;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class GetTodaysMatchesByTeamHandler : IRequestHandler<GetTodaysMatchesByTeamQuery, Result<IEnumerable<FootballMatchDto>>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly ILogger<GetTodaysMatchesByTeamHandler> _logger;

    public GetTodaysMatchesByTeamHandler(
        IFootballMatchRepository matchRepository,
        ILogger<GetTodaysMatchesByTeamHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<FootballMatchDto>>> Handle(
        GetTodaysMatchesByTeamQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<FootballMatch> matches =
                await _matchRepository.GetTodaysMatchesByTeamAsync(request.TeamId, cancellationToken);
            IEnumerable<FootballMatchDto> matchDtos = FootballMatchMapper.ToDtos(matches);
            return Result<IEnumerable<FootballMatchDto>>.Success(matchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's matches for team {teamId}", request.TeamId);
            return Result<IEnumerable<FootballMatchDto>>.Failure("Error getting today's matches");
        }
    }
}
