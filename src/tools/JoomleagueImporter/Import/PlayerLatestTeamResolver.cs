using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Picks each person's latest imported team from project dates (then project id).
/// That membership is the only one that should stay active on the MyLeague roster.
/// </summary>
internal static class PlayerLatestTeamResolver
{
    public static Dictionary<int, int> LatestOldTeamIdByPerson(FloorballImportSet set)
    {
        Dictionary<int, (DateTime SortDate, int ProjectId, int TeamId)> latest = [];

        foreach (ProjectImport pi in set.Projects)
        {
            DateTime sortDate = pi.Project.StartDate ?? DateTime.MinValue;
            foreach (ProjectTeamImport pti in pi.Teams.Values)
            {
                foreach (RosterEntry re in pti.Roster)
                {
                    int personId = re.Person.Id;
                    if (latest.TryGetValue(personId, out (DateTime SortDate, int ProjectId, int TeamId) existing)
                        && (existing.SortDate > sortDate
                            || (existing.SortDate == sortDate && existing.ProjectId >= pi.Project.Id)))
                    {
                        continue;
                    }

                    latest[personId] = (sortDate, pi.Project.Id, pti.Team.Id);
                }
            }
        }

        Dictionary<int, int> result = [];
        foreach (KeyValuePair<int, (DateTime SortDate, int ProjectId, int TeamId)> pair in latest)
            result[pair.Key] = pair.Value.TeamId;
        return result;
    }
}
