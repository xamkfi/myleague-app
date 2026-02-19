using Application.DTOs.Floorball;
using Application.Features.Floorball.Seasons.DTOs;
using MahlImporter.Models;

namespace MahlImporter.Import;

public class MatchImporter
{
    private readonly ApiClient _api;
    private readonly ImportLogger _log;
    private readonly int _yearsToAdd;

    public MatchImporter(ApiClient api, ImportLogger log, int yearsToAdd)
    {
        _api = api;
        _log = log;
        _yearsToAdd = yearsToAdd;
    }

    public async Task ImportAllMatchesAsync(
        List<ScrapedMatch> matches,
        FloorballSeasonDto season,
        Dictionary<string, FloorballTeamDto> teamMap,
        Dictionary<string, (Guid PersonId, Guid PlayerId)> playerMap,
        List<ScrapedTeam> scrapedTeams,
        Guid refereeId)
    {
        Console.WriteLine($"\n=== Importing {matches.Count} Matches ===\n");

        int success = 0;
        int failed = 0;

        for (int i = 0; i < matches.Count; i++)
        {
            ScrapedMatch sm = matches[i];
            Console.WriteLine($"[{i + 1}/{matches.Count}] {sm.HomeTeamName} vs {sm.AwayTeamName} ({sm.HomeScore}-{sm.AwayScore})");

            try
            {
                bool ok = await ImportSingleMatchAsync(sm, season, teamMap, playerMap, scrapedTeams, refereeId);
                if (ok) success++; else failed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR: {ex.Message}");
                _log.LogError($"Match {sm.HomeTeamName} vs {sm.AwayTeamName}", new { sm.MahlMatchId, sm.HomeTeamName, sm.AwayTeamName, sm.HomeScore, sm.AwayScore }, ex.Message);
                failed++;
            }
        }

        Console.WriteLine($"\nMatch import complete: {success} succeeded, {failed} failed.");
        if (_log.ErrorCount > 0)
        {
            Console.WriteLine($"  {_log.ErrorCount} errors logged to: {_log.LogPath}");
        }
    }

    private async Task<bool> ImportSingleMatchAsync(
        ScrapedMatch sm,
        FloorballSeasonDto season,
        Dictionary<string, FloorballTeamDto> teamMap,
        Dictionary<string, (Guid PersonId, Guid PlayerId)> playerMap,
        List<ScrapedTeam> scrapedTeams,
        Guid refereeId)
    {
        if (!teamMap.TryGetValue(sm.HomeTeamName, out FloorballTeamDto? homeTeam))
        {
            Console.WriteLine($"  SKIP: Home team '{sm.HomeTeamName}' not found.");
            _log.LogError("Match team lookup", new { sm.MahlMatchId, Team = sm.HomeTeamName, Side = "Home" }, $"Home team '{sm.HomeTeamName}' not found in teamMap.");
            return false;
        }
        if (!teamMap.TryGetValue(sm.AwayTeamName, out FloorballTeamDto? awayTeam))
        {
            Console.WriteLine($"  SKIP: Away team '{sm.AwayTeamName}' not found.");
            _log.LogError("Match team lookup", new { sm.MahlMatchId, Team = sm.AwayTeamName, Side = "Away" }, $"Away team '{sm.AwayTeamName}' not found in teamMap.");
            return false;
        }

        DateTime scheduledDate = AdjustDate(sm.OriginalDate);
        if (scheduledDate <= DateTime.UtcNow)
        {
            scheduledDate = DateTime.UtcNow.AddDays(30 + (teamMap.Count * 10));
        }

        // Step 1: Create match
        FloorballMatchDto? match = await _api.CreateMatchAsync(season.Id, homeTeam.Id, awayTeam.Id, scheduledDate, sm.Venue);
        if (match == null)
        {
            Console.WriteLine("  FAIL: Could not create match.");
            _log.LogError("CreateMatch", new { sm.MahlMatchId, sm.HomeTeamName, sm.AwayTeamName, scheduledDate, sm.Venue }, "API returned null - match creation failed.");
            return false;
        }
        Console.WriteLine($"  Created match: {match.Id}");

        // Step 2: Add referee
        await _api.AddOfficialToMatchAsync(match.Id, refereeId);

        // Step 3: Set goalies
        Guid homeGoalieId = FindGoaliePlayerId(sm.HomeTeamName, scrapedTeams, playerMap, homeTeam);
        Guid awayGoalieId = FindGoaliePlayerId(sm.AwayTeamName, scrapedTeams, playerMap, awayTeam);

        if (homeGoalieId == Guid.Empty || awayGoalieId == Guid.Empty)
        {
            Console.WriteLine("  SKIP: Could not find goalies for both teams. Match left as Scheduled.");
            _log.LogError("GoalieLookup", new { MatchId = match.Id, sm.HomeTeamName, sm.AwayTeamName, HomeGoalieFound = homeGoalieId != Guid.Empty, AwayGoalieFound = awayGoalieId != Guid.Empty }, "Could not find goalies for both teams.");
            return true;
        }

        await _api.SetGoalieAsync(match.Id, homeTeam.Id, homeGoalieId);
        await _api.SetGoalieAsync(match.Id, awayTeam.Id, awayGoalieId);

        // Step 4: Start match
        bool started = await _api.StartMatchAsync(match.Id);
        if (!started)
        {
            Console.WriteLine("  WARN: Could not start match. Events will be skipped.");
            _log.LogError("StartMatch", new { MatchId = match.Id, sm.HomeTeamName, sm.AwayTeamName }, "Could not start match - events skipped.");
            return true;
        }

        // Step 5-10: Periods and events
        List<ScrapedGoal> period1Goals = sm.Goals.Where(g => TotalMinutes(g.TimeMinutes, g.TimeSeconds) < 15).ToList();
        List<ScrapedGoal> period2Goals = sm.Goals.Where(g => TotalMinutes(g.TimeMinutes, g.TimeSeconds) >= 15).ToList();
        List<ScrapedPenalty> period1Penalties = sm.Penalties.Where(p => TotalMinutes(p.TimeMinutes, p.TimeSeconds) < 15).ToList();
        List<ScrapedPenalty> period2Penalties = sm.Penalties.Where(p => TotalMinutes(p.TimeMinutes, p.TimeSeconds) >= 15).ToList();

        // Period 1
        bool p1 = await _api.StartPeriodAsync(match.Id, 1);
        if (!p1) Console.WriteLine("  WARN: StartPeriod 1 failed.");
        await RecordGoalsAsync(match.Id, period1Goals, 1, sm, teamMap, playerMap);
        await RecordPenaltiesAsync(match.Id, period1Penalties, 1, sm, teamMap, playerMap);
        await _api.EndPeriodAsync(match.Id, 1);

        // Period 2
        bool p2 = await _api.StartPeriodAsync(match.Id, 2);
        if (!p2) Console.WriteLine("  WARN: StartPeriod 2 failed.");
        await RecordGoalsAsync(match.Id, period2Goals, 2, sm, teamMap, playerMap);
        await RecordPenaltiesAsync(match.Id, period2Penalties, 2, sm, teamMap, playerMap);
        await _api.EndPeriodAsync(match.Id, 2);

        // Step 11: Complete match
        await _api.CompleteMatchAsync(match.Id);
        Console.WriteLine($"  Completed. Goals: {sm.Goals.Count}, Penalties: {sm.Penalties.Count}");

        return true;
    }

    private async Task RecordGoalsAsync(
        Guid matchId,
        List<ScrapedGoal> goals,
        int periodNumber,
        ScrapedMatch sm,
        Dictionary<string, FloorballTeamDto> teamMap,
        Dictionary<string, (Guid PersonId, Guid PlayerId)> playerMap)
    {
        foreach (ScrapedGoal goal in goals.OrderBy(g => g.TimeMinutes * 60 + g.TimeSeconds))
        {
            string teamName = !string.IsNullOrEmpty(goal.TeamName) ? goal.TeamName : sm.HomeTeamName;
            FloorballTeamDto? scoringTeam = ResolveTeam(teamName, sm, teamMap);

            Guid scorerId = ResolvePlayerId(goal.ScorerName, playerMap);
            if (scorerId == Guid.Empty)
            {
                Console.WriteLine($"    SKIP goal: scorer '{goal.ScorerName}' not found in playerMap.");
                _log.LogError("RecordGoal", new { MatchId = matchId, Scorer = goal.ScorerName, Team = teamName, Period = periodNumber, Time = $"{goal.TimeMinutes}:{goal.TimeSeconds:D2}" }, $"Scorer '{goal.ScorerName}' not found in playerMap.");
                continue;
            }

            Guid? assisterId = null;
            if (!string.IsNullOrEmpty(goal.AssisterName))
            {
                Guid aid = ResolvePlayerId(goal.AssisterName, playerMap);
                if (aid != Guid.Empty) assisterId = aid;
            }

            int timeInSeconds = periodNumber == 1
                ? goal.TimeMinutes * 60 + goal.TimeSeconds
                : (goal.TimeMinutes - 15) * 60 + goal.TimeSeconds;

            if (timeInSeconds < 0) timeInSeconds = 0;
            if (timeInSeconds > 900) timeInSeconds = 900;

            Console.WriteLine($"    Goal P{periodNumber} {timeInSeconds}s: team={scoringTeam.Name}({scoringTeam.Id}), scorer={goal.ScorerName}({scorerId}), assister={goal.AssisterName ?? "none"}({assisterId?.ToString() ?? "-"})");
            bool ok = await _api.RecordGoalAsync(matchId, scoringTeam.Id, scorerId, assisterId, periodNumber, timeInSeconds);
            if (!ok)
            {
                Console.WriteLine($"    ^ Failed for above goal. Check server logs for details.");
                _log.LogError("RecordGoal API", new { MatchId = matchId, TeamId = scoringTeam.Id, Team = scoringTeam.Name, ScorerId = scorerId, Scorer = goal.ScorerName, AssisterId = assisterId, Assister = goal.AssisterName, Period = periodNumber, TimeInSeconds = timeInSeconds }, "API call to RecordGoal returned failure.");
            }
        }
    }

    private async Task RecordPenaltiesAsync(
        Guid matchId,
        List<ScrapedPenalty> penalties,
        int periodNumber,
        ScrapedMatch sm,
        Dictionary<string, FloorballTeamDto> teamMap,
        Dictionary<string, (Guid PersonId, Guid PlayerId)> playerMap)
    {
        foreach (ScrapedPenalty penalty in penalties.OrderBy(p => p.TimeMinutes * 60 + p.TimeSeconds))
        {
            string teamName = !string.IsNullOrEmpty(penalty.TeamName) ? penalty.TeamName : sm.HomeTeamName;
            FloorballTeamDto? penaltyTeam = ResolveTeam(teamName, sm, teamMap);

            Guid playerId = ResolvePlayerId(penalty.PlayerName, playerMap);
            if (playerId == Guid.Empty)
            {
                Console.WriteLine($"    SKIP penalty: player '{penalty.PlayerName}' not found.");
                _log.LogError("RecordPenalty", new { MatchId = matchId, Player = penalty.PlayerName, Team = teamName, Period = periodNumber, Time = $"{penalty.TimeMinutes}:{penalty.TimeSeconds:D2}", Duration = penalty.DurationMinutes }, $"Player '{penalty.PlayerName}' not found in playerMap.");
                continue;
            }

            int timeInSeconds = periodNumber == 1
                ? penalty.TimeMinutes * 60 + penalty.TimeSeconds
                : (penalty.TimeMinutes - 15) * 60 + penalty.TimeSeconds;

            if (timeInSeconds < 0) timeInSeconds = 0;
            if (timeInSeconds > 900) timeInSeconds = 900;

            bool penOk = await _api.RecordPenaltyAsync(matchId, penaltyTeam.Id, playerId, penalty.DurationMinutes, periodNumber, timeInSeconds);
            if (!penOk)
            {
                _log.LogError("RecordPenalty API", new { MatchId = matchId, TeamId = penaltyTeam.Id, Team = penaltyTeam.Name, PlayerId = playerId, Player = penalty.PlayerName, Duration = penalty.DurationMinutes, Period = periodNumber, TimeInSeconds = timeInSeconds }, "API call to RecordPenalty returned failure.");
            }
        }
    }

    private static Guid FindGoaliePlayerId(
        string teamName,
        List<ScrapedTeam> scrapedTeams,
        Dictionary<string, (Guid PersonId, Guid PlayerId)> playerMap,
        FloorballTeamDto team)
    {
        ScrapedTeam? st = scrapedTeams.FirstOrDefault(t => string.Equals(t.Name, teamName, StringComparison.OrdinalIgnoreCase));
        if (st == null) return Guid.Empty;

        ScrapedPlayer? goalie = st.Players.FirstOrDefault(p => p.IsGoalkeeper);
        if (goalie != null)
        {
            string name = $"{goalie.FirstName} {goalie.LastName}";
            if (playerMap.TryGetValue(name, out (Guid PersonId, Guid PlayerId) ids))
                return ids.PlayerId;
        }

        foreach (ScrapedPlayer sp in st.Players)
        {
            string name = $"{sp.FirstName} {sp.LastName}";
            if (playerMap.TryGetValue(name, out (Guid PersonId, Guid PlayerId) ids))
                return ids.PlayerId;
        }

        return Guid.Empty;
    }

    private static Guid ResolvePlayerId(string playerName, Dictionary<string, (Guid PersonId, Guid PlayerId)> playerMap)
    {
        if (string.IsNullOrWhiteSpace(playerName)) return Guid.Empty;

        if (playerMap.TryGetValue(playerName, out (Guid PersonId, Guid PlayerId) exact))
            return exact.PlayerId;

        string normalized = playerName.Trim();
        foreach (KeyValuePair<string, (Guid PersonId, Guid PlayerId)> kvp in playerMap)
        {
            if (string.Equals(kvp.Key, normalized, StringComparison.OrdinalIgnoreCase))
                return kvp.Value.PlayerId;
        }

        return Guid.Empty;
    }

    private static FloorballTeamDto ResolveTeam(
        string teamName,
        ScrapedMatch sm,
        Dictionary<string, FloorballTeamDto> teamMap)
    {
        if (teamMap.TryGetValue(teamName, out FloorballTeamDto? team))
            return team;

        foreach (KeyValuePair<string, FloorballTeamDto> kvp in teamMap)
        {
            if (kvp.Key.Contains(teamName, StringComparison.OrdinalIgnoreCase) ||
                teamName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        if (teamMap.TryGetValue(sm.HomeTeamName, out FloorballTeamDto? homeTeam))
            return homeTeam;

        return teamMap.Values.First();
    }

    private DateTime AdjustDate(DateTime original)
    {
        if (original == default)
            return DateTime.UtcNow.AddMonths(3);

        return original.AddYears(_yearsToAdd);
    }

    private static double TotalMinutes(int minutes, int seconds) => minutes + seconds / 60.0;
}
