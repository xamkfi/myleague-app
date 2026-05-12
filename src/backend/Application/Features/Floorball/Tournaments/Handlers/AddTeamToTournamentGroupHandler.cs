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
/// Handler for adding a team to a tournament group
/// </summary>
public class AddTeamToTournamentGroupHandler : IRequestHandler<AddTeamToTournamentGroupCommand, Result<FloorballTournamentDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<AddTeamToTournamentGroupHandler> _logger;

    public AddTeamToTournamentGroupHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<AddTeamToTournamentGroupHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTournamentDto>> Handle(AddTeamToTournamentGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Load with AsNoTracking so EF Core's TPH + owned-type change detection cannot mark the
            // parent FloorballTournament/Group rows as Modified spuriously and trigger a
            // DbUpdateConcurrencyException on SaveChanges. The parent aggregate is only used for
            // validation/lookup and idempotency checks here.
            FloorballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsNoTrackingAsync(request.CompetitionId, cancellationToken);
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

            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FloorballTournamentDto>.NotFound("FloorballTeam", request.TeamId);
            }

            _logger.LogInformation("Adding team {TeamId} to group {GroupId} in tournament: {TournamentId}", request.TeamId, request.GroupId, request.CompetitionId);

            // Run domain rule via the (untracked) aggregate. AddTeam is idempotent: if the team is
            // already in the group it does nothing. Use a count-delta check to detect whether a new
            // join entity was actually created so we only persist real additions.
            int beforeCount = group.Teams.Count;
            group.AddTeam(team);
            FloorballTournamentGroupTeam? newJoin = group.Teams.Count > beforeCount
                ? group.Teams.First(t => t.TeamId == team.Id)
                : null;

            if (newJoin != null)
            {
                await _tournamentRepository.AddGroupTeamAsync(newJoin, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                _logger.LogInformation("Team {TeamId} already in group {GroupId}, skipping persistence", request.TeamId, request.GroupId);
            }

            FloorballTournament? refreshed = await _tournamentRepository.GetByIdWithGroupsAsNoTrackingAsync(request.CompetitionId, cancellationToken);
            FloorballTournamentDto tournamentDto = FloorballTournamentMapper.ToDto(refreshed ?? tournament);
            _logger.LogInformation("Successfully added team {TeamId} to group {GroupId} in tournament: {TournamentId}", request.TeamId, request.GroupId, request.CompetitionId);

            return Result<FloorballTournamentDto>.Success(tournamentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while adding team to tournament group: {TournamentId}", request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding team {TeamId} to group {GroupId} in tournament: {TournamentId}", request.TeamId, request.GroupId, request.CompetitionId);
            return Result<FloorballTournamentDto>.Failure(
                "An error occurred while adding the team to the tournament group.",
                ex.Flatten());
        }
    }
}
