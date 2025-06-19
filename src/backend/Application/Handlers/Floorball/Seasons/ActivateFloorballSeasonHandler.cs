using Application.Commands.Floorball.Season;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
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

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for activating a floorball season
/// </summary>
public class ActivateFloorballSeasonHandler : IRequestHandler<ActivateFloorballSeasonCommand, Result<FloorballSeasonDto>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly ILogger<ActivateFloorballSeasonHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the ActivateFloorballSeasonHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public ActivateFloorballSeasonHandler(
        IFloorballSeasonRepository seasonRepository,
        IClubRepository clubRepository,
        IUnitOfWork unitOfWork,
        IFloorballUnitOfWork floorballUnitOfWork,
        ILogger<ActivateFloorballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _floorballUnitOfWork = floorballUnitOfWork;
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
            FloorballSeason? season = await _seasonRepository.GetByIdAsync(request.Id);
            if (season == null)
            {
                _logger.LogWarning("Season not found with ID: {SeasonId}", request.Id);
                return Result<FloorballSeasonDto>.Failure($"Season with ID {request.Id} not found.");
            }

            _logger.LogInformation("Activating floorball season: {SeasonId}", request.Id);
            season.Activate();
            
            // Update the season in the repository to track changes
            await _seasonRepository.UpdateAsync(season);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);

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

            FloorballSeasonDto seasonDto = FloorballSeasonMapper.ToDto(season, clubsDict);
            _logger.LogInformation("Successfully activated floorball season: {SeasonId}", request.Id);

            return Result<FloorballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating floorball season: {SeasonId}", request.Id);
            return Result<FloorballSeasonDto>.Failure("An error occurred while activating the season.");
        }
    }
} 
