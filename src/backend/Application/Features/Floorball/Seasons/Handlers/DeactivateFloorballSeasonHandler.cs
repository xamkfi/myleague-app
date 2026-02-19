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
/// Handler for deactivating a floorball season
/// </summary>
public class DeactivateFloorballSeasonHandler : IRequestHandler<DeactivateFloorballSeasonCommand, Result<FloorballSeasonDto>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballSeasonDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly ILogger<DeactivateFloorballSeasonHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeactivateFloorballSeasonHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="seasonDivisionRepository">The floorball season division repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="floorballUnitOfWork">The floorball unit of work</param>
    /// <param name="logger">The logger</param>
    public DeactivateFloorballSeasonHandler(
        IFloorballSeasonRepository seasonRepository,
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        IUnitOfWork unitOfWork,
        IFloorballUnitOfWork floorballUnitOfWork,
        ILogger<DeactivateFloorballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _floorballUnitOfWork = floorballUnitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeactivateFloorballSeasonCommand request
    /// </summary>
    /// <param name="request">The command containing season ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deactivated season as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballSeasonDto>> Handle(DeactivateFloorballSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the season
            FloorballSeason? season = await _seasonRepository.GetByIdAsync(request.Id);
            if (season == null)
            {
                _logger.LogWarning("Season not found with ID: {SeasonId}", request.Id);
                return Result<FloorballSeasonDto>.Failure($"Season with ID {request.Id} not found.");
            }

            _logger.LogInformation("Deactivating floorball season: {SeasonId}", request.Id);
            season.Deactivate();
            
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

            IEnumerable<FloorballSeasonDivision> seasonDivisions = await _seasonDivisionRepository.GetSeasonDivisionsAsync(season.Id);
            IReadOnlyCollection<FloorballSeasonDivisionDto> seasonDivisionDtos = FloorballSeasonMapper.ToDivisionDtos(seasonDivisions);
            FloorballSeasonDto seasonDto = FloorballSeasonMapper.ToDto(season, seasonDivisionDtos, clubsDict);
            _logger.LogInformation("Successfully deactivated floorball season: {SeasonId}", request.Id);

            return Result<FloorballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating floorball season: {SeasonId}", request.Id);
            return Result<FloorballSeasonDto>.Failure("An error occurred while deactivating the season.");
        }
    }
} 
