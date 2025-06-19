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
/// Handler for deactivating a division
/// </summary>
public class DeactivateDivisionHandler : IRequestHandler<DeactivateDivisionCommand, Result<bool>>
{
    private readonly IDivisionRepository _divisionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateDivisionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeactivateDivisionHandler class
    /// </summary>
    /// <param name="divisionRepository">The division repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public DeactivateDivisionHandler(IDivisionRepository divisionRepository, IUnitOfWork unitOfWork, ILogger<DeactivateDivisionHandler> logger)
    {
        _divisionRepository = divisionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeactivateDivisionCommand request
    /// </summary>
    /// <param name="request">The command containing division ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deactivation succeeded, wrapped in a Result</returns>
    public async Task<Result<bool>> Handle(DeactivateDivisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Division? division = await _divisionRepository.GetByIdAsync(request.Id);
            if (division == null)
            {
                _logger.LogWarning("Division with ID {DivisionId} not found for deactivation", request.Id);
                return Result<bool>.Failure($"Division with ID {request.Id} not found.");
            }

            if (!division.IsActive)
            {
                _logger.LogInformation("Division {DivisionId} is already inactive", request.Id);
                return Result<bool>.Success(true);
            }

            // Deactivate the division
            division.Deactivate();

            _logger.LogInformation("Deactivating division: {DivisionId}", division.Id);
            await _divisionRepository.UpdateAsync(division);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deactivated division with ID: {DivisionId}", division.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating division: {DivisionId}", request.Id);
            return Result<bool>.Failure("An error occurred while deactivating the division.");
        }
    }
} 