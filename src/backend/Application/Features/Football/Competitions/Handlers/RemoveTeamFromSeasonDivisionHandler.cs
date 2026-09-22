using Application.Common;
using Application.Features.Football.Competitions.Commands;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Competitions.Handlers;

public class RemoveTeamFromSeasonDivisionHandler : IRequestHandler<RemoveFootballTeamFromSeasonDivisionCommand, Result>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly ILogger<RemoveTeamFromSeasonDivisionHandler> _logger;

    public RemoveTeamFromSeasonDivisionHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballTeamRepository teamRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        ILogger<RemoveTeamFromSeasonDivisionHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _teamRepository = teamRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveFootballTeamFromSeasonDivisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _seasonRepository.ExistsAsync(request.CompetitionId))
            {
                return Result.NotFound("FootballSeason", request.CompetitionId);
            }

            if (!await _teamRepository.ExistsAsync(request.TeamId))
            {
                return Result.NotFound("FootballTeam", request.TeamId);
            }

            _logger.LogInformation(
                "Removing team {TeamId} from season {SeasonId} division {DivisionId}",
                request.TeamId,
                request.CompetitionId,
                request.DivisionId);
            await _seasonDivisionRepository.RemoveTeamFromCompetitionDivisionAsync(
                request.CompetitionId,
                request.DivisionId,
                request.TeamId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error removing team {TeamId} from season {SeasonId} division {DivisionId}",
                request.TeamId,
                request.CompetitionId,
                request.DivisionId);
            return Result.Failure("Failed to remove team from season division.");
        }
    }
}
