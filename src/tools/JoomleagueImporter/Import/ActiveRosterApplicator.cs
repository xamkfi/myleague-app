using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Teams;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// After every season is imported, keeps only each person's latest competition membership active.
/// Older seasons stay on the roster as history.
/// </summary>
internal static class ActiveRosterApplicator
{
    public static async Task ApplyFloorballAsync(FloorballImportSet set, IdMapStore idMap, FloorballApiClient api)
    {
        (Dictionary<int, LatestMembership> latest, Dictionary<Guid, int> personByPlayer, HashSet<Guid> unknown) =
            Context(set, idMap);
        int activated = 0;
        int deactivated = 0;
        int failed = 0;

        foreach (ProjectImport project in set.Projects)
        {
            if (!idMap.TryGetSeason(project.Project.Id, out Guid competitionId))
                continue;

            foreach (ProjectTeamImport pti in project.Teams.Values)
            {
                if (!idMap.TryGetTeam(pti.Team.Id, out Guid teamId))
                    continue;

                FloorballTeamDto? team = await api.GetTeamByIdAsync(teamId, competitionId);
                if (team == null)
                    continue;

                HashSet<int> used = UsedJerseys(team.Roster.Select(row => row.JerseyNumber));
                foreach (FloorballTeamPlayerDto row in team.Roster)
                {
                    if (!ShouldChange(row.PlayerId, row.IsActive, project, pti, personByPlayer, latest, unknown, out bool shouldBeActive))
                        continue;

                    int jersey = ExistingOrNext(row.JerseyNumber, used);
                    bool ok = await api.UpdateTeamPlayerAsync(
                        teamId, row.PlayerId, row.Position, jersey, shouldBeActive, competitionId);
                    Count(ok, shouldBeActive, ref activated, ref deactivated, ref failed);
                }
            }
        }

        Print(activated, deactivated, failed);
    }

    public static async Task ApplyFootballAsync(FloorballImportSet set, IdMapStore idMap, FootballApiClient api)
    {
        (Dictionary<int, LatestMembership> latest, Dictionary<Guid, int> personByPlayer, HashSet<Guid> unknown) =
            Context(set, idMap);
        int activated = 0;
        int deactivated = 0;
        int failed = 0;

        foreach (ProjectImport project in set.Projects)
        {
            if (!idMap.TryGetSeason(project.Project.Id, out Guid competitionId))
                continue;

            foreach (ProjectTeamImport pti in project.Teams.Values)
            {
                if (!idMap.TryGetTeam(pti.Team.Id, out Guid teamId))
                    continue;

                FootballTeamDto? team = await api.GetTeamByIdAsync(teamId, competitionId);
                if (team == null)
                    continue;

                HashSet<int> used = UsedJerseys(team.Roster.Select(row => row.JerseyNumber));
                foreach (FootballTeamPlayerDto row in team.Roster)
                {
                    if (!ShouldChange(row.PlayerId, row.IsActive, project, pti, personByPlayer, latest, unknown, out bool shouldBeActive))
                        continue;

                    int jersey = ExistingOrNext(row.JerseyNumber, used);
                    bool ok = await api.UpdateTeamPlayerAsync(
                        teamId, row.PlayerId, row.Position, jersey, shouldBeActive, competitionId);
                    Count(ok, shouldBeActive, ref activated, ref deactivated, ref failed);
                }
            }
        }

        Print(activated, deactivated, failed);
    }

    public static async Task ApplyHockeyAsync(FloorballImportSet set, IdMapStore idMap, HockeyApiClient api)
    {
        (Dictionary<int, LatestMembership> latest, Dictionary<Guid, int> personByPlayer, HashSet<Guid> unknown) =
            Context(set, idMap);
        int activated = 0;
        int deactivated = 0;
        int failed = 0;

        foreach (ProjectImport project in set.Projects)
        {
            if (!idMap.TryGetSeason(project.Project.Id, out Guid competitionId))
                continue;

            foreach (ProjectTeamImport pti in project.Teams.Values)
            {
                if (!idMap.TryGetTeam(pti.Team.Id, out Guid teamId))
                    continue;

                HockeyTeamDto? team = await api.GetTeamByIdAsync(teamId, competitionId);
                if (team == null)
                    continue;

                HashSet<int> used = UsedJerseys(team.Roster.Select(row => row.JerseyNumber));
                foreach (HockeyTeamPlayerDto row in team.Roster)
                {
                    if (!ShouldChange(row.PlayerId, row.IsActive, project, pti, personByPlayer, latest, unknown, out bool shouldBeActive))
                        continue;

                    if (!Enum.TryParse(row.Position, ignoreCase: true, out HockeyPosition position))
                        position = HockeyPosition.Center;
                    if (!Enum.TryParse(row.CaptainRole, ignoreCase: true, out HockeyCaptainRole captain))
                        captain = HockeyCaptainRole.None;
                    int jersey = ExistingOrNext(row.JerseyNumber, used);
                    bool ok = await api.UpdateTeamPlayerAsync(
                        teamId,
                        row.PlayerId,
                        position,
                        jersey,
                        shouldBeActive ? HockeyRosterStatus.Active : HockeyRosterStatus.Inactive,
                        captain,
                        competitionId);
                    Count(ok, shouldBeActive, ref activated, ref deactivated, ref failed);
                }
            }
        }

        Print(activated, deactivated, failed);
    }

    private static bool ShouldChange(
        Guid playerId,
        bool isActive,
        ProjectImport project,
        ProjectTeamImport team,
        Dictionary<Guid, int> personByPlayer,
        Dictionary<int, LatestMembership> latest,
        HashSet<Guid> unknown,
        out bool shouldBeActive)
    {
        shouldBeActive = false;
        if (unknown.Contains(playerId) || !personByPlayer.TryGetValue(playerId, out int personId))
            return false;
        if (!latest.TryGetValue(personId, out LatestMembership membership))
            return false;

        shouldBeActive = membership.ProjectId == project.Project.Id && membership.TeamId == team.Team.Id;
        return isActive != shouldBeActive;
    }

    private static (Dictionary<int, LatestMembership> Latest, Dictionary<Guid, int> PersonByPlayer, HashSet<Guid> Unknown)
        Context(FloorballImportSet set, IdMapStore idMap)
    {
        Dictionary<Guid, int> personByPlayer = [];
        foreach (KeyValuePair<int, IdMapStore.PersonMapping> pair in idMap.Persons)
        {
            if (pair.Value.PlayerId != Guid.Empty)
                personByPlayer[pair.Value.PlayerId] = pair.Key;
        }

        HashSet<Guid> unknown = [.. idMap.UnknownPlayers.Values];
        foreach (List<Guid> extras in idMap.ExtraUnknownPlayers.Values)
        {
            foreach (Guid playerId in extras)
                unknown.Add(playerId);
        }

        return (PlayerLatestTeamResolver.LatestMembershipByPerson(set), personByPlayer, unknown);
    }

    private static void Count(bool ok, bool activatedRow, ref int activated, ref int deactivated, ref int failed)
    {
        if (!ok)
        {
            failed++;
            return;
        }

        if (activatedRow)
            activated++;
        else
            deactivated++;
    }

    private static void Print(int activated, int deactivated, int failed)
    {
        Console.WriteLine(
            $"  Current club: activated {activated}, deactivated {deactivated} older memberships" +
            (failed > 0 ? $", {failed} failed" : "") + ".");
    }

    private static HashSet<int> UsedJerseys(IEnumerable<int?> numbers) =>
        numbers.Where(number => number is > 0 and < 100).Select(number => number!.Value).ToHashSet();

    private static int ExistingOrNext(int? preferred, HashSet<int> used)
    {
        if (preferred is > 0 and < 100)
            return preferred.Value;

        for (int number = 1; number <= 99; number++)
        {
            if (used.Add(number))
                return number;
        }

        return 99;
    }
}
