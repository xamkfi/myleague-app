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
/// Handler for creating a new division
/// </summary>
public class CreateDivisionHandler : IRequestHandler<CreateDivisionCommand, Result<DivisionDto>>
{
    private readonly IDivisionRepository _divisionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateDivisionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateDivisionHandler class
    /// </summary>
    /// <param name="divisionRepository">The division repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateDivisionHandler(IDivisionRepository divisionRepository, IUnitOfWork unitOfWork, ILogger<CreateDivisionHandler> logger)
    {
        _divisionRepository = divisionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateDivisionCommand request
    /// </summary>
    /// <param name="request">The command containing division information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created division as a DTO wrapped in a Result</returns>
    public async Task<Result<DivisionDto>> Handle(CreateDivisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if a division with the same name and sport type already exists
            if (await _divisionRepository.ExistsAsync(request.Name, request.SportType))
            {
                _logger.LogWarning("Attempt to create division with existing name and sport type: {DivisionName} for {SportType}", request.Name, request.SportType);
                return Result<DivisionDto>.Failure($"A division with the name '{request.Name}' already exists for {request.SportType}.");
            }

            // Create the division entity
            Division division = DivisionMapper.ToEntity(request);

            _logger.LogInformation("Creating new division: {DivisionName} for {SportType}", division.Name, division.SportType);
            await _divisionRepository.AddAsync(division);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            DivisionDto divisionDto = DivisionMapper.ToDto(division);
            _logger.LogInformation("Successfully created division with ID: {DivisionId}", division.Id);

            return Result<DivisionDto>.Success(divisionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating division: {DivisionName} for {SportType}", request.Name, request.SportType);
            return Result<DivisionDto>.Failure("An error occurred while creating the division.");
        }
    }
} 
