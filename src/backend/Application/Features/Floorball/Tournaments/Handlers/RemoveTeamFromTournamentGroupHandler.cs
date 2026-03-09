using Application.Features.Floorball.Tournaments.Commands;
using Application.Common;
using Domain.Entities.Floorball.Tournament;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Handlers;

public class RemoveTeamFromTournamentGroupHandler : IRequestHandler<RemoveTeamFromTournamentGroupCommand, Result>
{
    private readonly IFloorballTournamentGroupRepository _groupRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveTeamFromTournamentGroupHandler> _logger;

    public RemoveTeamFromTournamentGroupHandler(
        IFloorballTournamentGroupRepository groupRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<RemoveTeamFromTournamentGroupHandler> logger)
    {
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveTeamFromTournamentGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballTournamentGroup? group = await _groupRepository.GetByIdAsync(request.GroupId);
            if (group is null)
            {
                _logger.LogWarning("Group {GroupId} not found", request.GroupId);
                return Result.NotFound("FloorballTournamentGroup", request.GroupId);
            }

            if (group.TournamentId != request.TournamentId)
                return Result.Failure("Group does not belong to the specified tournament.");

            await _groupRepository.RemoveTeamFromGroupAsync(request.GroupId, request.TeamId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed team {TeamId} from group {GroupId} in tournament {TournamentId}",
                request.TeamId, request.GroupId, request.TournamentId);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Team {TeamId} not in group {GroupId}", request.TeamId, request.GroupId);
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team {TeamId} from group {GroupId}", request.TeamId, request.GroupId);
            return Result.Failure("An error occurred while removing the team from the tournament group.");
        }
    }
}
