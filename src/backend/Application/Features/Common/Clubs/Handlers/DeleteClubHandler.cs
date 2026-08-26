using Application.Common;
using Application.Features.Common.Clubs.Commands;
using Application.Features.Common.Deletion;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Clubs.Handlers;

/// <summary>
/// Handler for deleting a club that has no teams in any sport.
/// </summary>
public class DeleteClubHandler : IRequestHandler<DeleteClubCommand, Result>
{
    private readonly IClubRepository _clubRepository;
    private readonly IFloorballTeamRepository _floorballTeamRepository;
    private readonly IFootballTeamRepository _footballTeamRepository;
    private readonly IHockeyTeamRepository _hockeyTeamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteClubHandler> _logger;

    public DeleteClubHandler(
        IClubRepository clubRepository,
        IFloorballTeamRepository floorballTeamRepository,
        IFootballTeamRepository footballTeamRepository,
        IHockeyTeamRepository hockeyTeamRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteClubHandler> logger)
    {
        _clubRepository = clubRepository;
        _floorballTeamRepository = floorballTeamRepository;
        _footballTeamRepository = footballTeamRepository;
        _hockeyTeamRepository = hockeyTeamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteClubCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool clubExists = await _clubRepository.ExistsAsync(request.ClubId);
            if (!clubExists)
            {
                _logger.LogWarning("Attempt to delete non-existent club with ID: {ClubId}", request.ClubId);
                return Result.NotFound("Club", request.ClubId);
            }

            bool hasTeams =
                await _floorballTeamRepository.HasAnyForClubAsync(request.ClubId, cancellationToken)
                || await _footballTeamRepository.HasAnyForClubAsync(request.ClubId, cancellationToken)
                || await _hockeyTeamRepository.HasAnyForClubAsync(request.ClubId, cancellationToken);
            if (hasTeams)
            {
                _logger.LogWarning("Blocked club delete for {ClubId}: club still has teams", request.ClubId);
                return Result.Failure(DeletionReasons.ClubHasTeams);
            }

            _logger.LogInformation("Deleting club with ID: {ClubId}", request.ClubId);
            await _clubRepository.DeleteAsync(request.ClubId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted club with ID: {ClubId}", request.ClubId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting club: {ClubId}", request.ClubId);
            return Result.Failure("An error occurred while deleting the club.");
        }
    }
}
