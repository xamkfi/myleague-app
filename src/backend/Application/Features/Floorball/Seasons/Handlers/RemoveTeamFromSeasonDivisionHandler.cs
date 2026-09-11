using Application.Features.Floorball.Seasons.Commands;
using Application.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Seasons.Handlers;

/// <summary>
/// Handler to remove a team from a season division
/// </summary>
public class RemoveTeamFromSeasonDivisionHandler : IRequestHandler<RemoveTeamFromSeasonDivisionCommand, Result>
{
    private readonly IFloorballCompetitionRepository _seasonRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly ILogger<RemoveTeamFromSeasonDivisionHandler> _logger;

    public RemoveTeamFromSeasonDivisionHandler(
        IFloorballCompetitionRepository seasonRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballCompetitionDivisionRepository seasonDivisionRepository,
        ILogger<RemoveTeamFromSeasonDivisionHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _teamRepository = teamRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveTeamFromSeasonDivisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _seasonRepository.ExistsAsync(request.CompetitionId))
            {
                return Result.NotFound("FloorballSeason", request.CompetitionId);
            }
            if (!await _teamRepository.ExistsAsync(request.TeamId))
            {
                return Result.NotFound("FloorballTeam", request.TeamId);
            }

            _logger.LogInformation("Removing team {TeamId} from season {SeasonId} division {DivisionId}", request.TeamId, request.CompetitionId, request.DivisionId);
            await _seasonDivisionRepository.RemoveTeamFromCompetitionDivisionAsync(request.CompetitionId, request.DivisionId, request.TeamId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team {TeamId} from season {SeasonId} division {DivisionId}", request.TeamId, request.CompetitionId, request.DivisionId);
            return Result.Failure("Failed to remove team from season division.");
        }
    }
}


