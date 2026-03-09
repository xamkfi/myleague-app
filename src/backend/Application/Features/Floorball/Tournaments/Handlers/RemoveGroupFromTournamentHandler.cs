using Application.Features.Floorball.Tournaments.Commands;
using Application.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Handlers;

public class RemoveGroupFromTournamentHandler : IRequestHandler<RemoveGroupFromTournamentCommand, Result>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballTournamentGroupRepository _groupRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveGroupFromTournamentHandler> _logger;

    public RemoveGroupFromTournamentHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballTournamentGroupRepository groupRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<RemoveGroupFromTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveGroupFromTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool tournamentExists = await _tournamentRepository.ExistsAsync(request.TournamentId);
            if (!tournamentExists)
            {
                _logger.LogWarning("Attempt to remove group from non-existent tournament {TournamentId}", request.TournamentId);
                return Result.NotFound("FloorballTournament", request.TournamentId);
            }

            bool groupExists = await _groupRepository.ExistsAsync(request.GroupId);
            if (!groupExists)
            {
                _logger.LogWarning("Group {GroupId} not found", request.GroupId);
                return Result.NotFound("FloorballTournamentGroup", request.GroupId);
            }

            await _groupRepository.DeleteAsync(request.GroupId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed group {GroupId} from tournament {TournamentId}", request.GroupId, request.TournamentId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing group {GroupId} from tournament {TournamentId}", request.GroupId, request.TournamentId);
            return Result.Failure("An error occurred while removing the group from the tournament.");
        }
    }
}
