using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Floorball.Teams.Commands;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Teams.Handlers;

/// <summary>
/// Handler for deleting a floorball team that is not used in matches.
/// </summary>
public class DeleteFloorballTeamHandler : IRequestHandler<DeleteFloorballTeamCommand, Result>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly ILogger<DeleteFloorballTeamHandler> _logger;

    public DeleteFloorballTeamHandler(
        IFloorballTeamRepository teamRepository,
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork floorballUnitOfWork,
        ILogger<DeleteFloorballTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _matchRepository = matchRepository;
        _floorballUnitOfWork = floorballUnitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFloorballTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool teamExists = await _teamRepository.ExistsAsync(request.Id);
            if (!teamExists)
            {
                _logger.LogWarning("Attempt to delete non-existent floorball team with ID: {TeamId}", request.Id);
                return Result.NotFound("FloorballTeam", request.Id);
            }

            if (await _matchRepository.HasAnyForTeamAsync(request.Id, cancellationToken))
            {
                _logger.LogWarning("Blocked floorball team delete for {TeamId}: team is used in matches", request.Id);
                return Result.Failure(DeletionReasons.TeamUsedInMatches);
            }

            _logger.LogInformation("Deleting floorball team with ID: {TeamId}", request.Id);
            await _teamRepository.DeleteAsync(request.Id);
            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted floorball team with ID: {TeamId}", request.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting floorball team: {TeamId}", request.Id);
            return Result.Failure("An error occurred while deleting the floorball team.");
        }
    }
}
