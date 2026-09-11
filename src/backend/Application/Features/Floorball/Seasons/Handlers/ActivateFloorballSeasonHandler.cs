using Application.Features.Floorball.Seasons.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;
using Domain.Entities.Common;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Floorball.Seasons.Handlers;

/// <summary>
/// Handler for activating a floorball season
/// </summary>
public class ActivateFloorballSeasonHandler : IRequestHandler<ActivateFloorballSeasonCommand, Result<FloorballSeasonDto>>
{
    private readonly IFloorballCompetitionRepository _seasonRepository;
    private readonly IFloorballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly ILogger<ActivateFloorballSeasonHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the ActivateFloorballSeasonHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="seasonDivisionRepository">The floorball season division repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="floorballUnitOfWork">The floorball unit of work</param>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="logger">The logger</param>
    public ActivateFloorballSeasonHandler(
        IFloorballCompetitionRepository seasonRepository,
        IFloorballCompetitionDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        IUnitOfWork unitOfWork,
        IFloorballUnitOfWork floorballUnitOfWork,
        IFloorballStatisticsRepository statisticsRepository,
        ILogger<ActivateFloorballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _floorballUnitOfWork = floorballUnitOfWork;
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the ActivateFloorballSeasonCommand request
    /// </summary>
    /// <param name="request">The command containing season ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The activated season as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballSeasonDto>> Handle(ActivateFloorballSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the season
            FloorballCompetition? season = await _seasonRepository.GetByIdAsync(request.Id);
            if (season == null)
            {
                _logger.LogWarning("Season not found with ID: {SeasonId}", request.Id);
                return Result<FloorballSeasonDto>.NotFound("FloorballSeason", request.Id);
            }

            _logger.LogInformation("Activating floorball season: {SeasonId}", request.Id);
            season.Activate();

            //Initializes the statistics table for every team when season is activated
            foreach (FloorballTeam team in season.Teams)
            {
                FloorballTeamSeasonStatistics teamStatistics = new FloorballTeamSeasonStatistics(team.Id, request.Id);
                await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStatistics);
            }

            // Update the season in the repository to track changes
            await _seasonRepository.UpdateAsync(season);
            
            // Save changes explicitly to trigger domain events
            // Save FloorballDbContext changes first (for the season)
            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);
            // Then save CommonDbContext changes (for any club updates if needed)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Load clubs for all teams in the season
            Dictionary<Guid, Club> clubsDict = new Dictionary<Guid, Club>();
            foreach (FloorballTeam team in season.Teams)
            {
                Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
                if (club != null)
                {
                    clubsDict[team.ClubId] = club;
                }
            }

            IEnumerable<FloorballCompetitionDivision> seasonDivisions = await _seasonDivisionRepository.GetCompetitionDivisionsAsync(season.Id);
            IReadOnlyCollection<FloorballSeasonDivisionDto> seasonDivisionDtos = FloorballSeasonMapper.ToDivisionDtos(seasonDivisions);
            FloorballSeasonDto seasonDto = FloorballSeasonMapper.ToDto(season, seasonDivisionDtos, clubsDict);
            _logger.LogInformation("Successfully activated floorball season: {SeasonId}", request.Id);

            return Result<FloorballSeasonDto>.Success(seasonDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while activating floorball season: {SeasonId}", request.Id);
            return Result<FloorballSeasonDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating floorball season: {SeasonId}", request.Id);
            return Result<FloorballSeasonDto>.Failure("An error occurred while activating the season.");
        }
    }
} 
