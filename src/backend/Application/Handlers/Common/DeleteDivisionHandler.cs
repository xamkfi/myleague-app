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
/// Handler for deleting a division
/// </summary>
public class DeleteDivisionHandler : IRequestHandler<DeleteDivisionCommand, Result<bool>>
{
    private readonly IDivisionRepository _divisionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteDivisionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteDivisionHandler class
    /// </summary>
    /// <param name="divisionRepository">The division repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public DeleteDivisionHandler(IDivisionRepository divisionRepository, IUnitOfWork unitOfWork, ILogger<DeleteDivisionHandler> logger)
    {
        _divisionRepository = divisionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteDivisionCommand request
    /// </summary>
    /// <param name="request">The command containing division ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion succeeded, wrapped in a Result</returns>
    public async Task<Result<bool>> Handle(DeleteDivisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Division? division = await _divisionRepository.GetByIdAsync(request.Id);
            if (division == null)
            {
                _logger.LogWarning("Division with ID {DivisionId} not found for deletion", request.Id);
                return Result<bool>.Failure($"Division with ID {request.Id} not found.");
            }

            // Note: In a real application, you might want to check if the division is being used
            // by any teams or seasons before allowing deletion. For now, we'll allow it.

            _logger.LogInformation("Deleting division: {DivisionId}", division.Id);
            await _divisionRepository.DeleteAsync(division);
            
            // Save changes explicitly
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted division with ID: {DivisionId}", division.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting division: {DivisionId}", request.Id);
            return Result<bool>.Failure("An error occurred while deleting the division.");
        }
    }
} 