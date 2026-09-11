using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class AddTeamToSeasonDivisionHandler : IRequestHandler<AddTeamToSeasonDivisionCommand, Result>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly ILogger<AddTeamToSeasonDivisionHandler> _logger;

    public AddTeamToSeasonDivisionHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballTeamRepository teamRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        ILogger<AddTeamToSeasonDivisionHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _teamRepository = teamRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(AddTeamToSeasonDivisionCommand request, CancellationToken cancellationToken)
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
                "Adding team {TeamId} to season {SeasonId} division {DivisionId}",
                request.TeamId,
                request.CompetitionId,
                request.DivisionId);
            await _seasonDivisionRepository.AddTeamToCompetitionDivisionAsync(
                request.CompetitionId,
                request.DivisionId,
                request.TeamId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error adding team {TeamId} to season {SeasonId} division {DivisionId}",
                request.TeamId,
                request.CompetitionId,
                request.DivisionId);
            return Result.Failure("Failed to add team to season division.");
        }
    }
}
