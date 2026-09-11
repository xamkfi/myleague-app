using Application.Features.Football.Tournaments.Commands;
using Application.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Entities.Football.Statistics;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Football.Tournaments.Handlers;

/// <summary>
/// Handler for deleting a football tournament.
///
/// Delete semantics depend on the tournament's lifecycle status:
///
///   • <see cref="FootballTournamentStatus.Draft"/> — the tournament hasn't actually run yet, so
///     deletion is treated as "throw the whole draft away": matches, groups, group/team assignments
///     and the competition-team join rows are all wiped automatically. This makes botched JSON
///     imports easy to clean up without forcing the user to delete every match by hand.
///
///   • Any other status (GroupStage, PlayoffStage, Completed, Cancelled) — refuse the delete when
///     matches exist. Those statuses imply real game data (events, statistics, fan-facing history)
///     that an admin should consciously remove before discarding the tournament. The error message
///     directs them to delete the matches first, matching the original safety net.
/// </summary>
public class DeleteFootballTournamentHandler : IRequestHandler<DeleteFootballTournamentCommand, Result>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFootballTournamentHandler> _logger;

    public DeleteFootballTournamentHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<DeleteFootballTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFootballTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballTournament? tournament = await _tournamentRepository.GetByIdAsync(request.CompetitionId);
            if (tournament == null)
            {
                _logger.LogWarning("Attempt to delete non-existent football tournament with ID: {TournamentId}", request.CompetitionId);
                return Result.NotFound("FootballTournament", request.CompetitionId);
            }

            bool isDraft = tournament.TournamentStatus == FootballTournamentStatus.Draft;
            if (isDraft)
            {
                // Drafts cascade everything. We must remove matches BEFORE saving the tournament
                // delete because FootballMatch.TournamentGroupId is a RESTRICT FK — letting the
                // cascade from FootballCompetition → FootballTournamentGroup run while matches
                // still reference those groups would error out at the DB. The repository helper
                // also handles the orphan-prone FootballMatchTeamStatistics rows and the
                // self-referential NextMatchId column.
                int deletedMatches = await _matchRepository.DeleteAllByCompetitionIdAsync(request.CompetitionId, cancellationToken);
                if (deletedMatches > 0)
                {
                    _logger.LogInformation(
                        "Cascaded {DeletedMatches} match(es) for draft tournament {TournamentId}.",
                        deletedMatches,
                        request.CompetitionId);
                }
            }
            else
            {
                // Non-draft tournaments keep the old safety net: refuse to silently destroy
                // game-data-bearing matches.
                int matchCount = (await _matchRepository.GetByCompetitionIdAsync(request.CompetitionId)).Count();
                if (matchCount > 0)
                {
                    _logger.LogWarning(
                        "Refusing to delete tournament {TournamentId} in status {Status} because it still has {MatchCount} match(es).",
                        request.CompetitionId,
                        tournament.TournamentStatus,
                        matchCount);
                    return Result.Failure("Cannot delete a tournament that has matches. Delete the matches first.");
                }
            }

            _logger.LogInformation(
                "Deleting football tournament with ID: {TournamentId} (status: {Status})",
                request.CompetitionId,
                tournament.TournamentStatus);
            await _tournamentRepository.DeleteAsync(tournament, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted football tournament with ID: {TournamentId}", request.CompetitionId);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while deleting tournament: {TournamentId}", request.CompetitionId);
            return Result.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting football tournament: {TournamentId}", request.CompetitionId);
            return Result.Failure(
                "An error occurred while deleting the football tournament.",
                ex.Flatten());
        }
    }
}
