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
/// Handler for adding a team to a tournament group
/// </summary>
public class AddTeamToTournamentGroupHandler : IRequestHandler<AddTeamToTournamentGroupCommand, Result<FootballTournamentDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<AddTeamToTournamentGroupHandler> _logger;

    public AddTeamToTournamentGroupHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballTeamRepository teamRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<AddTeamToTournamentGroupHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTournamentDto>> Handle(AddTeamToTournamentGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Load with AsNoTracking so EF Core's TPH + owned-type change detection cannot mark the
            // parent FootballTournament/Group rows as Modified spuriously and trigger a
            // DbUpdateConcurrencyException on SaveChanges. The parent aggregate is only used for
            // validation/lookup and idempotency checks here.
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

            FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FootballTournamentDto>.NotFound("FootballTeam", request.TeamId);
            }

            _logger.LogInformation("Adding team {TeamId} to group {GroupId} in tournament: {TournamentId}", request.TeamId, request.GroupId, request.CompetitionId);

            // Run domain rule via the (untracked) aggregate. AddTeam is idempotent: if the team is
            // already in the group it does nothing. Use a count-delta check to detect whether a new
            // join entity was actually created so we only persist real additions.
            int beforeCount = group.Teams.Count;
            group.AddTeam(team);
            FootballTournamentGroupTeam? newJoin = group.Teams.Count > beforeCount
                ? group.Teams.First(t => t.TeamId == team.Id)
                : null;

            if (newJoin != null)
            {
                await _tournamentRepository.AddGroupTeamAsync(newJoin, cancellationToken);

                // Keep the parent FootballCompetition.Teams collection (mapped from the shared
                // FootballCompetitionTeam join table) in sync. Without this row the team is not
                // considered part of the tournament for the purposes of FootballCompetition.AddMatch
                // and IFootballTeamRepository.GetByCompetitionIdAsync. The lookup is idempotent —
                // a team may belong to several groups but only requires one parent join row.
                bool alreadyOnTournament = tournament.Teams.Any(t => t.Id == team.Id)
                    || await _tournamentRepository.ExistsCompetitionTeamAsync(request.CompetitionId, team.Id, cancellationToken);
                if (!alreadyOnTournament)
                {
                    await _tournamentRepository.AddCompetitionTeamAsync(request.CompetitionId, team.Id, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                _logger.LogInformation("Team {TeamId} already in group {GroupId}, skipping persistence", request.TeamId, request.GroupId);
            }

            FootballTournament? refreshed = await _tournamentRepository.GetByIdWithGroupsAsNoTrackingAsync(request.CompetitionId, cancellationToken);
            FootballTournamentDto tournamentDto = FootballTournamentMapper.ToDto(refreshed ?? tournament);
            _logger.LogInformation("Successfully added team {TeamId} to group {GroupId} in tournament: {TournamentId}", request.TeamId, request.GroupId, request.CompetitionId);

            return Result<FootballTournamentDto>.Success(tournamentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while adding team to tournament group: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding team {TeamId} to group {GroupId} in tournament: {TournamentId}", request.TeamId, request.GroupId, request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(
                "An error occurred while adding the team to the tournament group.",
                ex.Flatten());
        }
    }
}
