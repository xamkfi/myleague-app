using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Computes season standings and player counting stats from a parsed JoomLeague dump.
/// Team records use official match scores (3/1/0). Player games use appearances or events.
/// </summary>
public static class ImportExpectedStatsCalculator
{
    public static ImportExpectedReport Build(string sport, string dumpFile, FloorballImportSet set)
    {
        ImportExpectedReport report = new()
        {
            Sport = sport,
            DumpFile = dumpFile,
            GeneratedAtUtc = DateTime.UtcNow,
        };

        foreach (ProjectImport project in set.Projects)
            report.Seasons.Add(BuildSeason(project));

        return report;
    }

    public static void ApplyIdMap(ImportExpectedReport report, IdMapStore idMap)
    {
        foreach (ExpectedSeasonStats season in report.Seasons)
        {
            if (idMap.TryGetSeason(season.OldProjectId, out Guid seasonId))
                season.NewSeasonId = seasonId;

            foreach (ExpectedTeamStats team in season.Teams)
            {
                if (idMap.TryGetTeam(team.OldTeamId, out Guid teamId))
                    team.NewTeamId = teamId;
            }

            foreach (ExpectedPlayerStats player in season.Players)
            {
                if (idMap.TryGetPerson(player.OldPersonId, out IdMapStore.PersonMapping? mapping) && mapping != null)
                    player.NewPlayerId = mapping.PlayerId;
            }
        }
    }

    internal static ExpectedSeasonStats BuildSeason(ProjectImport project)
    {
        ExpectedSeasonStats season = new()
        {
            OldProjectId = project.Project.Id,
            Name = project.Project.Name,
            MatchCount = project.Matches.Count,
            CancelledMatchCount = project.Matches.Count(m => m.Match.Cancelled),
        };

        Dictionary<int, ExpectedTeamStats> teams = [];
        foreach (ProjectTeamImport pti in project.Teams.Values)
        {
            teams[pti.ProjectTeam.Id] = new ExpectedTeamStats
            {
                OldTeamId = pti.Team.Id,
                OldProjectTeamId = pti.ProjectTeam.Id,
                Name = pti.Team.Name,
            };
        }

        Dictionary<int, int> personByTeamPlayer = [];
        Dictionary<(int PersonId, int TeamId), ExpectedPlayerStats> players = [];
        foreach (ProjectTeamImport pti in project.Teams.Values)
        {
            foreach (RosterEntry entry in pti.Roster)
            {
                personByTeamPlayer[entry.TeamPlayer.Id] = entry.Person.Id;
                (int PersonId, int TeamId) key = (entry.Person.Id, pti.Team.Id);
                if (!players.ContainsKey(key))
                {
                    players[key] = new ExpectedPlayerStats
                    {
                        OldPersonId = entry.Person.Id,
                        OldTeamId = pti.Team.Id,
                        Name = entry.Person.FullName,
                    };
                }
            }
        }

        foreach (MatchImport matchImport in project.Matches)
        {
            OldMatch match = matchImport.Match;
            if (match.Cancelled)
                continue;
            if (!match.HasResult)
                continue;

            season.PlayedMatchCount++;
            int homeGoals = match.Team1Result!.Value;
            int awayGoals = match.Team2Result!.Value;
            season.TotalGoals += homeGoals + awayGoals;

            ApplyTeamResult(teams, match.ProjectTeam1Id, homeGoals, awayGoals);
            ApplyTeamResult(teams, match.ProjectTeam2Id, awayGoals, homeGoals);

            HashSet<(int PersonId, int TeamId)> appeared = Appearances(matchImport, project, personByTeamPlayer);
            foreach ((int PersonId, int TeamId) key in appeared)
            {
                ExpectedPlayerStats player = GetOrAddPlayer(players, key, project);
                player.GamesPlayed++;
            }

            foreach (OldMatchEvent ev in matchImport.Events)
            {
                if (!personByTeamPlayer.TryGetValue(ev.TeamPlayerId, out int personId))
                    continue;
                if (!project.Teams.TryGetValue(ev.ProjectTeamId, out ProjectTeamImport? pti))
                    continue;

                ExpectedPlayerStats player = GetOrAddPlayer(players, (personId, pti.Team.Id), project);
                if (JoomleagueDatabase.GoalEventTypes.Contains(ev.EventTypeId))
                    player.Goals += Math.Max(1, ev.Count);
                else if (JoomleagueDatabase.AssistEventTypes.Contains(ev.EventTypeId))
                    player.Assists++;
                else if (ev.EventTypeId == JoomleagueDatabase.EventPenalty)
                    player.PenaltyMinutes += Math.Clamp(ev.Count, 2, 20);
                else if (ev.EventTypeId == JoomleagueDatabase.EventYellowCard)
                    player.YellowCards += Math.Max(1, ev.Count);
                else if (ev.EventTypeId == JoomleagueDatabase.EventRedCard)
                    player.RedCards += Math.Max(1, ev.Count);
            }
        }

        season.Teams = teams.Values
            .OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.GoalsFor - t.GoalsAgainst)
            .ThenByDescending(t => t.GoalsFor)
            .ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        season.Players = players.Values
            .Where(p => p.GamesPlayed > 0 || p.Goals > 0 || p.Assists > 0 || p.PenaltyMinutes > 0
                || p.YellowCards > 0 || p.RedCards > 0)
            .OrderByDescending(p => p.Goals + p.Assists)
            .ThenByDescending(p => p.Goals)
            .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return season;
    }

    private static void ApplyTeamResult(
        Dictionary<int, ExpectedTeamStats> teams,
        int projectTeamId,
        int goalsFor,
        int goalsAgainst)
    {
        if (!teams.TryGetValue(projectTeamId, out ExpectedTeamStats? team))
            return;

        team.GamesPlayed++;
        team.GoalsFor += goalsFor;
        team.GoalsAgainst += goalsAgainst;
        if (goalsFor > goalsAgainst)
        {
            team.Wins++;
            team.Points += 3;
        }
        else if (goalsFor == goalsAgainst)
        {
            team.Ties++;
            team.Points += 1;
        }
        else
        {
            team.Losses++;
        }
    }

    private static HashSet<(int PersonId, int TeamId)> Appearances(
        MatchImport matchImport,
        ProjectImport project,
        Dictionary<int, int> personByTeamPlayer)
    {
        HashSet<(int PersonId, int TeamId)> result = [];
        OldMatch match = matchImport.Match;
        AddSideAppearances(result, matchImport, project, personByTeamPlayer, match.ProjectTeam1Id);
        AddSideAppearances(result, matchImport, project, personByTeamPlayer, match.ProjectTeam2Id);
        return result;
    }

    private static void AddSideAppearances(
        HashSet<(int PersonId, int TeamId)> result,
        MatchImport matchImport,
        ProjectImport project,
        Dictionary<int, int> personByTeamPlayer,
        int projectTeamId)
    {
        if (!project.Teams.TryGetValue(projectTeamId, out ProjectTeamImport? pti))
            return;

        HashSet<int> sideTeamPlayers = pti.Roster.Select(r => r.TeamPlayer.Id).ToHashSet();
        if (matchImport.Players.Count > 0)
        {
            foreach (OldMatchPlayer appearance in matchImport.Players)
            {
                if (!sideTeamPlayers.Contains(appearance.TeamPlayerId))
                    continue;
                if (personByTeamPlayer.TryGetValue(appearance.TeamPlayerId, out int personId))
                    result.Add((personId, pti.Team.Id));
            }
        }

        foreach (OldMatchEvent ev in matchImport.Events)
        {
            if (ev.ProjectTeamId != projectTeamId)
                continue;
            if (personByTeamPlayer.TryGetValue(ev.TeamPlayerId, out int personId))
                result.Add((personId, pti.Team.Id));
        }
    }

    private static ExpectedPlayerStats GetOrAddPlayer(
        Dictionary<(int PersonId, int TeamId), ExpectedPlayerStats> players,
        (int PersonId, int TeamId) key,
        ProjectImport project)
    {
        if (players.TryGetValue(key, out ExpectedPlayerStats? existing))
            return existing;

        string name = project.Teams.Values
            .SelectMany(t => t.Roster)
            .FirstOrDefault(r => r.Person.Id == key.PersonId)
            ?.Person.FullName ?? $"Person {key.PersonId}";
        ExpectedPlayerStats created = new()
        {
            OldPersonId = key.PersonId,
            OldTeamId = key.TeamId,
            Name = name,
        };
        players[key] = created;
        return created;
    }
}
