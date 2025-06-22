using Application.Commands.Floorball;
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
using Application.Commands.Floorball.Season;
using System.Linq;

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for creating a new floorball season
/// </summary>
public class CreateFloorballSeasonHandler : IRequestHandler<CreateFloorballSeasonCommand, Result<FloorballSeasonDto>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballSeasonHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFloorballSeasonHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="unitOfWork">The floorball unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFloorballSeasonHandler(
        IFloorballSeasonRepository seasonRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<CreateFloorballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFloorballSeasonCommand request
    /// </summary>
    /// <param name="request">The command containing season information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created season as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballSeasonDto>> Handle(CreateFloorballSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify no overlapping season exists
            IEnumerable<FloorballSeason> existingSeasons = await _seasonRepository.GetAllAsync();
            bool overlappingSeasonExists = existingSeasons.Any(s => 
                (request.StartDate < s.EndDate && request.EndDate > s.StartDate));
            
            if (overlappingSeasonExists)
            {
                _logger.LogWarning("Attempt to create season with overlapping dates: {StartDate} - {EndDate}", 
                    request.StartDate, request.EndDate);
                return Result<FloorballSeasonDto>.Failure("A season already exists that overlaps with the specified dates.");
            }

            // Create the season entity
            FloorballSeason season = FloorballSeasonMapper.ToEntity(request);

            _logger.LogInformation("Creating new floorball season: {Name}", request.Name);
            await _seasonRepository.AddAsync(season);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballSeasonDto seasonDto = FloorballSeasonMapper.ToDto(season);
            _logger.LogInformation("Successfully created floorball season with ID: {SeasonId}", season.Id);

            return Result<FloorballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating floorball season: {Name}", request.Name);
            return Result<FloorballSeasonDto>.Failure("An error occurred while creating the floorball season.");
        }
    }
} 
