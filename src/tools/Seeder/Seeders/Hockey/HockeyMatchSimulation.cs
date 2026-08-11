using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Matches;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace Seeder;

/// <summary>
/// Shared helpers to confirm roster, start, record a few events, finish, and recalculate stats.
/// </summary>
public static class HockeyMatchSimulation
{
    public static async Task<HockeyMatchDto> SimulateCompletedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyMatchDto match,
        Dictionary<Guid, HockeyTeamDto> rosterCache)
    {
        if (string.Equals(match.Status, "Finished", StringComparison.OrdinalIgnoreCase))
        {
            return match;
        }

        match = await GetMatchAsync(http, jsonOptions, match.Id) ?? match;

        foreach (HockeyMatchTeamDto side in match.MatchTeams)
        {
            if (side.IsConfirmedRoster)
            {
                continue;
            }

            HockeyTeamDto? team = await GetTeamAsync(http, jsonOptions, side.TeamId, rosterCache);
            if (team?.Roster == null || team.Roster.Count == 0)
            {
                throw new InvalidOperationException("Cannot simulate match " + match.Id + ": empty roster for team " + side.TeamId);
            }

            List<Guid> teamPlayerIds = PickMinimalRoster(team);
            ConfirmHockeyMatchRosterRequest rosterReq = new ConfirmHockeyMatchRosterRequest
            {
                MatchTeamId = side.Id,
                TeamPlayerIds = teamPlayerIds,
                Source = HockeyPlayerSelectionSource.Manual
            };

            HttpResponseMessage rosterResp = await http.PostAsJsonAsync("api/HockeyMatch/" + match.Id + "/roster/confirm", rosterReq);
            await SeederHttp.EnsureSuccessWithBody(rosterResp, "Confirm Hockey Match Roster");

            ApiResponse<HockeyMatchDto>? rosterApi = await rosterResp.Content.ReadFromJsonAsync<ApiResponse<HockeyMatchDto>>(jsonOptions);
            if (rosterApi?.Data != null)
            {
                match = rosterApi.Data;
            }
        }

        if (!string.Equals(match.Status, "InProgress", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(match.Status, "Warmup", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(match.Status, "Intermission", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(match.Status, "Overtime", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(match.Status, "Shootout", StringComparison.OrdinalIgnoreCase))
        {
            HttpResponseMessage startResp = await http.PostAsJsonAsync(
                "api/HockeyMatch/" + match.Id + "/start",
                new MarkHockeyMatchStartedRequest { ActualStartTime = DateTime.UtcNow.AddHours(-2) });
            await SeederHttp.EnsureSuccessWithBody(startResp, "Start Hockey Match");
            ApiResponse<HockeyMatchDto>? startApi = await startResp.Content.ReadFromJsonAsync<ApiResponse<HockeyMatchDto>>(jsonOptions);
            if (startApi?.Data != null)
            {
                match = startApi.Data;
            }
        }

        match = await GetMatchAsync(http, jsonOptions, match.Id) ?? match;
        await RecordLightEventsAsync(http, jsonOptions, match);

        HttpResponseMessage finishResp = await http.PostAsJsonAsync(
            "api/HockeyMatch/" + match.Id + "/finish",
            new MarkHockeyMatchFinishedRequest
            {
                ActualEndTime = DateTime.UtcNow.AddHours(-1),
                ResultType = HockeyMatchResultType.HomeWin
            });
        await SeederHttp.EnsureSuccessWithBody(finishResp, "Finish Hockey Match");

        ApiResponse<HockeyMatchDto>? finishApi = await finishResp.Content.ReadFromJsonAsync<ApiResponse<HockeyMatchDto>>(jsonOptions);
        if (finishApi?.Data != null)
        {
            match = finishApi.Data;
        }

        await RecalculateMatchAsync(http, match.Id);
        return await GetMatchAsync(http, jsonOptions, match.Id) ?? match;
    }

    public static async Task RecalculateMatchAsync(HttpClient http, Guid matchId)
    {
        HttpResponseMessage resp = await http.PostAsync("api/HockeyStatistics/matches/" + matchId + "/recalculate", content: null);
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine("Warning: match stats recalculate failed for " + matchId + " (" + (int)resp.StatusCode + "): " + Truncate(body));
            return;
        }

        Console.WriteLine("Recalculated hockey match statistics for " + matchId);
    }

    public static async Task RecalculateCompetitionAsync(HttpClient http, Guid competitionId)
    {
        HttpResponseMessage resp = await http.PostAsJsonAsync(
            "api/HockeyStatistics/competitions/" + competitionId + "/recalculate",
            new { });
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine("Warning: competition stats recalculate failed for " + competitionId + " (" + (int)resp.StatusCode + "): " + Truncate(body));
            return;
        }

        Console.WriteLine("Recalculated hockey competition statistics for " + competitionId);
    }

    private static List<Guid> PickMinimalRoster(HockeyTeamDto team)
    {
        List<HockeyTeamPlayerDto> active = team.Roster.Where(p => p.IsActive).ToList();
        HockeyTeamPlayerDto? goalie = active.FirstOrDefault(p =>
            string.Equals(p.Position, "Goalie", StringComparison.OrdinalIgnoreCase));
        List<HockeyTeamPlayerDto> skaters = active
            .Where(p => !string.Equals(p.Position, "Goalie", StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .ToList();

        List<Guid> ids = new List<Guid>();
        if (goalie != null)
        {
            ids.Add(goalie.Id);
        }
        ids.AddRange(skaters.Select(s => s.Id));

        if (ids.Count == 0)
        {
            ids.AddRange(active.Take(6).Select(p => p.Id));
        }

        return ids.Distinct().ToList();
    }

    private static async Task RecordLightEventsAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyMatchDto match)
    {
        HockeyMatchTeamDto? home = match.MatchTeams.FirstOrDefault(t =>
            string.Equals(t.TeamSlot, "Home", StringComparison.OrdinalIgnoreCase));
        HockeyMatchTeamDto? away = match.MatchTeams.FirstOrDefault(t =>
            string.Equals(t.TeamSlot, "Away", StringComparison.OrdinalIgnoreCase));
        if (home == null || away == null)
        {
            return;
        }

        HockeyMatchActivePlayerDto? homeScorer = home.ActivePlayers.FirstOrDefault(p => !p.IsGoalie);
        HockeyMatchActivePlayerDto? awayScorer = away.ActivePlayers.FirstOrDefault(p => !p.IsGoalie);
        // ConfirmRoster currently does not set IsGoalie; fall back to any non-first player / any player.
        homeScorer ??= home.ActivePlayers.Skip(1).FirstOrDefault() ?? home.ActivePlayers.FirstOrDefault();
        awayScorer ??= away.ActivePlayers.Skip(1).FirstOrDefault() ?? away.ActivePlayers.FirstOrDefault();

        if (homeScorer != null)
        {
            RecordHockeyShotRequest shotReq = new RecordHockeyShotRequest
            {
                ShootingMatchTeamId = home.Id,
                PeriodNumber = 1,
                TimeInSeconds = 120,
                ShotResult = HockeyShotResult.Saved,
                CountsAsShotOnGoal = true,
                ShooterActivePlayerId = homeScorer.Id
            };
            HttpResponseMessage shotResp = await http.PostAsJsonAsync("api/HockeyMatch/" + match.Id + "/events/shots", shotReq);
            if (!shotResp.IsSuccessStatusCode)
            {
                Console.WriteLine("Warning: shot event failed: " + await shotResp.Content.ReadAsStringAsync());
            }

            RecordHockeyGoalRequest goalReq = new RecordHockeyGoalRequest
            {
                ScoringMatchTeamId = home.Id,
                ScorerActivePlayerId = homeScorer.Id,
                PeriodNumber = 1,
                TimeInSeconds = 320,
                GoalStrength = HockeyGoalStrength.EvenStrength
            };
            HttpResponseMessage goalResp = await http.PostAsJsonAsync("api/HockeyMatch/" + match.Id + "/events/goals", goalReq);
            if (!goalResp.IsSuccessStatusCode)
            {
                Console.WriteLine("Warning: goal event failed: " + await goalResp.Content.ReadAsStringAsync());
            }
        }

        if (awayScorer != null)
        {
            RecordHockeyGoalRequest goalReq = new RecordHockeyGoalRequest
            {
                ScoringMatchTeamId = away.Id,
                ScorerActivePlayerId = awayScorer.Id,
                PeriodNumber = 2,
                TimeInSeconds = 410,
                GoalStrength = HockeyGoalStrength.EvenStrength
            };
            HttpResponseMessage goalResp = await http.PostAsJsonAsync("api/HockeyMatch/" + match.Id + "/events/goals", goalReq);
            if (!goalResp.IsSuccessStatusCode)
            {
                Console.WriteLine("Warning: away goal event failed: " + await goalResp.Content.ReadAsStringAsync());
            }
        }
    }

    private static async Task<HockeyMatchDto?> GetMatchAsync(HttpClient http, JsonSerializerOptions jsonOptions, Guid matchId)
    {
        HttpResponseMessage resp = await http.GetAsync("api/HockeyMatch/" + matchId);
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        ApiResponse<HockeyMatchDto>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<HockeyMatchDto>>(jsonOptions);
        return api?.Data;
    }

    private static async Task<HockeyTeamDto?> GetTeamAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        Guid teamId,
        Dictionary<Guid, HockeyTeamDto> cache)
    {
        if (cache.TryGetValue(teamId, out HockeyTeamDto? cached))
        {
            return cached;
        }

        HttpResponseMessage resp = await http.GetAsync("api/HockeyTeam/" + teamId);
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        ApiResponse<HockeyTeamDto>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<HockeyTeamDto>>(jsonOptions);
        if (api?.Data != null)
        {
            cache[teamId] = api.Data;
        }
        return api?.Data;
    }

    private static string Truncate(string body) => body.Length > 300 ? body.Substring(0, 300) + "..." : body;
}
