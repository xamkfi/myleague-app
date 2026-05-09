using Application.Features.Floorball.Tournaments.Commands;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Tournaments.Handlers;

/// <summary>
/// Handler for removing a team from a tournament group
/// </summary>
public class RemoveTeamFromTournamentGroupHandler : IRequestHandler<RemoveTeamFromTournamentGroupCommand, Result<FloorballTournamentDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveTeamFromTournamentGroupHandler> _logger;

    public RemoveTeamFromTournamentGroupHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<RemoveTeamFromTournamentGroupHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTournamentDto>> Handle(RemoveTeamFromTournamentGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId);
            if (tournament == null)
            {
                _logger.LogWarning("Tournament not found with ID: {TournamentId}", request.CompetitionId);
                return Result<FloorballTournamentDto>.NotFound("FloorballTournament", request.CompetitionId);
            }

            FloorballTournamentGroup? group = tournament.GetGroup(request.GroupId);
            if (group == null)
            {
                _logger.LogWarning("Group not found with ID: {GroupId} in tournament: {TournamentId}", request.GroupId, request.CompetitionId);
                return Result<FloorballTournamentDto>.NotFound("FloorballTournamentGroup", request.GroupId);
            }

            _logger.LogInformation("Removing team {TeamId} from group {GroupId} in tournament: {TournamentId}", request.TeamId, request.GroupId, request.CompetitionId);
            group.RemoveTeam(request.TeamId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTournamentDto tournamentDto = FloorballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully removed team {TeamId} from group {GroupId} in tournament: {TournamentId}", request.TeamId, request.GroupId, request.CompetitionId);

            return Result<FloorballTournamentDto>.Success(tournamentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while removing team from tournament group: {TournamentId}", request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing team {TeamId} from group {GroupId} in tournament: {TournamentId}", request.TeamId, request.GroupId, request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure(
                "An error occurred while removing the team from the tournament group.",
                ex.Flatten());
        }
    }
}
