using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Features.Football.Matches.Queries;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class GetFootballMatchesBySeasonHandler
    : IRequestHandler<GetFootballMatchesBySeasonQuery, Result<IEnumerable<FootballMatchDto>>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly ILogger<GetFootballMatchesBySeasonHandler> _logger;

    public GetFootballMatchesBySeasonHandler(
        IFootballMatchRepository matchRepository,
        ILogger<GetFootballMatchesBySeasonHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<FootballMatchDto>>> Handle(
        GetFootballMatchesBySeasonQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<FootballMatch> matches = await _matchRepository.GetByCompetitionIdAsync(request.CompetitionId);
            IEnumerable<FootballMatchDto> matchDtos = FootballMatchMapper.ToDtos(matches);
            return Result<IEnumerable<FootballMatchDto>>.Success(matchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football matches for season: {SeasonId}", request.CompetitionId);
            return Result<IEnumerable<FootballMatchDto>>.Failure("An error occurred while retrieving football matches.");
        }
    }
}
