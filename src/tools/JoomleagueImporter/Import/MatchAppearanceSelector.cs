using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Resolves who actually appeared in a JoomLeague match.
/// When <c>jos_joomleague_match_player</c> has rows, those are the source of truth.
/// Otherwise only event participants (goals, cards, penalties) count — the season
/// roster is not treated as having played every game.
/// </summary>
internal static class MatchAppearanceSelector
{
    public static HashSet<Guid> PlayerIdsOnSide(
        MatchImport match,
        IReadOnlyDictionary<int, Guid> playerByTeamPlayerId,
        IEnumerable<int> sideTeamPlayerIds,
        IEnumerable<Guid> eventPlayerIdsOnSide)
    {
        HashSet<int> sideTeamPlayers = sideTeamPlayerIds.ToHashSet();
        HashSet<Guid> result = [];

        if (match.Players.Count > 0)
        {
            foreach (OldMatchPlayer appearance in match.Players)
            {
                if (!sideTeamPlayers.Contains(appearance.TeamPlayerId))
                    continue;
                if (playerByTeamPlayerId.TryGetValue(appearance.TeamPlayerId, out Guid playerId))
                    result.Add(playerId);
            }
        }

        foreach (Guid eventPlayerId in eventPlayerIdsOnSide)
            result.Add(eventPlayerId);

        return result;
    }

    public static Guid? GoaliePlayerId(
        MatchImport match,
        IReadOnlyDictionary<int, Guid> playerByTeamPlayerId,
        IEnumerable<RosterEntry> sideRoster)
    {
        if (match.Players.Count == 0)
            return null;

        HashSet<int> appeared = match.Players.Select(p => p.TeamPlayerId).ToHashSet();
        foreach (RosterEntry entry in sideRoster)
        {
            if (!entry.IsGoalkeeper || !appeared.Contains(entry.TeamPlayer.Id))
                continue;
            if (playerByTeamPlayerId.TryGetValue(entry.TeamPlayer.Id, out Guid playerId))
                return playerId;
        }

        return null;
    }
}
