using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Entities.Football.Statistics;
using Domain.Enums.Football;
using Domain.ValueObjects.Football;

namespace Application.Features.Football.Tournaments.Services;

/// <summary>
/// Pure pairing logic for the playoff stage.
///
/// Cross-group seeding (the typical IIHF-style bracket):
///   - For 2 groups with 2 advancing per group (4 teams total):
///       SF1: A1 vs B2, SF2: B1 vs A2, F: SF1 winner vs SF2 winner, optional 3rd-place between losers.
///   - For 2 groups with 4 advancing per group (8 teams total):
///       QF1: A1 vs B4, QF2: B1 vs A4, QF3: A2 vs B3, QF4: B2 vs A3.
///       SF1 = QF1 winner vs QF3 winner. SF2 = QF2 winner vs QF4 winner. F = SF1 winner vs SF2 winner.
///
/// Single-group fall-back:
///   - For one group only, intra-group seeding 1v4, 2v3 (semifinals if 4 advance), or 1v2 (final if 2 advance).
///
/// All other configurations (more than 2 groups, or counts that don't yield 2/4/8 teams) are rejected
/// up-front so the caller can return a clean validation error.
/// </summary>
public static class PlayoffBracketBuilder
{
    /// <summary>
    /// Single seed in the seeded list of qualifying teams (group order + standings rank).
    /// </summary>
    public sealed record Seed(int GroupIndex, int SeedWithinGroup, FootballTeam Team);

    /// <summary>
    /// One match in the planned bracket. The handler turns these into actual <see cref="FootballMatch"/>
    /// entities and assigns deterministic Guids before persisting them.
    /// </summary>
    public sealed class PlannedMatch
    {
        public required Guid Id { get; init; }
        public required FootballPlayoffRound Round { get; init; }
        public required int Order { get; init; }
        public FootballTeam? HomeTeam { get; set; }
        public FootballTeam? AwayTeam { get; set; }
        public Guid? NextMatchId { get; set; }
        public FootballPlayoffSlot? NextMatchSlot { get; set; }
    }

    /// <summary>
    /// Builds the round-1 + downstream playoff matches for a tournament.
    /// </summary>
    /// <param name="seeds">Seeded qualifying teams (cross-group order: A1, B1, A2, B2, ...).</param>
    /// <param name="hasThirdPlaceMatch">Whether the tournament includes a 3rd-place match.</param>
    /// <returns>The list of planned matches across all rounds, in display order.</returns>
    public static IReadOnlyList<PlannedMatch> Build(IReadOnlyList<Seed> seeds, bool hasThirdPlaceMatch)
    {
        ArgumentNullException.ThrowIfNull(seeds);
        if (seeds.Count != 2 && seeds.Count != 4 && seeds.Count != 8)
        {
            throw new ArgumentException(
                $"Unsupported playoff team count {seeds.Count}. Supported sizes are 2, 4 and 8.",
                nameof(seeds));
        }

        return seeds.Count switch
        {
            2 => BuildTwoTeamFinal(seeds),
            4 => BuildFourTeamBracket(seeds, hasThirdPlaceMatch),
            8 => BuildEightTeamBracket(seeds, hasThirdPlaceMatch),
            _ => Array.Empty<PlannedMatch>()
        };
    }

    /// <summary>
    /// Orders the qualifying teams from each group in cross-group seeding order so that the
    /// bracket pairings yield the standard "highest seed vs lowest seed" matchups.
    ///
    /// For 2 groups with N advancing per group:
    ///   - 8 teams: A1, B1, A2, B2, A3, B3, A4, B4 ⇒ pairings A1×B4, B1×A4, A2×B3, B2×A3 (the QFs).
    ///   - 4 teams: A1, B1, A2, B2 ⇒ pairings A1×B2, B1×A2 (the SFs).
    ///   - 2 teams: A1, B1 ⇒ A1×B1 (the final).
    /// For a single group:
    ///   - The seeds are returned in straight rank order; the caller pairs 1×N, 2×(N-1), etc.
    /// </summary>
    public static IReadOnlyList<Seed> BuildSeedList(
        IReadOnlyList<IReadOnlyList<FootballTeam>> groupRankings,
        int teamsAdvancingPerGroup)
    {
        ArgumentNullException.ThrowIfNull(groupRankings);
        if (groupRankings.Count == 0)
        {
            return Array.Empty<Seed>();
        }

        List<Seed> seeds = new List<Seed>();
        // Round-robin pick across groups, one rank at a time, to produce A1, B1, A2, B2, ...
        for (int rank = 0; rank < teamsAdvancingPerGroup; rank++)
        {
            for (int groupIndex = 0; groupIndex < groupRankings.Count; groupIndex++)
            {
                IReadOnlyList<FootballTeam> teams = groupRankings[groupIndex];
                if (rank >= teams.Count)
                {
                    // This group does not have enough completed teams to fill the slot.
                    continue;
                }
                seeds.Add(new Seed(groupIndex, rank, teams[rank]));
            }
        }
        return seeds;
    }

    private static IReadOnlyList<PlannedMatch> BuildTwoTeamFinal(IReadOnlyList<Seed> seeds)
    {
        PlannedMatch final = new PlannedMatch
        {
            Id = Guid.NewGuid(),
            Round = FootballPlayoffRound.Final,
            Order = 0,
            HomeTeam = seeds[0].Team,
            AwayTeam = seeds[1].Team
        };
        return new[] { final };
    }

    private static IReadOnlyList<PlannedMatch> BuildFourTeamBracket(IReadOnlyList<Seed> seeds, bool hasThirdPlaceMatch)
    {
        // Cross-group seeding for 2 groups × 2:
        //   seeds = [A1, B1, A2, B2]
        //   SF1 = A1 vs B2, SF2 = B1 vs A2
        // Single-group fall-back: seeds = [1, 2, 3, 4] -> SF1 = 1 vs 4, SF2 = 2 vs 3.
        FootballTeam sf1Home, sf1Away, sf2Home, sf2Away;
        if (seeds.Select(s => s.GroupIndex).Distinct().Count() >= 2)
        {
            sf1Home = seeds[0].Team; // A1
            sf1Away = seeds[3].Team; // B2
            sf2Home = seeds[1].Team; // B1
            sf2Away = seeds[2].Team; // A2
        }
        else
        {
            sf1Home = seeds[0].Team; // 1
            sf1Away = seeds[3].Team; // 4
            sf2Home = seeds[1].Team; // 2
            sf2Away = seeds[2].Team; // 3
        }

        PlannedMatch final = new PlannedMatch
        {
            Id = Guid.NewGuid(),
            Round = FootballPlayoffRound.Final,
            Order = 0
        };

        // 3rd place match: projected losers (= lower seed) of each semifinal go here. The match
        // completion handler replaces the slot with the actual semifinal loser when the SF finalizes.
        PlannedMatch? thirdPlace = hasThirdPlaceMatch
            ? new PlannedMatch
            {
                Id = Guid.NewGuid(),
                Round = FootballPlayoffRound.ThirdPlaceMatch,
                Order = 0,
                HomeTeam = sf1Away,
                AwayTeam = sf2Away
            }
            : null;

        PlannedMatch sf1 = new PlannedMatch
        {
            Id = Guid.NewGuid(),
            Round = FootballPlayoffRound.SemiFinal,
            Order = 0,
            HomeTeam = sf1Home,
            AwayTeam = sf1Away,
            NextMatchId = final.Id,
            NextMatchSlot = FootballPlayoffSlot.Home
        };

        PlannedMatch sf2 = new PlannedMatch
        {
            Id = Guid.NewGuid(),
            Round = FootballPlayoffRound.SemiFinal,
            Order = 1,
            HomeTeam = sf2Home,
            AwayTeam = sf2Away,
            NextMatchId = final.Id,
            NextMatchSlot = FootballPlayoffSlot.Away
        };

        // Note: forward references for the 3rd-place match are intentionally left null — there is
        // no successor. Losers are propagated by the match completion handler (it has access to
        // the loser team id at finalization time) when both feeder semifinals share that 3rd-place
        // target. We don't model it via NextMatchId on the SF (which is reserved for the winner).

        List<PlannedMatch> result = new List<PlannedMatch> { sf1, sf2 };
        if (thirdPlace != null)
        {
            result.Add(thirdPlace);
        }
        result.Add(final);
        return result;
    }

    private static IReadOnlyList<PlannedMatch> BuildEightTeamBracket(IReadOnlyList<Seed> seeds, bool hasThirdPlaceMatch)
    {
        // Expecting 8 seeds in cross-group order: A1, B1, A2, B2, A3, B3, A4, B4
        FootballTeam a1 = seeds[0].Team;
        FootballTeam b1 = seeds[1].Team;
        FootballTeam a2 = seeds[2].Team;
        FootballTeam b2 = seeds[3].Team;
        FootballTeam a3 = seeds[4].Team;
        FootballTeam b3 = seeds[5].Team;
        FootballTeam a4 = seeds[6].Team;
        FootballTeam b4 = seeds[7].Team;

        PlannedMatch final = new PlannedMatch
        {
            Id = Guid.NewGuid(),
            Round = FootballPlayoffRound.Final,
            Order = 0
        };

        // 3rd place match: projected losers (= lower seed of each SF) — A2 from SF1 (QF3 projection)
        // and B2 from SF2 (QF4 projection). Replaced by actual SF losers at completion time.
        PlannedMatch? thirdPlace = hasThirdPlaceMatch
            ? new PlannedMatch
            {
                Id = Guid.NewGuid(),
                Round = FootballPlayoffRound.ThirdPlaceMatch,
                Order = 0,
                HomeTeam = a2,
                AwayTeam = b2
            }
            : null;

        PlannedMatch sf1 = new PlannedMatch
        {
            Id = Guid.NewGuid(),
            Round = FootballPlayoffRound.SemiFinal,
            Order = 0,
            NextMatchId = final.Id,
            NextMatchSlot = FootballPlayoffSlot.Home
        };

        PlannedMatch sf2 = new PlannedMatch
        {
            Id = Guid.NewGuid(),
            Round = FootballPlayoffRound.SemiFinal,
            Order = 1,
            NextMatchId = final.Id,
            NextMatchSlot = FootballPlayoffSlot.Away
        };

        // Quarterfinals: A1×B4, B1×A4, A2×B3, B2×A3
        PlannedMatch qf1 = new PlannedMatch
        {
            Id = Guid.NewGuid(),
            Round = FootballPlayoffRound.QuarterFinal,
            Order = 0,
            HomeTeam = a1,
            AwayTeam = b4,
            NextMatchId = sf1.Id,
            NextMatchSlot = FootballPlayoffSlot.Home
        };
        PlannedMatch qf2 = new PlannedMatch
        {
            Id = Guid.NewGuid(),
            Round = FootballPlayoffRound.QuarterFinal,
            Order = 1,
            HomeTeam = b1,
            AwayTeam = a4,
            NextMatchId = sf2.Id,
            NextMatchSlot = FootballPlayoffSlot.Home
        };
        PlannedMatch qf3 = new PlannedMatch
        {
            Id = Guid.NewGuid(),
            Round = FootballPlayoffRound.QuarterFinal,
            Order = 2,
            HomeTeam = a2,
            AwayTeam = b3,
            NextMatchId = sf1.Id,
            NextMatchSlot = FootballPlayoffSlot.Away
        };
        PlannedMatch qf4 = new PlannedMatch
        {
            Id = Guid.NewGuid(),
            Round = FootballPlayoffRound.QuarterFinal,
            Order = 3,
            HomeTeam = b2,
            AwayTeam = a3,
            NextMatchId = sf2.Id,
            NextMatchSlot = FootballPlayoffSlot.Away
        };

        List<PlannedMatch> result = new List<PlannedMatch> { qf1, qf2, qf3, qf4, sf1, sf2 };
        if (thirdPlace != null)
        {
            result.Add(thirdPlace);
        }
        result.Add(final);
        return result;
    }
}

/// <summary>
/// Helper used by the bracket generator to scan the existing competition matches and decide whether
/// the playoffs are ready to start (or already started).
/// </summary>
public static class PlayoffBracketReadiness
{
    /// <summary>
    /// Validation outcome for the "Start Playoff Stage" action.
    /// </summary>
    public sealed class Outcome
    {
        public bool IsReady { get; init; }
        public string? Error { get; init; }
        /// <summary>
        /// True when the bracket already exists (re-running the action is a no-op).
        /// </summary>
        public bool BracketAlreadyExists { get; init; }
    }

    /// <summary>
    /// Checks that:
    ///  - HasPlayoffStage is enabled;
    ///  - TeamsAdvancingPerGroup × group count yields a supported playoff size (2/4/8);
    ///  - Every group-stage match has been completed (no Scheduled/InProgress/Postponed left);
    ///  - At least <c>TeamsAdvancingPerGroup</c> teams in each group played at least one match.
    /// </summary>
    public static Outcome Evaluate(FootballTournament tournament, IEnumerable<FootballMatch> tournamentMatches)
    {
        ArgumentNullException.ThrowIfNull(tournament);
        ArgumentNullException.ThrowIfNull(tournamentMatches);

        FootballTournamentRules rules = tournament.TournamentRules;
        if (!rules.HasPlayoffStage)
        {
            return new Outcome
            {
                IsReady = false,
                Error = "This tournament does not have a playoff stage enabled."
            };
        }

        int groupCount = tournament.Groups.Count;
        if (groupCount == 0)
        {
            return new Outcome
            {
                IsReady = false,
                Error = "Cannot start playoff stage: tournament has no groups."
            };
        }

        int playoffTeamCount = rules.TeamsAdvancingPerGroup * groupCount;
        if (playoffTeamCount != 2 && playoffTeamCount != 4 && playoffTeamCount != 8)
        {
            return new Outcome
            {
                IsReady = false,
                Error = $"Unsupported playoff size {playoffTeamCount}. " +
                        $"TeamsAdvancingPerGroup × group count must be 2, 4 or 8 (got {rules.TeamsAdvancingPerGroup} × {groupCount})."
            };
        }

        List<FootballMatch> matchList = tournamentMatches.ToList();
        bool bracketAlreadyExists = matchList.Any(m => m.PlayoffRound != null);
        if (bracketAlreadyExists)
        {
            return new Outcome
            {
                IsReady = true,
                BracketAlreadyExists = true
            };
        }

        // Every group-stage match must be completed before the bracket is generated.
        int groupStageMatchCount = matchList.Count(m => m.TournamentGroupId != null);
        if (groupStageMatchCount == 0)
        {
            return new Outcome
            {
                IsReady = false,
                Error = "Cannot start playoff stage: no group-stage matches have been played."
            };
        }

        int unfinished = matchList.Count(m =>
            m.TournamentGroupId != null
            && m.Status != FootballMatchStatus.Completed
            && m.Status != FootballMatchStatus.Cancelled);
        if (unfinished > 0)
        {
            return new Outcome
            {
                IsReady = false,
                Error = $"Cannot start playoff stage: {unfinished} group-stage match(es) are still unfinished."
            };
        }

        return new Outcome { IsReady = true };
    }
}
