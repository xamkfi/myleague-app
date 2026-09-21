using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Teams;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// After a season's matches are imported, marks dump roster rows inactive so historical
/// memberships do not appear as the player's current club assignment.
/// </summary>
internal static class HistoricalRosterApplicator
{
    public static async Task DeactivateFloorballAsync(
        ProjectImport project,
        Guid competitionId,
        IdMapStore idMap,
        FloorballApiClient api)
    {
        HashSet<Guid> mappedPlayers = MappedPlayerIds(project, idMap);
        int deactivated = 0;
        int failed = 0;
        HashSet<Guid> seen = [];
        foreach (ProjectTeamImport pti in project.Teams.Values)
        {
            if (!idMap.TryGetTeam(pti.Team.Id, out Guid teamId) || !seen.Add(teamId))
                continue;

            FloorballTeamDto? team = await api.GetTeamByIdAsync(teamId, competitionId);
            if (team == null)
                continue;

            HashSet<int> used = UsedJerseys(team.Roster.Select(r => r.JerseyNumber));
            foreach (FloorballTeamPlayerDto row in team.Roster.Where(r => r.IsActive && mappedPlayers.Contains(r.PlayerId)))
            {
                int jersey = ExistingOrNext(row.JerseyNumber, used);
                if (await api.UpdateTeamPlayerAsync(teamId, row.PlayerId, row.Position, jersey, false, competitionId))
                    deactivated++;
                else
                    failed++;
            }
        }

        Console.WriteLine($"  Historical roster: deactivated {deactivated} floorball memberships" +
                          (failed > 0 ? $", {failed} failed" : "") + ".");
    }

    public static async Task DeactivateFootballAsync(
        ProjectImport project,
        Guid competitionId,
        IdMapStore idMap,
        FootballApiClient api)
    {
        HashSet<Guid> mappedPlayers = MappedPlayerIds(project, idMap);
        int deactivated = 0;
        int failed = 0;
        HashSet<Guid> seen = [];
        foreach (ProjectTeamImport pti in project.Teams.Values)
        {
            if (!idMap.TryGetTeam(pti.Team.Id, out Guid teamId) || !seen.Add(teamId))
                continue;

            FootballTeamDto? team = await api.GetTeamByIdAsync(teamId, competitionId);
            if (team == null)
                continue;

            HashSet<int> used = UsedJerseys(team.Roster.Select(r => r.JerseyNumber));
            foreach (FootballTeamPlayerDto row in team.Roster.Where(r => r.IsActive && mappedPlayers.Contains(r.PlayerId)))
            {
                int jersey = ExistingOrNext(row.JerseyNumber, used);
                if (await api.UpdateTeamPlayerAsync(teamId, row.PlayerId, row.Position, jersey, false, competitionId))
                    deactivated++;
                else
                    failed++;
            }
        }

        Console.WriteLine($"  Historical roster: deactivated {deactivated} football memberships" +
                          (failed > 0 ? $", {failed} failed" : "") + ".");
    }

    public static async Task DeactivateHockeyAsync(
        ProjectImport project,
        Guid competitionId,
        IdMapStore idMap,
        HockeyApiClient api)
    {
        HashSet<Guid> mappedPlayers = MappedPlayerIds(project, idMap);
        int deactivated = 0;
        int failed = 0;
        HashSet<Guid> seen = [];
        foreach (ProjectTeamImport pti in project.Teams.Values)
        {
            if (!idMap.TryGetTeam(pti.Team.Id, out Guid teamId) || !seen.Add(teamId))
                continue;

            HockeyTeamDto? team = await api.GetTeamByIdAsync(teamId, competitionId);
            if (team == null)
                continue;

            HashSet<int> used = UsedJerseys(team.Roster.Select(r => r.JerseyNumber));
            foreach (HockeyTeamPlayerDto row in team.Roster.Where(r => r.IsActive && mappedPlayers.Contains(r.PlayerId)))
            {
                if (!Enum.TryParse(row.Position, ignoreCase: true, out HockeyPosition position))
                    position = HockeyPosition.Center;
                if (!Enum.TryParse(row.CaptainRole, ignoreCase: true, out HockeyCaptainRole captain))
                    captain = HockeyCaptainRole.None;
                int jersey = ExistingOrNext(row.JerseyNumber, used);
                if (await api.UpdateTeamPlayerAsync(
                        teamId,
                        row.PlayerId,
                        position,
                        jersey,
                        HockeyRosterStatus.Inactive,
                        captain,
                        competitionId))
                    deactivated++;
                else
                    failed++;
            }
        }

        Console.WriteLine($"  Historical roster: deactivated {deactivated} hockey memberships" +
                          (failed > 0 ? $", {failed} failed" : "") + ".");
    }

    private static HashSet<Guid> MappedPlayerIds(ProjectImport project, IdMapStore idMap)
    {
        HashSet<Guid> ids = [];
        foreach (ProjectTeamImport pti in project.Teams.Values)
        {
            foreach (RosterEntry entry in pti.Roster)
            {
                if (idMap.TryGetPerson(entry.Person.Id, out IdMapStore.PersonMapping? mapping) && mapping != null)
                    ids.Add(mapping.PlayerId);
            }
        }

        return ids;
    }

    private static HashSet<int> UsedJerseys(IEnumerable<int?> numbers) =>
        numbers.Where(n => n is > 0 and < 100).Select(n => n!.Value).ToHashSet();

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
