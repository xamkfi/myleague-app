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

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for completing a floorball season
/// </summary>
public class CompleteFloorballSeasonHandler : IRequestHandler<CompleteFloorballSeasonCommand, Result<FloorballSeasonDto>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballSeasonDivisionRepository _seasonDivisionRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteFloorballSeasonHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CompleteFloorballSeasonHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="seasonDivisionRepository">The floorball season division repository</param>
    /// <param name="unitOfWork">The floorball unit of work</param>
    /// <param name="logger">The logger</param>
    public CompleteFloorballSeasonHandler(
        IFloorballSeasonRepository seasonRepository,
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<CompleteFloorballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CompleteFloorballSeasonCommand request
    /// </summary>
    /// <param name="request">The command containing season ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The completed season as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballSeasonDto>> Handle(CompleteFloorballSeasonCommand request, CancellationToken cancellationToken)
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

            _logger.LogInformation("Completing floorball season: {SeasonId}", request.Id);
            season.Complete();
            
            // Update the season in the repository to track changes
            await _seasonRepository.UpdateAsync(season);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballSeasonDto seasonDto = await FloorballSeasonMapper.ToDtoAsync(season, _seasonDivisionRepository);
            _logger.LogInformation("Successfully completed floorball season: {SeasonId}", request.Id);

            return Result<FloorballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing floorball season: {SeasonId}", request.Id);
            return Result<FloorballSeasonDto>.Failure("An error occurred while completing the season.");
        }
    }
} 
