using Application.Commands.Common;
using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Common;

/// <summary>
/// Handler for activating a division
/// </summary>
public class ActivateDivisionHandler : IRequestHandler<ActivateDivisionCommand, Result<bool>>
{
    private readonly IDivisionRepository _divisionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateDivisionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the ActivateDivisionHandler class
    /// </summary>
    /// <param name="divisionRepository">The division repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public ActivateDivisionHandler(IDivisionRepository divisionRepository, IUnitOfWork unitOfWork, ILogger<ActivateDivisionHandler> logger)
    {
        _divisionRepository = divisionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the ActivateDivisionCommand request
    /// </summary>
    /// <param name="request">The command containing division ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if activation succeeded, wrapped in a Result</returns>
    public async Task<Result<bool>> Handle(ActivateDivisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Division? division = await _divisionRepository.GetByIdAsync(request.Id);
            if (division == null)
            {
                _logger.LogWarning("Division with ID {DivisionId} not found for activation", request.Id);
                return Result<bool>.Failure($"Division with ID {request.Id} not found.");
            }

            if (division.IsActive)
            {
                _logger.LogInformation("Division {DivisionId} is already active", request.Id);
                return Result<bool>.Success(true);
            }

            // Activate the division
            division.Activate();

            _logger.LogInformation("Activating division: {DivisionId}", division.Id);
            await _divisionRepository.UpdateAsync(division);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully activated division with ID: {DivisionId}", division.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating division: {DivisionId}", request.Id);
            return Result<bool>.Failure("An error occurred while activating the division.");
        }
    }
} 