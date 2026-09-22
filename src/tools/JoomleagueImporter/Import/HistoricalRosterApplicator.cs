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
        foreach (Guid teamId in DistinctMappedTeamIds(project, idMap))
        {
            FloorballTeamDto? team = await api.GetTeamByIdAsync(teamId, competitionId);
            if (team == null)
                continue;

            List<FloorballTeamPlayerDto> toDeactivate = team.Roster
                .Where(r => r.IsActive && mappedPlayers.Contains(r.PlayerId))
                .ToList();
            HashSet<Guid> updating = toDeactivate.Select(r => r.PlayerId).ToHashSet();
            HashSet<int> claimed = ReservedJerseys(
                team.Roster.Where(r => !updating.Contains(r.PlayerId)).Select(r => r.JerseyNumber));
            foreach (FloorballTeamPlayerDto row in toDeactivate)
            {
                int jersey = ClaimJersey(row.JerseyNumber, claimed);
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
        foreach (Guid teamId in DistinctMappedTeamIds(project, idMap))
        {
            FootballTeamDto? team = await api.GetTeamByIdAsync(teamId, competitionId);
            if (team == null)
                continue;

            List<FootballTeamPlayerDto> toDeactivate = team.Roster
                .Where(r => r.IsActive && mappedPlayers.Contains(r.PlayerId))
                .ToList();
            HashSet<Guid> updating = toDeactivate.Select(r => r.PlayerId).ToHashSet();
            HashSet<int> claimed = ReservedJerseys(
                team.Roster.Where(r => !updating.Contains(r.PlayerId)).Select(r => r.JerseyNumber));
            foreach (FootballTeamPlayerDto row in toDeactivate)
            {
                int jersey = ClaimJersey(row.JerseyNumber, claimed);
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
        foreach (Guid teamId in DistinctMappedTeamIds(project, idMap))
        {
            HockeyTeamDto? team = await api.GetTeamByIdAsync(teamId, competitionId);
            if (team == null)
                continue;

            List<HockeyTeamPlayerDto> toDeactivate = team.Roster
                .Where(r => r.IsActive && mappedPlayers.Contains(r.PlayerId))
                .ToList();
            HashSet<Guid> updating = toDeactivate.Select(r => r.PlayerId).ToHashSet();
            HashSet<int> claimed = ReservedJerseys(
                team.Roster.Where(r => !updating.Contains(r.PlayerId)).Select(r => r.JerseyNumber));
            foreach (HockeyTeamPlayerDto row in toDeactivate)
            {
                if (!Enum.TryParse(row.Position, ignoreCase: true, out HockeyPosition position))
                    position = HockeyPosition.Center;
                if (!Enum.TryParse(row.CaptainRole, ignoreCase: true, out HockeyCaptainRole captain))
                    captain = HockeyCaptainRole.None;
                int jersey = ClaimJersey(row.JerseyNumber, claimed);
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

    private static IEnumerable<Guid> DistinctMappedTeamIds(ProjectImport project, IdMapStore idMap)
    {
        HashSet<Guid> seen = [];
        return project.Teams.Values
            .Select(pti =>
            {
                bool mapped = idMap.TryGetTeam(pti.Team.Id, out Guid teamId);
                return (mapped, teamId);
            })
            .Where(item => item.mapped && seen.Add(item.teamId))
            .Select(item => item.teamId);
    }

    private static HashSet<Guid> MappedPlayerIds(ProjectImport project, IdMapStore idMap)
    {
        return project.Teams.Values
            .SelectMany(pti => pti.Roster)
            .Select(entry =>
            {
                bool found = idMap.TryGetPerson(entry.Person.Id, out IdMapStore.PersonMapping? mapping)
                    && mapping != null;
                return (found, playerId: mapping?.PlayerId ?? Guid.Empty);
            })
            .Where(item => item.found)
            .Select(item => item.playerId)
            .ToHashSet();
    }

    /// <summary>
    /// Numbers already held by roster rows that this pass will not update.
    /// Inactive floorball and football rows still occupy a number in the database.
    /// </summary>
    internal static HashSet<int> ReservedJerseys(IEnumerable<int?> jerseyNumbersHeldByOthers)
    {
        HashSet<int> claimed = [];
        foreach (int? jerseyNumber in jerseyNumbersHeldByOthers)
        {
            if (jerseyNumber is > 0 and < 100)
                claimed.Add(jerseyNumber.Value);
        }

        return claimed;
    }

    /// <summary>
    /// Keeps <paramref name="preferred"/> when it is free. A second player who already
    /// shares that number gets the next free number so the unique jersey constraint holds.
    /// </summary>
    internal static int ClaimJersey(int? preferred, HashSet<int> claimed)
    {
        if (preferred is > 0 and < 100 && claimed.Add(preferred.Value))
            return preferred.Value;

        for (int number = 1; number <= 99; number++)
        {
            if (claimed.Add(number))
                return number;
        }

        return 99;
    }
}
