using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Statistics.Mappings;
using Application.Features.Football.Statistics.Queries;
using Domain.Entities.Football.Matches;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using MediatR;

namespace Application.Features.Football.Statistics.Handlers;

/// <summary>
/// Handler for retrieving match statistics, counted from the match events
/// </summary>
public class GetMatchStatisticsHandler : IRequestHandler<GetFootballMatchStatisticsQuery, Result<List<FootballMatchTeamStatisticsDto>>>
{
    private readonly IFootballMatchRepository _matchRepository;

    public GetMatchStatisticsHandler(IFootballMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public async Task<Result<List<FootballMatchTeamStatisticsDto>>> Handle(GetFootballMatchStatisticsQuery request, CancellationToken cancellationToken)
    {
        FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match == null || match.Status == FootballMatchStatus.Scheduled)
            return Result<List<FootballMatchTeamStatisticsDto>>.Success(new List<FootballMatchTeamStatisticsDto>());

        List<FootballMatchTeamStatisticsDto> dtos = new[] { match.HomeTeamId, match.AwayTeamId }
            .OfType<Guid>()
            .Select(teamId => FootballStatisticsMapper.ToMatchTeamStatisticsDto(match, teamId))
            .ToList();

        return Result<List<FootballMatchTeamStatisticsDto>>.Success(dtos);
    }
}
