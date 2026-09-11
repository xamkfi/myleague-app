using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Common.Divisions.Commands;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Divisions.Handlers;

/// <summary>
/// Handler for deleting a division that is not referenced by any team.
/// </summary>
public class DeleteDivisionHandler : IRequestHandler<DeleteDivisionCommand, Result<bool>>
{
    private readonly IDivisionRepository _divisionRepository;
    private readonly IFloorballTeamRepository _floorballTeamRepository;
    private readonly IFootballTeamRepository _footballTeamRepository;
    private readonly IHockeyTeamRepository _hockeyTeamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteDivisionHandler> _logger;

    public DeleteDivisionHandler(
        IDivisionRepository divisionRepository,
        IFloorballTeamRepository floorballTeamRepository,
        IFootballTeamRepository footballTeamRepository,
        IHockeyTeamRepository hockeyTeamRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteDivisionHandler> logger)
    {
        _divisionRepository = divisionRepository;
        _floorballTeamRepository = floorballTeamRepository;
        _footballTeamRepository = footballTeamRepository;
        _hockeyTeamRepository = hockeyTeamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteDivisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Division? division = await _divisionRepository.GetByIdAsync(request.Id);
            if (division == null)
            {
                _logger.LogWarning("Division with ID {DivisionId} not found for deletion", request.Id);
                return Result<bool>.NotFound("Division", request.Id);
            }

            bool hasTeams =
                await _floorballTeamRepository.HasAnyForDivisionAsync(request.Id, cancellationToken)
                || await _footballTeamRepository.HasAnyForDivisionAsync(request.Id, cancellationToken)
                || await _hockeyTeamRepository.HasAnyForDivisionAsync(request.Id, cancellationToken);
            if (hasTeams)
            {
                _logger.LogWarning("Blocked division delete for {DivisionId}: teams still use it", request.Id);
                return Result<bool>.Failure(DeletionReasons.DivisionHasTeams);
            }

            _logger.LogInformation("Deleting division: {DivisionId}", division.Id);
            await _divisionRepository.DeleteAsync(division);
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
