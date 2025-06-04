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
using Domain.Repositories.Common;
using System.Linq;

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for updating an existing floorball season
/// </summary>
public class UpdateFloorballSeasonHandler : IRequestHandler<UpdateFloorballSeasonCommand, Result<FloorballSeasonDto>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFloorballSeasonHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFloorballSeasonHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFloorballSeasonHandler(
        IFloorballSeasonRepository seasonRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateFloorballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFloorballSeasonCommand request
    /// </summary>
    /// <param name="request">The command containing updated season information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated season as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballSeasonDto>> Handle(UpdateFloorballSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the existing season
            FloorballSeason? existingSeason = await _seasonRepository.GetByIdAsync(request.Id);
            if (existingSeason == null)
            {
                _logger.LogWarning("Attempt to update non-existent floorball season with ID: {SeasonId}", request.Id);
                return Result<FloorballSeasonDto>.NotFound("FloorballSeason", request.Id);
            }

            // Check for overlapping seasons if dates are being updated
            if (request.StartDate != existingSeason.StartDate || request.EndDate != existingSeason.EndDate)
            {
                IEnumerable<FloorballSeason> allSeasons = await _seasonRepository.GetAllAsync();
                bool overlappingSeasonExists = allSeasons
                    .Where(s => s.Id != request.Id) // Exclude the current season being updated
                    .Any(s => (request.StartDate < s.EndDate && request.EndDate > s.StartDate));
                
                if (overlappingSeasonExists)
                {
                    _logger.LogWarning("Attempt to update season with overlapping dates: {StartDate} - {EndDate}", 
                        request.StartDate, request.EndDate);
                    return Result<FloorballSeasonDto>.Failure("A season already exists that overlaps with the specified dates.");
                }
            }

            // Update the season
            FloorballSeasonMapper.UpdateFromCommand(existingSeason, request);
            
            _logger.LogInformation("Updating floorball season: {SeasonId}", existingSeason.Id);
            await _seasonRepository.UpdateAsync(existingSeason);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballSeasonDto seasonDto = FloorballSeasonMapper.ToDto(existingSeason);
            _logger.LogInformation("Successfully updated floorball season with ID: {SeasonId}", existingSeason.Id);

            return Result<FloorballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating floorball season: {SeasonId}", request.Id);
            return Result<FloorballSeasonDto>.Failure("An error occurred while updating the floorball season.");
        }
    }
} 
