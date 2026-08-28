using Domain.Enums.Common;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Unions roster rows and inferred categories per old team across selected projects.
/// Later projects overwrite jersey/position.
/// </summary>
internal static class TeamRosterUnion
{
    public static (Dictionary<int, Dictionary<int, RosterEntry>> RosterByTeam, Dictionary<int, TeamCategory> CategoryByTeam)
        Build(FloorballImportSet set)
    {
        Dictionary<int, Dictionary<int, RosterEntry>> rosterByTeam = [];
        Dictionary<int, TeamCategory> categoryByTeam = [];
        foreach (ProjectImport pi in set.Projects.OrderBy(p => p.Project.Id))
        {
            TeamCategory projectCategory = TeamCategoryResolver.InferFromName(pi.Project.Name);
            foreach (ProjectTeamImport pti in pi.Teams.Values)
            {
                if (!rosterByTeam.TryGetValue(pti.Team.Id, out Dictionary<int, RosterEntry>? union))
                {
                    union = [];
                    rosterByTeam[pti.Team.Id] = union;
                }
                foreach (RosterEntry re in pti.Roster)
                    union[re.Person.Id] = re;

                TeamCategory fromTeamName = TeamCategoryResolver.InferFromName(pti.Team.Name);
                TeamCategory combined = TeamCategoryResolver.Prefer(projectCategory, fromTeamName);
                if (categoryByTeam.TryGetValue(pti.Team.Id, out TeamCategory existingCat))
                    categoryByTeam[pti.Team.Id] = TeamCategoryResolver.Prefer(existingCat, combined);
                else
                    categoryByTeam[pti.Team.Id] = combined;
            }
        }

        return (rosterByTeam, categoryByTeam);
    }
}
