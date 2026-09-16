using Application.Common;
using Application.Features.Common.Shared;
using Application.Features.Floorball.Seasons.Commands;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Seasons.Handlers;

/// <summary>
/// Handler to add a team to a season division and optionally copy the latest roster.
/// </summary>
public class AddTeamToSeasonDivisionHandler : IRequestHandler<AddTeamToSeasonDivisionCommand, Result>
{
    private readonly IFloorballCompetitionRepository _seasonRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<AddTeamToSeasonDivisionHandler> _logger;

    public AddTeamToSeasonDivisionHandler(
        IFloorballCompetitionRepository seasonRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballCompetitionDivisionRepository seasonDivisionRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<AddTeamToSeasonDivisionHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _teamRepository = teamRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(AddTeamToSeasonDivisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballCompetition? season = await _seasonRepository.GetByIdAsync(request.CompetitionId);
            if (season == null)
                return Result.NotFound("FloorballSeason", request.CompetitionId);

            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
                return Result.NotFound("FloorballTeam", request.TeamId);

            _logger.LogInformation(
                "Adding team {TeamId} to season {SeasonId} division {DivisionId} ({RosterMode})",
                request.TeamId, request.CompetitionId, request.DivisionId, request.RosterMode);

            season.AddTeam(team);
            RosterEnrollment.Apply(team, request.CompetitionId, request.RosterMode);

            if (season.IsActive)
            {
                FloorballTeamSeasonStatistics teamStatistics = new FloorballTeamSeasonStatistics(team.Id, request.CompetitionId);
                await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStatistics, cancellationToken);
            }

            List<FloorballPlayerSeasonStatistics> playerStats = team.GetActiveRoster(request.CompetitionId)
                .Select(player => new FloorballPlayerSeasonStatistics(player.PlayerId, request.TeamId, request.CompetitionId))
                .ToList();
            if (playerStats.Count > 0)
                await _statisticsRepository.SavePlayerSeasonStatisticsBatchAsync(playerStats, cancellationToken);

            await _seasonDivisionRepository.AddTeamToCompetitionDivisionAsync(
                request.CompetitionId, request.DivisionId, request.TeamId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation adding team {TeamId} to season division", request.TeamId);
            return Result.Failure(ex.Message);
        }
    }
}
