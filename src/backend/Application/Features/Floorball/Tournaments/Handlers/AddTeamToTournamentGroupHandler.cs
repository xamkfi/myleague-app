using Application.Features.Floorball.Tournaments.Commands;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Floorball.Tournament;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Handlers;

public class AddTeamToTournamentGroupHandler : IRequestHandler<AddTeamToTournamentGroupCommand, Result<FloorballTournamentGroupTeamDto>>
{
    private readonly IFloorballTournamentGroupRepository _groupRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<AddTeamToTournamentGroupHandler> _logger;

    public AddTeamToTournamentGroupHandler(
        IFloorballTournamentGroupRepository groupRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<AddTeamToTournamentGroupHandler> logger)
    {
        _groupRepository = groupRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTournamentGroupTeamDto>> Handle(AddTeamToTournamentGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballTournamentGroup? group = await _groupRepository.GetByIdAsync(request.GroupId);
            if (group is null)
            {
                _logger.LogWarning("Group {GroupId} not found", request.GroupId);
                return Result<FloorballTournamentGroupTeamDto>.NotFound("FloorballTournamentGroup", request.GroupId);
            }

            if (group.TournamentId != request.TournamentId)
                return Result<FloorballTournamentGroupTeamDto>.Failure("Group does not belong to the specified tournament.");

            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                _logger.LogWarning("Team {TeamId} not found", request.TeamId);
                return Result<FloorballTournamentGroupTeamDto>.NotFound("FloorballTeam", request.TeamId);
            }

            FloorballTournamentGroupTeam membership = await _groupRepository.AddTeamToGroupAsync(
                request.GroupId, request.TeamId, request.TournamentId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added team {TeamId} to group {GroupId} in tournament {TournamentId}",
                request.TeamId, request.GroupId, request.TournamentId);

            FloorballTournamentGroupTeamDto dto = new(
                membership.Id,
                membership.GroupId,
                membership.TeamId,
                team.Name);

            return Result<FloorballTournamentGroupTeamDto>.Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Team {TeamId} already in group {GroupId}", request.TeamId, request.GroupId);
            return Result<FloorballTournamentGroupTeamDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding team {TeamId} to group {GroupId}", request.TeamId, request.GroupId);
            return Result<FloorballTournamentGroupTeamDto>.Failure("An error occurred while adding the team to the tournament group.");
        }
    }
}
