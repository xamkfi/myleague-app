using Application.Common;
using Application.Features.Football.Tournaments.Commands;
using Application.Features.Football.Tournaments.DTOs;
using Application.Features.Football.Tournaments.Mappings;
using Application.Features.Football.Tournaments.Services;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Entities.Football.Statistics;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Tournaments.Handlers;

/// <summary>
/// Handler for starting the playoff stage of a tournament.
///
/// Responsibilities:
///  1. Move the tournament from <see cref="FootballTournamentStatus.GroupStage"/> to <see cref="FootballTournamentStatus.PlayoffStage"/>.
///  2. Compute group standings using <see cref="TournamentStandingsCalculator"/> (same logic as the public standings table).
///  3. Build a cross-group seeded bracket using <see cref="PlayoffBracketBuilder"/>.
///  4. Persist the round-1 + downstream playoff matches with playoff-specific rules and forward references
///     (so the completion handler can advance winners automatically).
///
/// Idempotency:
///  - If the tournament is already in PlayoffStage and at least one playoff match exists, the existing
///    structure is returned (no duplicates created, no error).
/// </summary>
public class StartTournamentPlayoffStageHandler : IRequestHandler<StartTournamentPlayoffStageCommand, Result<FootballTournamentDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<StartTournamentPlayoffStageHandler> _logger;

    public StartTournamentPlayoffStageHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballMatchRepository matchRepository,
        IFootballTeamRepository teamRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<StartTournamentPlayoffStageHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTournamentDto>> Handle(StartTournamentPlayoffStageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.CompetitionId, cancellationToken);
            if (tournament == null)
            {
                _logger.LogWarning("Tournament not found with ID: {TournamentId}", request.CompetitionId);
                return Result<FootballTournamentDto>.NotFound("FootballTournament", request.CompetitionId);
            }

            // Pull all matches (group + any existing playoff) for this tournament.
            IEnumerable<FootballMatch> tournamentMatches = await _matchRepository.GetByCompetitionIdAsync(request.CompetitionId);
            List<FootballMatch> matchList = tournamentMatches.ToList();

            // Idempotency: bracket already generated -> no-op, return current state.
            bool hasPlayoffMatches = matchList.Any(m => m.PlayoffRound != null);
            if (tournament.TournamentStatus == FootballTournamentStatus.PlayoffStage && hasPlayoffMatches)
            {
                _logger.LogInformation("Playoff stage already started for tournament {TournamentId}, returning existing bracket", request.CompetitionId);
                return Result<FootballTournamentDto>.Success(FootballTournamentMapper.ToDto(tournament));
            }

            // Validate readiness (rules + completion + supported size).
            PlayoffBracketReadiness.Outcome readiness = PlayoffBracketReadiness.Evaluate(tournament, matchList);
            if (!readiness.IsReady)
            {
                _logger.LogWarning("Cannot start playoff stage for tournament {TournamentId}: {Error}", request.CompetitionId, readiness.Error);
                return Result<FootballTournamentDto>.Failure(readiness.Error ?? "Cannot start playoff stage.");
            }

            // Compute group standings and pick the top-N qualifying teams from each group.
            int teamsAdvancing = tournament.TournamentRules.TeamsAdvancingPerGroup;
            List<FootballTournamentGroup> orderedGroups = tournament.Groups.OrderBy(g => g.Order).ToList();

            List<IReadOnlyList<FootballTeam>> groupRankings = new List<IReadOnlyList<FootballTeam>>();
            foreach (FootballTournamentGroup group in orderedGroups)
            {
                IEnumerable<FootballMatch> groupCompleted = matchList.Where(m =>
                    m.TournamentGroupId == group.Id && m.Status == FootballMatchStatus.Completed);

                List<TournamentStandingsCalculator.StandingsRow> rows =
                    TournamentStandingsCalculator.Compute(group, groupCompleted);

                // Resolve each ranked row back into a FootballTeam entity (the rows have ids only).
                List<FootballTeam> rankedTeams = new List<FootballTeam>();
                foreach (TournamentStandingsCalculator.StandingsRow row in rows.Take(teamsAdvancing))
                {
                    FootballTeam? team = await _teamRepository.GetByIdAsync(row.TeamId);
                    if (team == null)
                    {
                        return Result<FootballTournamentDto>.Failure(
                            $"Cannot start playoff stage: team {row.TeamId} not found while computing standings for group '{group.Name}'.");
                    }
                    rankedTeams.Add(team);
                }
                groupRankings.Add(rankedTeams);
            }

            // Build seeds (cross-group order: A1, B1, A2, B2, ...).
            IReadOnlyList<PlayoffBracketBuilder.Seed> seeds =
                PlayoffBracketBuilder.BuildSeedList(groupRankings, teamsAdvancing);

            int expectedSeedCount = teamsAdvancing * orderedGroups.Count;
            if (seeds.Count < expectedSeedCount)
            {
                return Result<FootballTournamentDto>.Failure(
                    $"Cannot start playoff stage: not enough completed teams to fill the bracket (expected {expectedSeedCount}, got {seeds.Count}).");
            }

            IReadOnlyList<PlayoffBracketBuilder.PlannedMatch> plannedMatches =
                PlayoffBracketBuilder.Build(seeds, tournament.TournamentRules.HasThirdPlaceMatch);

            // Schedule rounds: spread them across the days following the tournament end (or now if past).
            // Used only as the fallback when no pre-defined slot exists for a given (round, order).
            DateTime baseDate = (tournament.EndDate < DateTime.UtcNow ? DateTime.UtcNow : tournament.EndDate)
                .ToUniversalTime()
                .Date
                .AddDays(1)
                .AddHours(16); // 16:00 UTC
            // Give every round its own day. Same-round matches share the day with 2 hour offsets.
            Dictionary<FootballPlayoffRound, int> roundDayOffsets = new Dictionary<FootballPlayoffRound, int>
            {
                { FootballPlayoffRound.QuarterFinal, 0 },
                { FootballPlayoffRound.SemiFinal, 1 },
                { FootballPlayoffRound.ThirdPlaceMatch, 2 },
                { FootballPlayoffRound.Final, 2 }
            };

            // Index any pre-defined playoff slots saved on the tournament (set via the import or
            // tournament edit form). Lookups are by (round, order) which is the same identity the
            // bracket builder uses for planned matches.
            Dictionary<(FootballPlayoffRound Round, int Order), FootballPlayoffScheduleSlot> scheduleSlots =
                tournament.PlayoffSchedule
                    .GroupBy(s => (s.Round, s.Order))
                    // Defensive: ignore duplicates that shouldn't exist (SetPlayoffSchedule blocks them).
                    .ToDictionary(g => g.Key, g => g.First());

            // Pre-populate each planned match's home/away with projected winners (best feeder seed).
            // The frontend renders these as "TBD" if the feeder hasn't been completed yet.
            FillProjectedTeams(plannedMatches);

            FootballTournament tournamentRef = tournament; // captured for closure clarity
            int sameRoundCounter = 0;
            FootballPlayoffRound currentRound = FootballPlayoffRound.None;
            List<FootballMatch> createdMatches = new List<FootballMatch>();

            foreach (PlayoffBracketBuilder.PlannedMatch planned in plannedMatches.OrderBy(m => (int)m.Round).ThenBy(m => m.Order))
            {
                if (planned.HomeTeam == null || planned.AwayTeam == null)
                {
                    return Result<FootballTournamentDto>.Failure(
                        "Internal error while generating playoff bracket: a planned match is missing teams.");
                }

                if (currentRound != planned.Round)
                {
                    currentRound = planned.Round;
                    sameRoundCounter = 0;
                }

                // Prefer the pre-defined slot (admin-controlled, matches the schedule shown to
                // end-users while the group stage was running). Fall back to auto-calculation
                // when no slot was registered for this bracket position.
                DateTime scheduled;
                string? scheduledVenue;
                if (scheduleSlots.TryGetValue((planned.Round, planned.Order), out FootballPlayoffScheduleSlot? slot))
                {
                    scheduled = slot.ScheduledDateTime;
                    scheduledVenue = slot.Venue ?? tournamentRef.Venue;
                }
                else
                {
                    int dayOffset = roundDayOffsets.TryGetValue(planned.Round, out int d) ? d : 0;
                    scheduled = baseDate.AddDays(dayOffset).AddHours(2d * sameRoundCounter);
                    scheduledVenue = tournamentRef.Venue;
                }
                sameRoundCounter++;

                FootballMatch match = FootballMatch.CreatePlayoffMatch(
                    planned.Id,
                    tournamentRef,
                    planned.HomeTeam,
                    planned.AwayTeam,
                    scheduled,
                    scheduledVenue,
                    tournamentRef.TournamentRules.PlayoffMatchRules);

                match.SetPlayoffInfo(
                    planned.Round,
                    planned.Order,
                    planned.NextMatchId,
                    planned.NextMatchSlot);

                await _matchRepository.AddAsync(match);
                createdMatches.Add(match);
            }

            _logger.LogInformation("Generated {Count} playoff matches for tournament {TournamentId}",
                createdMatches.Count, request.CompetitionId);

            // If the bracket extends past the tournament's configured EndDate (it almost always does
            // because we schedule rounds after EndDate), push EndDate out so the tournament still
            // qualifies as "active/ongoing" on the public lifecycle view. Use the latest match date
            // as the baseline so the tournament isn't immediately marked past.
            if (createdMatches.Count > 0)
            {
                DateTime latestPlayoffDate = createdMatches.Max(m => m.ScheduledDateTime);
                if (latestPlayoffDate > tournament.EndDate)
                {
                    tournament.UpdateDateRange(tournament.StartDate, latestPlayoffDate);
                }
            }

            // Flip the lifecycle status (no-op for already-PlayoffStage tournaments without bracket).
            if (tournament.TournamentStatus == FootballTournamentStatus.GroupStage)
            {
                tournament.StartPlayoffStage();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballTournamentDto tournamentDto = FootballTournamentMapper.ToDto(tournament);
            return Result<FootballTournamentDto>.Success(tournamentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while starting playoff stage for tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while starting playoff stage for tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballTournamentDto>.Failure(
                "An error occurred while starting the tournament playoff stage.",
                ex.Flatten());
        }
    }

    /// <summary>
    /// Walks the planned bracket and fills in teams for matches that don't yet have a HomeTeam/AwayTeam,
    /// using the highest-seeded team from each feeder slot. The actual winner replaces this projection
    /// when the feeder match is completed.
    /// </summary>
    private static void FillProjectedTeams(IReadOnlyList<PlayoffBracketBuilder.PlannedMatch> plannedMatches)
    {
        Dictionary<Guid, PlayoffBracketBuilder.PlannedMatch> byId = plannedMatches.ToDictionary(m => m.Id);

        // Order so feeders are processed before their successors.
        IEnumerable<PlayoffBracketBuilder.PlannedMatch> ordered =
            plannedMatches.OrderBy(m => (int)m.Round).ThenBy(m => m.Order);

        foreach (PlayoffBracketBuilder.PlannedMatch m in ordered)
        {
            // Propagate this match's "winner" projection (= its higher seed = HomeTeam) to its successor.
            if (m.NextMatchId.HasValue && m.NextMatchSlot.HasValue && m.HomeTeam != null && byId.TryGetValue(m.NextMatchId.Value, out PlayoffBracketBuilder.PlannedMatch? nextMatch))
            {
                if (m.NextMatchSlot.Value == FootballPlayoffSlot.Home)
                {
                    nextMatch.HomeTeam ??= m.HomeTeam;
                }
                else
                {
                    nextMatch.AwayTeam ??= m.HomeTeam;
                }
            }
        }
    }
}
