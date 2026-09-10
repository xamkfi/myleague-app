using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// After every team roster has been imported, keeps only the latest club/team
/// membership active for each person.
/// </summary>
internal static class ActiveRosterApplicator
{
    public static async Task ApplyAsync(
        FloorballImportSet set,
        IdMapStore idMap,
        Func<Guid, RosterEntry, Guid, bool, Task<bool>> setActiveOnTeam)
    {
        Dictionary<int, int> latestTeamByPerson = PlayerLatestTeamResolver.LatestOldTeamIdByPerson(set);
        (Dictionary<int, Dictionary<int, RosterEntry>> rosterByTeam, _) = TeamRosterUnion.Build(set);

        int deactivated = 0;
        int failed = 0;
        foreach (OldTeam oldTeam in set.UniqueTeams.Values)
        {
            if (!idMap.TryGetTeam(oldTeam.Id, out Guid teamId))
                continue;
            if (!rosterByTeam.TryGetValue(oldTeam.Id, out Dictionary<int, RosterEntry>? roster))
                continue;

            foreach (RosterEntry entry in roster.Values)
            {
                if (!idMap.TryGetPerson(entry.Person.Id, out IdMapStore.PersonMapping? mapping) || mapping == null)
                    continue;

                bool shouldBeActive = latestTeamByPerson.TryGetValue(entry.Person.Id, out int latestTeamId)
                    && latestTeamId == oldTeam.Id;
                if (shouldBeActive)
                    continue;

                if (await setActiveOnTeam(teamId, entry, mapping.PlayerId, false))
                    deactivated++;
                else
                    failed++;
            }
        }

        Console.WriteLine($"  Active club: deactivated {deactivated} stale memberships" +
                          (failed > 0 ? $", {failed} failed" : "") + ".");
    }
}
