using Application.Features.Floorball.Seasons.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Floorball.Seasons.Handlers;

/// <summary>
/// Handler for adding a team to a floorball season
/// </summary>
public class AddTeamToSeasonHandler : IRequestHandler<AddTeamToSeasonCommand, Result<FloorballSeasonDto>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballSeasonDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly IFloorballStatisticsRepository _floorballStatisticsRepository;
    private readonly ILogger<AddTeamToSeasonHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the AddTeamToSeasonHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="seasonDivisionRepository">The floorball season division repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="floorballUnitOfWork">The floorball unit of work</param>
    /// <param name="floorballStatisticsRepository">The floorball statistics repository</param>
    /// <param name="logger">The logger</param>
    public AddTeamToSeasonHandler(
        IFloorballSeasonRepository seasonRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        IFloorballUnitOfWork floorballUnitOfWork,
        IFloorballStatisticsRepository floorballStatisticsRepository,
        ILogger<AddTeamToSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _teamRepository = teamRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
        _floorballUnitOfWork = floorballUnitOfWork;
        _floorballStatisticsRepository = floorballStatisticsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the AddTeamToSeasonCommand request
    /// </summary>
    /// <param name="request">The command containing season and team information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated season as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballSeasonDto>> Handle(AddTeamToSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the season
            FloorballSeason? season = await _seasonRepository.GetByIdAsync(request.SeasonId);
            if (season == null)
            {
                _logger.LogWarning("Season not found with ID: {SeasonId}", request.SeasonId);
                return Result<FloorballSeasonDto>.NotFound("Season with ID {SeasonId} not found.", request.SeasonId);
            }

            // Get the team
            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FloorballSeasonDto>.NotFound("Team with ID {TeamId} not found.", request.TeamId);
            }

            _logger.LogInformation("Adding team {TeamId} to season {SeasonId}", request.TeamId, request.SeasonId);
            
            // Use the domain method to add the team (includes business logic validation)
            season.AddTeam(team);

            //Initialize the player season statistics for the added team players
            List<FloorballPlayerSeasonStatistics> players = new List<FloorballPlayerSeasonStatistics>();
            foreach (FloorballTeamPlayer player in team.Roster)
            {
                FloorballPlayerSeasonStatistics playerSeasonStatistics = new FloorballPlayerSeasonStatistics(player.PlayerId, request.TeamId, request.SeasonId);
                players.Add(playerSeasonStatistics);
            }
            await _floorballStatisticsRepository.SavePlayerSeasonStatisticsBatchAsync(players, cancellationToken);

            // Save changes explicitly to trigger domain events
            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);

            // Load clubs for all teams in the season for the DTO mapping
            Dictionary<Guid, Club> clubsDict = new Dictionary<Guid, Club>();
            foreach (FloorballTeam seasonTeam in season.Teams)
            {
                Club? club = await _clubRepository.GetByIdAsync(seasonTeam.ClubId);
                if (club != null)
                {
                    clubsDict[seasonTeam.ClubId] = club;
                }
            }

            IEnumerable<FloorballSeasonDivision> seasonDivisions = await _seasonDivisionRepository.GetSeasonDivisionsAsync(season.Id);
            IReadOnlyCollection<FloorballSeasonDivisionDto> seasonDivisionDtos = FloorballSeasonMapper.ToDivisionDtos(seasonDivisions);
            FloorballSeasonDto seasonDto = FloorballSeasonMapper.ToDto(season, seasonDivisionDtos, clubsDict);
            _logger.LogInformation("Successfully added team {TeamId} to season {SeasonId}", request.TeamId, request.SeasonId);

            return Result<FloorballSeasonDto>.Success(seasonDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while adding team {TeamId} to season {SeasonId}", request.TeamId, request.SeasonId);
            return Result<FloorballSeasonDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding team {TeamId} to season {SeasonId}", request.TeamId, request.SeasonId);
            return Result<FloorballSeasonDto>.Failure("An error occurred while adding the team to the season.");
        }
    }
} 
