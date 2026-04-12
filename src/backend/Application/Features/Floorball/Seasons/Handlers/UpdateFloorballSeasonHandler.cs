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
using System.Linq;

namespace Application.Features.Floorball.Seasons.Handlers;

/// <summary>
/// Handler for updating an existing floorball season
/// </summary>
public class UpdateFloorballSeasonHandler : IRequestHandler<UpdateFloorballSeasonCommand, Result<FloorballSeasonDto>>
{
    private readonly IFloorballCompetitionRepository _seasonRepository;
    private readonly IFloorballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFloorballSeasonHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFloorballSeasonHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="seasonDivisionRepository">The floorball season division repository</param>
    /// <param name="unitOfWork">The floorball unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFloorballSeasonHandler(
        IFloorballCompetitionRepository seasonRepository,
        IFloorballCompetitionDivisionRepository seasonDivisionRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<UpdateFloorballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
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
            FloorballCompetition? existingSeason = await _seasonRepository.GetByIdAsync(request.Id);
            if (existingSeason == null)
            {
                _logger.LogWarning("Attempt to update non-existent floorball season with ID: {SeasonId}", request.Id);
                return Result<FloorballSeasonDto>.NotFound("FloorballSeason", request.Id);
            }

            // Update the season
            FloorballSeasonMapper.UpdateFromCommand(existingSeason, request);
            
            _logger.LogInformation("Updating floorball season: {SeasonId}", existingSeason.Id);
            await _seasonRepository.UpdateAsync(existingSeason);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            IEnumerable<FloorballCompetitionDivision> seasonDivisions = await _seasonDivisionRepository.GetCompetitionDivisionsAsync(existingSeason.Id);
            IReadOnlyCollection<FloorballSeasonDivisionDto> seasonDivisionDtos = FloorballSeasonMapper.ToDivisionDtos(seasonDivisions);

            FloorballSeasonDto seasonDto = FloorballSeasonMapper.ToDto(existingSeason, seasonDivisionDtos);
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
