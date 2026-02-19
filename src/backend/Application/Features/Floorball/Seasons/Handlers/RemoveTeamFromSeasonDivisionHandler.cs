using Application.Commands.Floorball.Season;
using Application.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler to remove a team from a season division
/// </summary>
public class RemoveTeamFromSeasonDivisionHandler : IRequestHandler<RemoveTeamFromSeasonDivisionCommand, Result>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballSeasonDivisionRepository _seasonDivisionRepository;
    private readonly ILogger<RemoveTeamFromSeasonDivisionHandler> _logger;

    public RemoveTeamFromSeasonDivisionHandler(
        IFloorballSeasonRepository seasonRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
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
            if (!await _seasonRepository.ExistsAsync(request.SeasonId))
            {
                return Result.NotFound("FloorballSeason", request.SeasonId);
            }
            if (!await _teamRepository.ExistsAsync(request.TeamId))
            {
                return Result.NotFound("FloorballTeam", request.TeamId);
            }

            _logger.LogInformation("Removing team {TeamId} from season {SeasonId} division {DivisionId}", request.TeamId, request.SeasonId, request.DivisionId);
            await _seasonDivisionRepository.RemoveTeamFromSeasonDivisionAsync(request.SeasonId, request.DivisionId, request.TeamId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team {TeamId} from season {SeasonId} division {DivisionId}", request.TeamId, request.SeasonId, request.DivisionId);
            return Result.Failure("Failed to remove team from season division.");
        }
    }
}


