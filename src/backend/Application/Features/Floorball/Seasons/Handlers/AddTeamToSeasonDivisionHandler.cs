using Application.Features.Floorball.Seasons.Commands;
using Application.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Seasons.Handlers;

/// <summary>
/// Handler to add a team to a season division
/// </summary>
public class AddTeamToSeasonDivisionHandler : IRequestHandler<AddTeamToSeasonDivisionCommand, Result>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballSeasonDivisionRepository _seasonDivisionRepository;
    private readonly ILogger<AddTeamToSeasonDivisionHandler> _logger;

    public AddTeamToSeasonDivisionHandler(
        IFloorballSeasonRepository seasonRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
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
            if (!await _seasonRepository.ExistsAsync(request.SeasonId))
            {
                return Result.NotFound("FloorballSeason", request.SeasonId);
            }
            if (!await _teamRepository.ExistsAsync(request.TeamId))
            {
                return Result.NotFound("FloorballTeam", request.TeamId);
            }

            _logger.LogInformation("Adding team {TeamId} to season {SeasonId} division {DivisionId}", request.TeamId, request.SeasonId, request.DivisionId);
            await _seasonDivisionRepository.AddTeamToSeasonDivisionAsync(request.SeasonId, request.DivisionId, request.TeamId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding team {TeamId} to season {SeasonId} division {DivisionId}", request.TeamId, request.SeasonId, request.DivisionId);
            return Result.Failure("Failed to add team to season division.");
        }
    }
}


