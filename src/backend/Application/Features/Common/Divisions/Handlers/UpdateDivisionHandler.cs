using Application.Features.Common.Divisions.Commands;
using Application.Features.Common.MatchTimer.Commands;
using Application.Features.Common.Images.Commands;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Common.Divisions.Handlers;

/// <summary>
/// Handler for updating an existing division
/// </summary>
public class UpdateDivisionHandler : IRequestHandler<UpdateDivisionCommand, Result<DivisionDto>>
{
    private readonly IDivisionRepository _divisionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateDivisionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateDivisionHandler class
    /// </summary>
    /// <param name="divisionRepository">The division repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateDivisionHandler(IDivisionRepository divisionRepository, IUnitOfWork unitOfWork, ILogger<UpdateDivisionHandler> logger)
    {
        _divisionRepository = divisionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateDivisionCommand request
    /// </summary>
    /// <param name="request">The command containing updated division information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated division as a DTO wrapped in a Result</returns>
    public async Task<Result<DivisionDto>> Handle(UpdateDivisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Division? division = await _divisionRepository.GetByIdAsync(request.Id);
            if (division == null)
            {
                _logger.LogWarning("Division with ID {DivisionId} not found for update", request.Id);
                return Result<DivisionDto>.Failure($"Division with ID {request.Id} not found.");
            }

            // Check if another division with the same name and sport type already exists
            Division? existingDivision = await _divisionRepository.GetByNameAndSportTypeAsync(request.Name, division.SportType);
            if (existingDivision != null && existingDivision.Id != request.Id)
            {
                _logger.LogWarning("Attempt to update division to existing name: {DivisionName} for {SportType}", request.Name, division.SportType);
                return Result<DivisionDto>.Failure($"A division with the name '{request.Name}' already exists for {division.SportType}.");
            }

            // Update the division using the mapper
            DivisionMapper.UpdateFromCommand(division, request);

            _logger.LogInformation("Updating division: {DivisionId}", division.Id);
            await _divisionRepository.UpdateAsync(division);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            DivisionDto divisionDto = DivisionMapper.ToDto(division);
            _logger.LogInformation("Successfully updated division with ID: {DivisionId}", division.Id);

            return Result<DivisionDto>.Success(divisionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating division: {DivisionId}", request.Id);
            return Result<DivisionDto>.Failure("An error occurred while updating the division.");
        }
    }
} 
