using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Picks each person's latest imported team from project dates (then project id).
/// That membership is the only one that should stay active on the MyLeague roster.
/// </summary>
internal readonly record struct LatestMembership(int ProjectId, int TeamId);

internal static class PlayerLatestTeamResolver
{
    public static Dictionary<int, int> LatestOldTeamIdByPerson(FloorballImportSet set)
    {
        Dictionary<int, int> result = [];
        foreach (KeyValuePair<int, LatestMembership> pair in LatestMembershipByPerson(set))
            result[pair.Key] = pair.Value.TeamId;
        return result;
    }

    public static Dictionary<int, LatestMembership> LatestMembershipByPerson(FloorballImportSet set)
    {
        Dictionary<int, (DateTime SortDate, LatestMembership Membership)> latest = [];

        foreach (ProjectImport pi in set.Projects)
        {
            DateTime sortDate = pi.Project.StartDate ?? DateTime.MinValue;
            foreach (ProjectTeamImport pti in pi.Teams.Values)
            {
                LatestMembership membership = new(pi.Project.Id, pti.Team.Id);
                foreach (int personId in pti.Roster.Select(entry => entry.Person.Id))
                {
                    if (latest.TryGetValue(personId, out (DateTime SortDate, LatestMembership Membership) existing)
                        && (existing.SortDate > sortDate
                            || (existing.SortDate == sortDate && existing.Membership.ProjectId >= membership.ProjectId)))
                    {
                        continue;
                    }

                    latest[personId] = (sortDate, membership);
                }
            }
        }

        Dictionary<int, LatestMembership> result = [];
        foreach (KeyValuePair<int, (DateTime SortDate, LatestMembership Membership)> pair in latest)
            result[pair.Key] = pair.Value.Membership;
        return result;
    }
}
