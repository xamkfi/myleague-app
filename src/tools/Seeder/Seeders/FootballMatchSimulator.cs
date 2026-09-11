using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Teams.DTOs;
using Domain.Enums.Football;
using WebAPI.Models.Common;
using WebAPI.Models.Football;

namespace Seeder;

/// <summary>
/// Simulates a football match through lineup → start → goals/cards/subs → complete.
/// Failures are thrown to the caller so season/tournament seeders can catch per match.
/// </summary>
public static class FootballMatchSimulator
{
    private const int GoalsPerMatchMin = 2;
    private const int GoalsPerMatchMax = 6;

    public static async Task SimulateCompletedMatchAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        FootballMatchDto match,
        string homeTeamName,
        string awayTeamName,
        Dictionary<Guid, FootballTeamDto> rosterCache,
        Random rng)
    {
        if (match.Status == FootballMatchStatus.Completed)
        {
            Console.WriteLine($"  Skipping simulation for {homeTeamName} vs {awayTeamName}: already completed.");
            return;
        }

        if (match.Status == FootballMatchStatus.InProgress)
        {
            HttpResponseMessage completeInProgress = await http.PutAsync($"api/football-matches/{match.Id}/complete", content: null);
            await SeederHttp.EnsureSuccessWithBody(completeInProgress, "Complete Football Match");
            Console.WriteLine($"  Completed in-progress match: {homeTeamName} vs {awayTeamName}");
            return;
        }

        if (!match.HomeTeamId.HasValue || !match.AwayTeamId.HasValue)
        {
            Console.WriteLine($"  Skipping simulation for {homeTeamName} vs {awayTeamName}: match is missing a team assignment.");
            return;
        }

        Guid homeTeamId = match.HomeTeamId.Value;
        Guid awayTeamId = match.AwayTeamId.Value;

        FootballTeamDto? homeTeam = await GetTeamWithRosterAsync(http, jsonOptions, homeTeamId, rosterCache);
        FootballTeamDto? awayTeam = await GetTeamWithRosterAsync(http, jsonOptions, awayTeamId, rosterCache);
        if (homeTeam == null || awayTeam == null)
        {
            Console.WriteLine($"  Skipping simulation for {homeTeamName} vs {awayTeamName}: missing team rosters.");
            return;
        }

        int playersOnField = match.MatchRules.PlayersOnField;
        bool requireGoalkeeper = match.MatchRules.RequireGoalkeeper;
        int halfSeconds = Math.Max(60, match.MatchRules.HalfDurationMinutes * 60);
        int halves = Math.Max(1, match.MatchRules.NumberOfHalves);

        List<LineupPlayerRequest>? homeLineup = BuildLineup(homeTeam, playersOnField, requireGoalkeeper);
        List<LineupPlayerRequest>? awayLineup = BuildLineup(awayTeam, playersOnField, requireGoalkeeper);
        if (homeLineup == null || awayLineup == null)
        {
            Console.WriteLine($"  Skipping simulation for {homeTeamName} vs {awayTeamName}: roster cannot satisfy {playersOnField} on-field players.");
            return;
        }

        await SetLineupAsync(http, match.Id, homeTeamId, homeLineup);
        await SetLineupAsync(http, match.Id, awayTeamId, awayLineup);

        HttpResponseMessage startResp = await http.PutAsync($"api/football-matches/{match.Id}/start", content: null);
        await SeederHttp.EnsureSuccessWithBody(startResp, "Start Football Match");

        HashSet<Guid> homeOnField = homeLineup.Where(p => p.IsOnField).Select(p => p.PlayerId).ToHashSet();
        HashSet<Guid> awayOnField = awayLineup.Where(p => p.IsOnField).Select(p => p.PlayerId).ToHashSet();
        Guid? homeGkId = homeLineup.FirstOrDefault(p => p.IsOnField && p.Position == FootballPosition.Goalkeeper)?.PlayerId;
        Guid? awayGkId = awayLineup.FirstOrDefault(p => p.IsOnField && p.Position == FootballPosition.Goalkeeper)?.PlayerId;

        int totalGoals = rng.Next(GoalsPerMatchMin, GoalsPerMatchMax + 1);
        int homeGoals = rng.Next(0, totalGoals + 1);
        int awayGoals = totalGoals - homeGoals;

        await RecordGoalsAsync(http, match.Id, homeTeamId, homeOnField, homeGkId, homeGoals, halves, halfSeconds, rng);
        await RecordGoalsAsync(http, match.Id, awayTeamId, awayOnField, awayGkId, awayGoals, halves, halfSeconds, rng);

        if (rng.Next(0, 100) < 55)
        {
            await TryRecordCardAsync(http, match.Id, homeTeamId, homeOnField, homeGkId, halves, halfSeconds, rng);
        }

        if (rng.Next(0, 100) < 40)
        {
            await TryRecordCardAsync(http, match.Id, awayTeamId, awayOnField, awayGkId, halves, halfSeconds, rng);
        }

        if (rng.Next(0, 100) < 50)
        {
            await TryRecordSubstitutionAsync(http, match.Id, homeTeamId, homeLineup, homeOnField, homeGkId, requireGoalkeeper, halves, halfSeconds, rng);
        }

        if (rng.Next(0, 100) < 40)
        {
            await TryRecordSubstitutionAsync(http, match.Id, awayTeamId, awayLineup, awayOnField, awayGkId, requireGoalkeeper, halves, halfSeconds, rng);
        }

        HttpResponseMessage completeResp = await http.PutAsync($"api/football-matches/{match.Id}/complete", content: null);
        await SeederHttp.EnsureSuccessWithBody(completeResp, "Complete Football Match");

        Console.WriteLine($"  Completed: {homeTeamName} {homeGoals} - {awayGoals} {awayTeamName}");
    }

    public static bool IsPastScheduled(DateTime scheduledUtc)
    {
        DateTime utc = scheduledUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(scheduledUtc, DateTimeKind.Utc)
            : scheduledUtc.ToUniversalTime();
        return utc <= DateTime.UtcNow.AddMinutes(-5);
    }

    private static async Task<FootballTeamDto?> GetTeamWithRosterAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        Guid teamId,
        Dictionary<Guid, FootballTeamDto> cache)
    {
        if (cache.TryGetValue(teamId, out FootballTeamDto? cached))
        {
            return cached;
        }

        HttpResponseMessage resp = await http.GetAsync($"api/FootballTeam/{teamId}");
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        ApiResponse<FootballTeamDto>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<FootballTeamDto>>(jsonOptions);
        if (api?.Data == null)
        {
            return null;
        }

        cache[teamId] = api.Data;
        return api.Data;
    }

    private static List<LineupPlayerRequest>? BuildLineup(FootballTeamDto team, int playersOnField, bool requireGoalkeeper)
    {
        List<FootballTeamPlayerDto> roster = team.Roster
            .Where(p => p.IsActive)
            .ToList();
        if (roster.Count == 0)
        {
            roster = team.Roster.ToList();
        }

        if (roster.Count < playersOnField)
        {
            return null;
        }

        List<FootballTeamPlayerDto> goalkeepers = roster
            .Where(p => p.Position == FootballPosition.Goalkeeper)
            .OrderBy(p => p.JerseyNumber ?? int.MaxValue)
            .ToList();
        List<FootballTeamPlayerDto> fieldPlayers = roster
            .Where(p => p.Position != FootballPosition.Goalkeeper)
            .OrderBy(p => p.JerseyNumber ?? int.MaxValue)
            .ToList();

        int fieldSlots = requireGoalkeeper ? playersOnField - 1 : playersOnField;
        if (requireGoalkeeper && goalkeepers.Count == 0)
        {
            return null;
        }

        if (fieldPlayers.Count < fieldSlots)
        {
            return null;
        }

        List<LineupPlayerRequest> lineup = new List<LineupPlayerRequest>();
        HashSet<Guid> used = new HashSet<Guid>();

        if (requireGoalkeeper)
        {
            FootballTeamPlayerDto gk = goalkeepers[0];
            lineup.Add(new LineupPlayerRequest
            {
                PlayerId = gk.PlayerId,
                Position = FootballPosition.Goalkeeper,
                IsOnField = true
            });
            used.Add(gk.PlayerId);
        }

        foreach (FootballTeamPlayerDto field in fieldPlayers.Take(fieldSlots))
        {
            lineup.Add(new LineupPlayerRequest
            {
                PlayerId = field.PlayerId,
                Position = field.Position,
                IsOnField = true
            });
            used.Add(field.PlayerId);
        }

        foreach (FootballTeamPlayerDto extra in roster)
        {
            if (used.Contains(extra.PlayerId))
            {
                continue;
            }

            lineup.Add(new LineupPlayerRequest
            {
                PlayerId = extra.PlayerId,
                Position = extra.Position,
                IsOnField = false
            });
        }

        int onFieldCount = lineup.Count(p => p.IsOnField);
        if (onFieldCount != playersOnField)
        {
            return null;
        }

        return lineup;
    }

    private static async Task SetLineupAsync(HttpClient http, Guid matchId, Guid teamId, List<LineupPlayerRequest> players)
    {
        SetMatchLineupRequest request = new SetMatchLineupRequest { Players = players };
        HttpResponseMessage resp = await http.PutAsJsonAsync($"api/football-matches/{matchId}/teams/{teamId}/lineup", request);
        await SeederHttp.EnsureSuccessWithBody(resp, "Set Football Match Lineup");
    }

    private static async Task RecordGoalsAsync(
        HttpClient http,
        Guid matchId,
        Guid teamId,
        HashSet<Guid> onField,
        Guid? gkId,
        int goalCount,
        int halves,
        int halfSeconds,
        Random rng)
    {
        List<Guid> scorers = onField.Where(id => gkId == null || id != gkId.Value).ToList();
        if (scorers.Count == 0)
        {
            scorers = onField.ToList();
        }

        if (scorers.Count == 0)
        {
            return;
        }

        for (int g = 0; g < goalCount; g++)
        {
            Guid scorerId = scorers[rng.Next(scorers.Count)];
            Guid? assistId = null;
            List<Guid> assistCandidates = scorers.Where(id => id != scorerId).ToList();
            if (assistCandidates.Count > 0 && rng.Next(0, 100) < 70)
            {
                assistId = assistCandidates[rng.Next(assistCandidates.Count)];
            }

            RecordGoalRequest goalReq = new RecordGoalRequest
            {
                MatchId = matchId,
                ScoringTeamId = teamId,
                ScoringPlayerId = scorerId,
                AssistingPlayerId = assistId,
                PeriodNumber = rng.Next(1, halves + 1),
                TimeInSeconds = rng.Next(30, halfSeconds),
                Description = "Seed goal",
                GoalType = null
            };

            HttpResponseMessage resp = await http.PostAsJsonAsync($"api/football-matches/{matchId}/events/goal", goalReq);
            await SeederHttp.EnsureSuccessWithBody(resp, "Record Football Goal");
            await Task.Delay(60);
        }
    }

    private static async Task TryRecordCardAsync(
        HttpClient http,
        Guid matchId,
        Guid teamId,
        HashSet<Guid> onField,
        Guid? gkId,
        int halves,
        int halfSeconds,
        Random rng)
    {
        List<Guid> candidates = onField.Where(id => gkId == null || id != gkId.Value).ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        Guid playerId = candidates[rng.Next(candidates.Count)];
        RecordCardEventRequest cardReq = new RecordCardEventRequest
        {
            MatchId = matchId,
            TeamId = teamId,
            PlayerId = playerId,
            CardType = FootballCardType.Yellow,
            PeriodNumber = rng.Next(1, halves + 1),
            TimeInSeconds = rng.Next(30, halfSeconds),
            Description = "Seed yellow card"
        };

        HttpResponseMessage resp = await http.PostAsJsonAsync($"api/football-matches/{matchId}/events/card", cardReq);
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"  Warning: card event failed ({(int)resp.StatusCode}): {Truncate(body, 200)}");
        }
    }

    private static async Task TryRecordSubstitutionAsync(
        HttpClient http,
        Guid matchId,
        Guid teamId,
        List<LineupPlayerRequest> lineup,
        HashSet<Guid> onField,
        Guid? gkId,
        bool requireGoalkeeper,
        int halves,
        int halfSeconds,
        Random rng)
    {
        List<Guid> offCandidates = onField
            .Where(id => !requireGoalkeeper || gkId == null || id != gkId.Value)
            .ToList();
        List<Guid> onCandidates = lineup
            .Where(p => !p.IsOnField && (p.Position != FootballPosition.Goalkeeper || !requireGoalkeeper))
            .Select(p => p.PlayerId)
            .ToList();

        if (offCandidates.Count == 0 || onCandidates.Count == 0)
        {
            return;
        }

        RecordSubstitutionEventRequest subReq = new RecordSubstitutionEventRequest
        {
            MatchId = matchId,
            TeamId = teamId,
            PlayerOffId = offCandidates[rng.Next(offCandidates.Count)],
            PlayerOnId = onCandidates[rng.Next(onCandidates.Count)],
            PeriodNumber = rng.Next(1, halves + 1),
            TimeInSeconds = rng.Next(30, halfSeconds),
            Description = "Seed substitution"
        };

        HttpResponseMessage resp = await http.PostAsJsonAsync($"api/football-matches/{matchId}/events/substitution", subReq);
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"  Warning: substitution event failed ({(int)resp.StatusCode}): {Truncate(body, 200)}");
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value ?? string.Empty;
        }
        return value.Substring(0, maxLength) + "...";
    }
}
