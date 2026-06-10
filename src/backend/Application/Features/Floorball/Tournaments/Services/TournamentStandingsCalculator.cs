using Domain.Entities.Floorball;

namespace Application.Features.Floorball.Tournaments.Services;

/// <summary>
/// Pure calculator that turns a set of completed group-stage matches into ordered standings rows.
/// Kept separate from the read-side handler so that bracket generation reuses the exact same
/// tie-break ordering as the public group standings table.
/// </summary>
public static class TournamentStandingsCalculator
{
    /// <summary>
    /// A single standings row. The fields here intentionally mirror the read-side
    /// <c>FloorballTournamentGroupStandingDto</c>; the calculator stays in the application layer
    /// (rather than the domain) because it's a query-side concern.
    /// </summary>
    public sealed class StandingsRow
    {
        public Guid TeamId { get; init; }
        public string TeamName { get; init; } = string.Empty;
        public int GamesPlayed { get; private set; }
        public int Wins { get; private set; }
        public int Draws { get; private set; }
        public int Losses { get; private set; }
        public int GoalsFor { get; private set; }
        public int GoalsAgainst { get; private set; }
        public int GoalDifference => GoalsFor - GoalsAgainst;
        public int Points { get; private set; }

        internal void AddResult(int scoredFor, int scoredAgainst)
        {
            GamesPlayed++;
            GoalsFor += scoredFor;
            GoalsAgainst += scoredAgainst;
            if (scoredFor > scoredAgainst)
            {
                Wins++;
                Points += 3;
            }
            else if (scoredFor < scoredAgainst)
            {
                Losses++;
            }
            else
            {
                Draws++;
                Points += 1;
            }
        }
    }

    /// <summary>
    /// Compute standings for a single group from its completed matches.
    /// The teams are seeded with zero-row entries so groups with no completed matches still produce
    /// a stable result (lexicographically ordered by name).
    /// </summary>
    public static List<StandingsRow> Compute(FloorballTournamentGroup group, IEnumerable<FloorballMatch> completedMatches)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(completedMatches);

        Dictionary<Guid, StandingsRow> rows = group.Teams.ToDictionary(
            gt => gt.TeamId,
            gt => new StandingsRow
            {
                TeamId = gt.TeamId,
                TeamName = gt.Team?.Name ?? "Unknown"
            });

        foreach (FloorballMatch match in completedMatches)
        {
            // A completed match always has both team IDs assigned (Start() enforces it). Defensively
            // skip matches where a slot is somehow null instead of crashing the standings query.
            if (!match.HomeTeamId.HasValue || !match.AwayTeamId.HasValue)
            {
                continue;
            }
            bool homeKnown = rows.TryGetValue(match.HomeTeamId.Value, out StandingsRow? home);
            bool awayKnown = rows.TryGetValue(match.AwayTeamId.Value, out StandingsRow? away);
            if (!homeKnown && !awayKnown)
            {
                continue;
            }
            if (homeKnown && home != null)
            {
                home.AddResult(match.HomeScore, match.AwayScore);
            }
            if (awayKnown && away != null)
            {
                away.AddResult(match.AwayScore, match.HomeScore);
            }
        }

        return rows.Values
            .OrderByDescending(r => r.Points)
            .ThenByDescending(r => r.GoalDifference)
            .ThenByDescending(r => r.GoalsFor)
            .ThenBy(r => r.TeamName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
