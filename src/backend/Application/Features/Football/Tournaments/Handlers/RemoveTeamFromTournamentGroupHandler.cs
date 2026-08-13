using Application.Features.Football.Tournaments.Commands;
using Application.Features.Football.Tournaments.DTOs;
using Application.Features.Football.Tournaments.Mappings;
using Application.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Entities.Football.Statistics;
using Domain.Repositories.Football;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Football.Tournaments.Handlers;

/// <summary>
/// Handler for removing a team from a tournament group
/// </summary>
public class RemoveTeamFromTournamentGroupHandler : IRequestHandler<RemoveTeamFromTournamentGroupCommand, Result<FootballTournamentDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveTeamFromTournamentGroupHandler> _logger;

    public RemoveTeamFromTournamentGroupHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<RemoveTeamFromTournamentGroupHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTournamentDto>> Handle(RemoveTeamFromTournamentGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Mirror the Add handler: load with AsNoTracking to avoid TPH/owned-type change tracking
            // marking the parent tournament Modified during SaveChanges.
            FootballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsNoTrackingAsync(request.CompetitionId, cancellationToken);
            if (tournament == null)
            {
                _logger.LogWarning("Tournament not found with ID: {TournamentId}", request.CompetitionId);
                return Result<FootballTournamentDto>.NotFound("FootballTournament", request.CompetitionId);
            }

            FootballTournamentGroup? group = tournament.GetGroup(request.GroupId);
            if (group == null)
            {
                _logger.LogWarning("Group not found with ID: {GroupId} in tournament: {TournamentId}", request.GroupId, request.CompetitionId);
                return Result<FootballTournamentDto>.NotFound("FootballTournamentGroup", request.GroupId);
            }

            FootballTournamentGroupTeam? joinToRemove = group.Teams.FirstOrDefault(t => t.TeamId == request.TeamId);
            if (joinToRemove == null)
            {
                _logger.LogInformation("Team {TeamId} not in group {GroupId}, nothing to remove", request.TeamId, request.GroupId);
                return Result<FootballTournamentDto>.Success(FootballTournamentMapper.ToDto(tournament));
            }

            _logger.LogInformation("Removing team {TeamId} from group {GroupId} in tournament: {TournamentId}", request.TeamId, request.GroupId, request.CompetitionId);
            await _tournamentRepository.RemoveGroupTeamAsync(joinToRemove, cancellationToken);

            // If this was the team's last group in the tournament, also drop the parent
            // FootballCompetitionTeam row so the inherited FootballCompetition.Teams collection
            // stays consistent. Teams kept in any other group should retain the parent row.
            bool stillInAnotherGroup = tournament.Groups
                .Where(g => g.Id != group.Id)
                .Any(g => g.Teams.Any(t => t.TeamId == request.TeamId));
            if (!stillInAnotherGroup)
            {
                await _tournamentRepository.RemoveCompetitionTeamAsync(request.CompetitionId, request.TeamId, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballTournament? refreshed = await _tournamentRepository.GetByIdWithGroupsAsNoTrackingAsync(request.CompetitionId, cancellationToken);
            FootballTournamentDto tournamentDto = FootballTournamentMapper.ToDto(refreshed ?? tournament);
            _logger.LogInformation("Successfully removed team {TeamId} from group {GroupId} in tournament: {TournamentId}", request.TeamId, request.GroupId, request.CompetitionId);

            return Result<FootballTournamentDto>.Success(tournamentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while removing team from tournament group: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing team {TeamId} from group {GroupId} in tournament: {TournamentId}", request.TeamId, request.GroupId, request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(
                "An error occurred while removing the team from the tournament group.",
                ex.Flatten());
        }
    }
}
