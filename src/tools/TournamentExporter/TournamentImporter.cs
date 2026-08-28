using System.Text.Json;
using System.Text.Json.Serialization;

namespace TournamentExporter;

internal static class TournamentImporter
{
    public static async Task ImportDirectoryAsync(string targetApiUrl, string email, string directory, bool replace)
    {
        string[] files = Directory.GetFiles(directory, "*.json")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (files.Length == 0)
            throw new InvalidOperationException($"No JSON files found in {directory}");

        JsonSerializerOptions json = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        json.Converters.Add(new JsonStringEnumConverter());

        using TargetApiClient api = new(targetApiUrl);
        await api.AuthenticateAsync(email);
        Guid refereeId = await api.GetOrCreateImportRefereeAsync();
        Console.WriteLine($"Using import referee {refereeId}\n");

        foreach (string file in files)
        {
            Console.WriteLine($"=== Import {Path.GetFileName(file)} ===");
            string text = await File.ReadAllTextAsync(file);
            ExportPayload? payload = JsonSerializer.Deserialize<ExportPayload>(text, json);
            if (payload?.Tournament is null)
                throw new InvalidOperationException($"{file} is not a tournament import payload.");

            await ImportOneAsync(api, payload, replace, refereeId);
            Console.WriteLine();
        }
    }

    private static async Task ImportOneAsync(TargetApiClient api, ExportPayload payload, bool replace, Guid refereeId)
    {
        string category = payload.Tournament.TeamCategory;
        if (!ExportBuilder.TryParseCategory(category, out string parsedCategory))
            parsedCategory = "Adult";

        IdName? existingTournament = await api.FindTournamentAsync(payload.Tournament.Name);
        if (existingTournament is not null)
        {
            if (!replace)
            {
                Console.WriteLine($"  Tournament '{payload.Tournament.Name}' already exists ({existingTournament.Id}) — skipping file.");
                return;
            }

            await api.DeleteTournamentAsync(existingTournament.Id);
            Console.WriteLine($"  Replaced existing tournament '{payload.Tournament.Name}' ({existingTournament.Id})");
        }

        Dictionary<string, Guid> clubIds = new(StringComparer.OrdinalIgnoreCase);
        foreach (ExportClub club in payload.Clubs)
        {
            IdName? existing = await api.FindClubAsync(club.Name);
            if (existing is not null)
            {
                clubIds[club.Name] = existing.Id;
                Console.WriteLine($"  Club exists: {club.Name}");
                continue;
            }

            IdName created = await api.CreateClubAsync(club);
            clubIds[club.Name] = created.Id;
            Console.WriteLine($"  Created club: {club.Name}");
        }

        Dictionary<string, Guid> teamIds = new(StringComparer.OrdinalIgnoreCase);
        foreach (ExportTeam team in payload.Teams)
        {
            if (!clubIds.TryGetValue(team.ClubName, out Guid clubId))
            {
                IdName? club = await api.FindClubAsync(team.ClubName);
                if (club is null)
                    club = await api.CreateClubAsync(new ExportClub { Name = team.ClubName });
                clubId = club.Id;
                clubIds[team.ClubName] = clubId;
            }

            string teamCategory = ExportBuilder.TryParseCategory(team.Category, out string teamCat)
                ? teamCat
                : parsedCategory;

            IdName? existingTeam = await api.FindTeamAsync(team.Name);
            if (existingTeam is not null)
            {
                teamIds[team.Name] = existingTeam.Id;
                Console.WriteLine($"  Team exists: {team.Name}");
            }
            else
            {
                IdName createdTeam = await api.CreateTeamAsync(team, clubId, teamCategory);
                teamIds[team.Name] = createdTeam.Id;
                Console.WriteLine($"  Created team: {team.Name} [{teamCategory}]");
            }

            await ImportPlayersAsync(api, team, teamIds[team.Name]);
        }

        TournamentDetail tournament = await api.CreateTournamentWithScheduleAsync(
            payload.Tournament,
            payload.PlayoffSchedule);
        Console.WriteLine($"  Created tournament: {tournament.Name} ({tournament.Id}) [{parsedCategory}]");

        Dictionary<string, Guid> groupIds = new(StringComparer.OrdinalIgnoreCase);
        foreach (ExportGroup group in payload.Groups)
        {
            tournament = await api.AddGroupAsync(tournament.Id, group.Name);
            TournamentGroupDetail created = tournament.Groups.First(g =>
                string.Equals(g.Name, group.Name, StringComparison.OrdinalIgnoreCase));
            groupIds[group.Name] = created.Id;
            Console.WriteLine($"  Created group: {group.Name}");

            foreach (string teamName in group.TeamNames)
            {
                if (!teamIds.TryGetValue(teamName, out Guid teamId))
                    throw new InvalidOperationException($"Group '{group.Name}' references unknown team '{teamName}'.");
                tournament = await api.AddTeamToGroupAsync(tournament.Id, created.Id, teamId);
                Console.WriteLine($"    Assigned {teamName} → {group.Name}");
            }
        }

        Dictionary<string, Dictionary<string, Guid>> rosterByTeam = [];
        foreach ((string teamName, Guid teamId) in teamIds)
        {
            SourceTeam? snapshot = await api.GetTeamAsync(teamId);
            Dictionary<string, Guid> names = new(StringComparer.OrdinalIgnoreCase);
            foreach (SourceRosterPlayer row in snapshot?.Roster ?? [])
            {
                if (row.PlayerId != Guid.Empty && !string.IsNullOrWhiteSpace(row.PlayerName))
                    names.TryAdd(row.PlayerName.Trim(), row.PlayerId);
            }

            rosterByTeam[teamName] = names;
        }

        int matchesCreated = 0;
        int matchesWithEvents = 0;
        foreach (ExportMatch match in payload.Matches)
        {
            if (!teamIds.TryGetValue(match.HomeTeamName, out Guid homeId)
                || !teamIds.TryGetValue(match.AwayTeamName, out Guid awayId))
            {
                Console.WriteLine($"  WARN: skip match {match.HomeTeamName} vs {match.AwayTeamName} — missing team id");
                continue;
            }

            Guid? groupId = null;
            if (!string.IsNullOrWhiteSpace(match.GroupName) && groupIds.TryGetValue(match.GroupName, out Guid mappedGroup))
                groupId = mappedGroup;

            string? venue = NonEmpty(match.Venue) ?? ComposeVenue(payload.Tournament.Venue, match.Field);
            IdName created = await api.CreateMatchAsync(
                tournament.Id,
                homeId,
                awayId,
                match.ScheduledDateTime,
                venue,
                groupId,
                match.TournamentStage);
            matchesCreated++;

            bool replayed = await ReplayMatchEventsAsync(
                api,
                created.Id,
                match,
                homeId,
                awayId,
                refereeId,
                rosterByTeam,
                payload.Tournament);
            if (replayed)
                matchesWithEvents++;
        }

        Console.WriteLine($"  Created {matchesCreated} matches ({matchesWithEvents} with events).");
    }

    private static async Task ImportPlayersAsync(TargetApiClient api, ExportTeam team, Guid teamId)
    {
        if (team.Players is null || team.Players.Count == 0)
            return;

        SourceTeam? snapshot = await api.GetTeamAsync(teamId);
        HashSet<string> rosterNames = new(StringComparer.OrdinalIgnoreCase);
        foreach (SourceRosterPlayer row in snapshot?.Roster ?? [])
        {
            if (!string.IsNullOrWhiteSpace(row.PlayerName))
                rosterNames.Add(row.PlayerName.Trim());
        }

        int added = 0;
        int skipped = 0;
        foreach (ExportPlayer player in team.Players)
        {
            string fullName = $"{player.FirstName} {player.LastName}".Trim();
            if (rosterNames.Contains(fullName))
            {
                skipped++;
                continue;
            }

            IdName? person = await api.FindPersonAsync(player.FirstName, player.LastName);
            if (person is null)
                person = await api.CreatePersonAsync(player.FirstName, player.LastName);

            Guid? playerId = await api.FindPlayerByPersonAsync(person.Id, fullName);
            if (playerId is null)
                playerId = await api.CreatePlayerAsync(person.Id);

            string position = string.IsNullOrWhiteSpace(player.Position) ? "Forward" : player.Position;
            bool ok = await api.AddPlayerToTeamAsync(teamId, playerId.Value, position, player.JerseyNumber);
            if (ok)
            {
                rosterNames.Add(fullName);
                added++;
            }
            else
            {
                skipped++;
            }
        }

        Console.WriteLine($"    Roster {team.Name}: +{added} players ({skipped} skipped)");
    }

    private static async Task<bool> ReplayMatchEventsAsync(
        TargetApiClient api,
        Guid matchId,
        ExportMatch match,
        Guid homeTeamId,
        Guid awayTeamId,
        Guid refereeId,
        Dictionary<string, Dictionary<string, Guid>> rosterByTeam,
        ExportTournament tournament)
    {
        int goalCount = match.Goals?.Count ?? 0;
        int penaltyCount = match.Penalties?.Count ?? 0;
        int saveCount = match.Saves?.Count ?? 0;
        bool hasEvents = goalCount + penaltyCount + saveCount > 0;
        bool shouldComplete = string.Equals(match.Status, "Completed", StringComparison.OrdinalIgnoreCase);

        if (!hasEvents)
        {
            if (shouldComplete && match.HomeScore + match.AwayScore > 0)
            {
                Console.WriteLine(
                    $"  WARN: {match.HomeTeamName} vs {match.AwayTeamName} is completed {match.HomeScore}-{match.AwayScore} but has no events — left scheduled.");
            }

            return false;
        }

        await api.AddOfficialAsync(matchId, refereeId);

        Guid? homeGoalie = await ResolveGoalieAsync(api, match.HomeTeamName, match.HomeGoalieName, homeTeamId, rosterByTeam);
        Guid? awayGoalie = await ResolveGoalieAsync(api, match.AwayTeamName, match.AwayGoalieName, awayTeamId, rosterByTeam);
        if (homeGoalie is null || awayGoalie is null)
        {
            Console.WriteLine($"  WARN: skip events for {match.HomeTeamName} vs {match.AwayTeamName} — missing goalie");
            return false;
        }

        await api.SetGoalieAsync(matchId, homeTeamId, homeGoalie.Value);
        await api.SetGoalieAsync(matchId, awayTeamId, awayGoalie.Value);

        if (!await api.StartMatchAsync(matchId))
        {
            Console.WriteLine($"  WARN: could not start {match.HomeTeamName} vs {match.AwayTeamName} — events skipped");
            return false;
        }

        List<ReplayEvent> events = BuildReplayEvents(match);
        int regulationPeriods = string.Equals(match.TournamentStage, "GroupStage", StringComparison.OrdinalIgnoreCase)
            ? Math.Max(1, tournament.GroupStageNumberOfPeriods)
            : Math.Max(1, tournament.PlayoffNumberOfPeriods);
        int maxPeriod = events.Count > 0 ? events.Max(e => e.PeriodNumber) : regulationPeriods;
        maxPeriod = Math.Max(maxPeriod, regulationPeriods);

        bool overtimeStarted = false;
        bool shootoutStarted = false;
        int recordedGoals = 0;
        int recordedPenalties = 0;
        int recordedSaves = 0;

        for (int period = 1; period <= maxPeriod; period++)
        {
            await api.StartPeriodAsync(matchId, period);
            List<ReplayEvent> periodEvents = events
                .Where(e => e.PeriodNumber == period)
                .OrderBy(e => e.TimeInSeconds)
                .ToList();

            if (!overtimeStarted && periodEvents.Exists(e => e.WasInOvertime))
            {
                await api.RecordOvertimeAsync(matchId);
                overtimeStarted = true;
            }

            if (!shootoutStarted && periodEvents.Exists(e => e.WasInShootout))
            {
                await api.RecordShootoutAsync(matchId);
                shootoutStarted = true;
            }

            foreach (ReplayEvent ev in periodEvents)
            {
                Guid? teamId = ResolveTeamId(ev.TeamName, match, homeTeamId, awayTeamId);
                if (teamId is null)
                    continue;

                if (ev.Kind == ReplayKind.Goal)
                {
                    Guid? scorer = await EnsurePlayerAsync(api, ev.TeamName, ev.PlayerName, teamId.Value, rosterByTeam, "Forward");
                    if (scorer is null)
                        continue;
                    Guid? assister = await EnsurePlayerAsync(api, ev.TeamName, ev.AssisterName, teamId.Value, rosterByTeam, "Forward");
                    Guid? second = await EnsurePlayerAsync(api, ev.TeamName, ev.SecondaryAssisterName, teamId.Value, rosterByTeam, "Forward");
                    if (assister == scorer)
                        assister = null;
                    if (second == scorer || second == assister)
                        second = null;
                    if (await api.RecordGoalAsync(matchId, teamId.Value, scorer.Value, assister, second, ev.PeriodNumber, ev.TimeInSeconds))
                        recordedGoals++;
                }
                else if (ev.Kind == ReplayKind.Penalty)
                {
                    Guid? player = await EnsurePlayerAsync(api, ev.TeamName, ev.PlayerName, teamId.Value, rosterByTeam, "Forward");
                    if (player is null)
                        continue;
                    int minutes = Math.Clamp(ev.Minutes, 2, 20);
                    if (await api.RecordPenaltyAsync(
                        matchId,
                        teamId.Value,
                        player.Value,
                        minutes,
                        ev.PeriodNumber,
                        ev.TimeInSeconds,
                        string.IsNullOrWhiteSpace(ev.PenaltyType) ? "Minor" : ev.PenaltyType,
                        ev.Description))
                    {
                        recordedPenalties++;
                    }
                }
                else
                {
                    Guid? goalie = await EnsurePlayerAsync(api, ev.TeamName, ev.PlayerName, teamId.Value, rosterByTeam, "Goalkeeper")
                        ?? (string.Equals(ev.TeamName, match.HomeTeamName, StringComparison.OrdinalIgnoreCase) ? homeGoalie : awayGoalie);
                    if (goalie is null)
                        continue;
                    if (await api.RecordSaveAsync(
                        matchId,
                        teamId.Value,
                        goalie.Value,
                        ev.PeriodNumber,
                        ev.TimeInSeconds,
                        ev.WasInOvertime,
                        ev.WasInShootout))
                    {
                        recordedSaves++;
                    }
                }
            }

            await api.EndPeriodAsync(matchId, period);
        }

        if (shouldComplete || string.Equals(match.Status, "InProgress", StringComparison.OrdinalIgnoreCase) is false)
            await api.CompleteMatchAsync(matchId);

        Console.WriteLine(
            $"  Replayed {match.HomeTeamName} vs {match.AwayTeamName}: " +
            $"{recordedGoals}/{goalCount} goals, {recordedPenalties}/{penaltyCount} penalties, {recordedSaves}/{saveCount} saves");
        return true;
    }

    private static List<ReplayEvent> BuildReplayEvents(ExportMatch match)
    {
        List<ReplayEvent> events = [];
        foreach (ExportGoalEvent goal in match.Goals ?? [])
        {
            events.Add(new ReplayEvent(
                ReplayKind.Goal,
                goal.TeamName,
                goal.PlayerName,
                goal.AssisterName,
                goal.SecondaryAssisterName,
                goal.PeriodNumber,
                goal.TimeInSeconds,
                goal.WasInOvertime,
                goal.WasInShootout,
                0,
                null,
                null));
        }

        foreach (ExportPenaltyEvent penalty in match.Penalties ?? [])
        {
            events.Add(new ReplayEvent(
                ReplayKind.Penalty,
                penalty.TeamName,
                penalty.PlayerName,
                null,
                null,
                penalty.PeriodNumber,
                penalty.TimeInSeconds,
                false,
                false,
                penalty.Minutes,
                penalty.PenaltyType,
                penalty.Description));
        }

        foreach (ExportSaveEvent save in match.Saves ?? [])
        {
            events.Add(new ReplayEvent(
                ReplayKind.Save,
                save.TeamName,
                save.GoalieName,
                null,
                null,
                save.PeriodNumber,
                save.TimeInSeconds,
                save.WasInOvertime,
                save.WasInShootout,
                0,
                null,
                null));
        }

        return events;
    }

    private static Guid? ResolveTeamId(string teamName, ExportMatch match, Guid homeTeamId, Guid awayTeamId)
    {
        if (string.Equals(teamName, match.HomeTeamName, StringComparison.OrdinalIgnoreCase))
            return homeTeamId;
        if (string.Equals(teamName, match.AwayTeamName, StringComparison.OrdinalIgnoreCase))
            return awayTeamId;
        return null;
    }

    private static async Task<Guid?> ResolveGoalieAsync(
        TargetApiClient api,
        string teamName,
        string? goalieName,
        Guid teamId,
        Dictionary<string, Dictionary<string, Guid>> rosterByTeam)
    {
        Guid? named = await EnsurePlayerAsync(api, teamName, goalieName, teamId, rosterByTeam, "Goalkeeper");
        if (named is not null)
            return named;

        if (rosterByTeam.TryGetValue(teamName, out Dictionary<string, Guid>? roster) && roster.Count > 0)
            return roster.Values.First();

        SourceTeam? snapshot = await api.GetTeamAsync(teamId);
        SourceRosterPlayer? goalie = (snapshot?.Roster ?? [])
            .FirstOrDefault(p => string.Equals(p.Position, "Goalkeeper", StringComparison.OrdinalIgnoreCase))
            ?? snapshot?.Roster.FirstOrDefault(p => p.PlayerId != Guid.Empty);
        return goalie?.PlayerId is Guid id && id != Guid.Empty ? id : null;
    }

    private static async Task<Guid?> EnsurePlayerAsync(
        TargetApiClient api,
        string teamName,
        string? fullName,
        Guid teamId,
        Dictionary<string, Dictionary<string, Guid>> rosterByTeam,
        string position)
    {
        string? usable = ExportBuilder.UsablePlayerName(fullName);
        if (usable is null)
            return null;

        string trimmed = usable;
        if (!rosterByTeam.TryGetValue(teamName, out Dictionary<string, Guid>? roster))
        {
            roster = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            rosterByTeam[teamName] = roster;
        }

        if (roster.TryGetValue(trimmed, out Guid existing))
            return existing;

        string[] parts = trimmed.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string firstName = parts[0];
        string lastName = parts.Length > 1 ? parts[1] : parts[0];

        IdName? person = await api.FindPersonAsync(firstName, lastName);
        person ??= await api.CreatePersonAsync(firstName, lastName);

        Guid? playerId = await api.FindPlayerByPersonAsync(person.Id, trimmed);
        playerId ??= await api.CreatePlayerAsync(person.Id);

        await api.AddPlayerToTeamAsync(teamId, playerId.Value, position, null);
        roster[trimmed] = playerId.Value;
        return playerId.Value;
    }

    private static string? NonEmpty(string? value)
    {
        string? trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static string? ComposeVenue(string? baseVenue, string? field)
    {
        if (!string.IsNullOrWhiteSpace(baseVenue) && !string.IsNullOrWhiteSpace(field))
            return $"{baseVenue} - Kenttä {field}";
        if (!string.IsNullOrWhiteSpace(baseVenue))
            return baseVenue;
        if (!string.IsNullOrWhiteSpace(field))
            return $"Kenttä {field}";
        return null;
    }

    private enum ReplayKind
    {
        Goal,
        Penalty,
        Save,
    }

    private sealed record ReplayEvent(
        ReplayKind Kind,
        string TeamName,
        string? PlayerName,
        string? AssisterName,
        string? SecondaryAssisterName,
        int PeriodNumber,
        int TimeInSeconds,
        bool WasInOvertime,
        bool WasInShootout,
        int Minutes,
        string? PenaltyType,
        string? Description);
}
