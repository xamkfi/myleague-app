using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Officials.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Teams;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace Seeder;

/// <summary>
/// Shared helpers to confirm roster, attach officials, start, record events, finish, and recalculate stats.
/// </summary>
public static class HockeyMatchSimulation
{
    private const int MinDressedPlayers = 15;

    public static async Task<HockeyMatchDto> SimulateCompletedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyMatchDto match,
        Dictionary<Guid, HockeyTeamDto> rosterCache,
        IReadOnlyList<HockeyOfficialDto>? officials = null)
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
                throw new InvalidOperationException(
                    "Cannot simulate match " + match.Id + ": empty roster for team " + side.TeamId);
            }

            List<Guid> teamPlayerIds = PickMatchRoster(team);
            ConfirmHockeyMatchRosterRequest rosterReq = new ConfirmHockeyMatchRosterRequest
            {
                MatchTeamId = side.Id,
                TeamPlayerIds = teamPlayerIds,
                Source = HockeyPlayerSelectionSource.Manual
            };

            HttpResponseMessage rosterResp =
                await http.PostAsJsonAsync("api/HockeyMatch/" + match.Id + "/roster/confirm", rosterReq);
            await SeederHttp.EnsureSuccessWithBody(rosterResp, "Confirm Hockey Match Roster");

            ApiResponse<HockeyMatchDto>? rosterApi =
                await rosterResp.Content.ReadFromJsonAsync<ApiResponse<HockeyMatchDto>>(jsonOptions);
            if (rosterApi?.Data != null)
            {
                match = rosterApi.Data;
            }
        }

        await AttachOfficialsAsync(http, jsonOptions, match, officials);

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
            ApiResponse<HockeyMatchDto>? startApi =
                await startResp.Content.ReadFromJsonAsync<ApiResponse<HockeyMatchDto>>(jsonOptions);
            if (startApi?.Data != null)
            {
                match = startApi.Data;
            }
        }

        match = await GetMatchAsync(http, jsonOptions, match.Id) ?? match;
        await SetActiveGoaliesAsync(http, jsonOptions, match);
        match = await GetMatchAsync(http, jsonOptions, match.Id) ?? match;
        await TryApplyMatchLineAsync(http, jsonOptions, match, rosterCache);
        match = await GetMatchAsync(http, jsonOptions, match.Id) ?? match;
        await RecordRichEventsAsync(http, jsonOptions, match);

        HttpResponseMessage finishResp = await http.PostAsJsonAsync(
            "api/HockeyMatch/" + match.Id + "/finish",
            new MarkHockeyMatchFinishedRequest
            {
                ActualEndTime = DateTime.UtcNow.AddHours(-1),
                ResultType = HockeyMatchResultType.HomeWin
            });
        await SeederHttp.EnsureSuccessWithBody(finishResp, "Finish Hockey Match");

        ApiResponse<HockeyMatchDto>? finishApi =
            await finishResp.Content.ReadFromJsonAsync<ApiResponse<HockeyMatchDto>>(jsonOptions);
        if (finishApi?.Data != null)
        {
            match = finishApi.Data;
        }

        await RecalculateMatchAsync(http, match.Id);
        return await GetMatchAsync(http, jsonOptions, match.Id) ?? match;
    }

    public static async Task RecalculateMatchAsync(HttpClient http, Guid matchId)
    {
        HttpResponseMessage resp =
            await http.PostAsync("api/HockeyStatistics/matches/" + matchId + "/recalculate", content: null);
        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine(
                "Warning: match stats recalculate failed for " + matchId + " (" + (int)resp.StatusCode + "): " +
                Truncate(body));
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
            Console.WriteLine(
                "Warning: competition stats recalculate failed for " + competitionId + " (" + (int)resp.StatusCode +
                "): " + Truncate(body));
            return;
        }

        Console.WriteLine("Recalculated hockey competition statistics for " + competitionId);
    }

    /// <summary>
    /// Dresses goalie(s) plus enough skaters to satisfy default MinDressedPlayers (15).
    /// </summary>
    private static List<Guid> PickMatchRoster(HockeyTeamDto team)
    {
        List<HockeyTeamPlayerDto> active = team.Roster.Where(p => p.IsActive).ToList();
        List<HockeyTeamPlayerDto> goalies = active
            .Where(p => string.Equals(p.Position, "Goalie", StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .ToList();
        List<HockeyTeamPlayerDto> skaters = active
            .Where(p => !string.Equals(p.Position, "Goalie", StringComparison.OrdinalIgnoreCase))
            .Take(Math.Max(MinDressedPlayers - 1, 0))
            .ToList();

        List<Guid> ids = new List<Guid>();
        ids.AddRange(goalies.Select(g => g.Id));
        ids.AddRange(skaters.Select(s => s.Id));

        if (ids.Count < MinDressedPlayers)
        {
            ids.AddRange(active.Select(p => p.Id).Where(id => !ids.Contains(id)));
        }

        if (ids.Count == 0)
        {
            throw new InvalidOperationException("Team " + team.Id + " has no active roster players to dress.");
        }

        if (ids.Count < MinDressedPlayers)
        {
            throw new InvalidOperationException(
                "Team " + team.Name + " has only " + ids.Count +
                " active players; need at least " + MinDressedPlayers + " for roster confirm.");
        }

        return ids.Distinct().Take(20).ToList();
    }

    private static async Task AttachOfficialsAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyMatchDto match,
        IReadOnlyList<HockeyOfficialDto>? officials)
    {
        if (officials == null || officials.Count == 0)
        {
            return;
        }

        match = await GetMatchAsync(http, jsonOptions, match.Id) ?? match;
        if (match.Officials.Count > 0)
        {
            return;
        }

        HockeyOfficialDto official = officials[0];
        AddHockeyMatchOfficialRequest request = new AddHockeyMatchOfficialRequest
        {
            OfficialId = official.Id,
            Role = HockeyOfficialRole.Referee,
            IsMainOfficial = true
        };
        HttpResponseMessage resp = await http.PostAsJsonAsync("api/HockeyMatch/" + match.Id + "/officials", request);
        if (!resp.IsSuccessStatusCode)
        {
            Console.WriteLine("Warning: attach official failed: " + await resp.Content.ReadAsStringAsync());
        }
        else
        {
            Console.WriteLine("Attached hockey official " + official.Id + " to match " + match.Id);
        }
    }

    private static async Task SetActiveGoaliesAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyMatchDto match)
    {
        foreach (HockeyMatchTeamDto side in match.MatchTeams)
        {
            HockeyMatchActivePlayerDto? goalie = side.ActivePlayers.FirstOrDefault(p => p.IsGoalie && p.IsActive);
            if (goalie == null || side.ActiveGoalieMatchPlayerId == goalie.Id)
            {
                continue;
            }

            HockeyMatchTeamPlayerRequest request = new HockeyMatchTeamPlayerRequest
            {
                MatchTeamId = side.Id,
                MatchActivePlayerId = goalie.Id
            };
            HttpResponseMessage resp =
                await http.PutAsJsonAsync("api/HockeyMatch/" + match.Id + "/active-goalie", request);
            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine("Warning: set active goalie failed: " + await resp.Content.ReadAsStringAsync());
            }
        }
    }

    private static async Task TryApplyMatchLineAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyMatchDto match,
        Dictionary<Guid, HockeyTeamDto> rosterCache)
    {
        try
        {
            HockeyMatchTeamDto? home = match.MatchTeams.FirstOrDefault(t =>
                string.Equals(t.TeamSlot, "Home", StringComparison.OrdinalIgnoreCase));
            if (home == null)
            {
                return;
            }

            HockeyTeamDto? team = await GetTeamAsync(http, jsonOptions, home.TeamId, rosterCache);
            HockeyLineDto? line1 = team?.Lines.FirstOrDefault(l =>
                string.Equals(l.Name, "Line 1", StringComparison.OrdinalIgnoreCase) && l.IsActive);
            if (line1 == null || line1.Players.Count == 0)
            {
                return;
            }

            AddHockeyMatchLineRequest createLine = new AddHockeyMatchLineRequest
            {
                MatchTeamId = home.Id,
                Name = "Match Line 1",
                LineNumber = 1,
                LineType = HockeyLineType.ForwardLine
            };
            HttpResponseMessage createResp =
                await http.PostAsJsonAsync("api/HockeyMatch/" + match.Id + "/lines", createLine);
            if (!createResp.IsSuccessStatusCode)
            {
                Console.WriteLine("Warning: create match line skipped: " + await createResp.Content.ReadAsStringAsync());
                return;
            }

            ApiResponse<HockeyMatchDto>? createApi =
                await createResp.Content.ReadFromJsonAsync<ApiResponse<HockeyMatchDto>>(jsonOptions);
            HockeyMatchDto? updated = createApi?.Data;
            HockeyMatchLineDto? matchLine = updated?.MatchTeams
                .FirstOrDefault(t => t.Id == home.Id)?.Lines
                .FirstOrDefault(l => string.Equals(l.Name, "Match Line 1", StringComparison.OrdinalIgnoreCase));
            if (matchLine == null || updated == null)
            {
                return;
            }

            Dictionary<Guid, Guid> teamPlayerToActive = updated.MatchTeams
                .First(t => t.Id == home.Id)
                .ActivePlayers
                .ToDictionary(p => p.TeamPlayerId, p => p.Id);

            foreach (HockeyLinePlayerDto lp in line1.Players)
            {
                if (!teamPlayerToActive.TryGetValue(lp.TeamPlayerId, out Guid activeId))
                {
                    continue;
                }

                AddHockeyMatchLinePlayerRequest addPlayer = new AddHockeyMatchLinePlayerRequest
                {
                    MatchTeamId = home.Id,
                    MatchActivePlayerId = activeId,
                    Slot = Enum.TryParse(lp.Slot, ignoreCase: true, out HockeyLineSlot slot)
                        ? slot
                        : HockeyLineSlot.Any,
                    Order = lp.Order
                };
                HttpResponseMessage addResp = await http.PostAsJsonAsync(
                    "api/HockeyMatch/" + match.Id + "/lines/" + matchLine.Id + "/players",
                    addPlayer);
                if (!addResp.IsSuccessStatusCode)
                {
                    Console.WriteLine("Warning: match line player skipped: " + await addResp.Content.ReadAsStringAsync());
                }
            }

            HttpResponseMessage enableResp = await http.PostAsJsonAsync(
                "api/HockeyMatch/" + match.Id + "/on-ice/enable",
                new HockeyMatchTeamIdRequest { MatchTeamId = home.Id });
            if (!enableResp.IsSuccessStatusCode)
            {
                Console.WriteLine("Warning: on-ice enable skipped: " + await enableResp.Content.ReadAsStringAsync());
                return;
            }

            HttpResponseMessage applyResp = await http.PostAsJsonAsync(
                "api/HockeyMatch/" + match.Id + "/on-ice/apply-line/" + matchLine.Id,
                new HockeyMatchIceActionRequest
                {
                    MatchTeamId = home.Id,
                    PeriodNumber = 1,
                    TimeInSeconds = 60
                });
            if (!applyResp.IsSuccessStatusCode)
            {
                Console.WriteLine("Warning: apply match line skipped: " + await applyResp.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Warning: match line / on-ice step skipped: " + ex.Message);
        }
    }

    private static async Task RecordRichEventsAsync(
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

        HockeyMatchActivePlayerDto? homeScorer = home.ActivePlayers.FirstOrDefault(p => !p.IsGoalie && p.IsActive);
        HockeyMatchActivePlayerDto? awayScorer = away.ActivePlayers.FirstOrDefault(p => !p.IsGoalie && p.IsActive);
        HockeyMatchActivePlayerDto? homePenalized = home.ActivePlayers
            .Where(p => !p.IsGoalie && p.IsActive)
            .Skip(1)
            .FirstOrDefault() ?? homeScorer;

        await PostPeriodScoreAsync(http, match.Id, 1);
        await PostPeriodScoreAsync(http, match.Id, 2);

        if (homeScorer != null)
        {
            RecordHockeyFaceoffRequest faceoff = new RecordHockeyFaceoffRequest
            {
                WinningMatchTeamId = home.Id,
                LosingMatchTeamId = away.Id,
                PeriodNumber = 1,
                TimeInSeconds = 0,
                Zone = HockeyFaceoffZone.NeutralZone,
                Spot = HockeyFaceoffSpot.CenterIce,
                WinningActivePlayerId = homeScorer.Id,
                LosingActivePlayerId = awayScorer?.Id
            };
            await PostEventWarnAsync(http, "api/HockeyMatch/" + match.Id + "/events/faceoffs", faceoff, "faceoff");

            RecordHockeyShotRequest shotReq = new RecordHockeyShotRequest
            {
                ShootingMatchTeamId = home.Id,
                PeriodNumber = 1,
                TimeInSeconds = 120,
                ShotResult = HockeyShotResult.Saved,
                CountsAsShotOnGoal = true,
                ShooterActivePlayerId = homeScorer.Id
            };
            await PostEventWarnAsync(http, "api/HockeyMatch/" + match.Id + "/events/shots", shotReq, "shot");

            RecordHockeyGoalRequest goalReq = new RecordHockeyGoalRequest
            {
                ScoringMatchTeamId = home.Id,
                ScorerActivePlayerId = homeScorer.Id,
                PeriodNumber = 1,
                TimeInSeconds = 320,
                GoalStrength = HockeyGoalStrength.EvenStrength
            };
            await PostEventWarnAsync(http, "api/HockeyMatch/" + match.Id + "/events/goals", goalReq, "goal");
        }

        if (homePenalized != null)
        {
            RecordHockeyPenaltyRequest penalty = new RecordHockeyPenaltyRequest
            {
                PenaltyMatchTeamId = home.Id,
                PeriodNumber = 1,
                TimeInSeconds = 450,
                Severity = HockeyPenaltySeverity.Minor,
                Offence = HockeyPenaltyOffence.Tripping,
                PenaltyMinutes = 2,
                PenalizedActivePlayerId = homePenalized.Id,
                IsBenchPenalty = false
            };
            await PostEventWarnAsync(http, "api/HockeyMatch/" + match.Id + "/events/penalties", penalty, "penalty");
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
            await PostEventWarnAsync(http, "api/HockeyMatch/" + match.Id + "/events/goals", goalReq, "away goal");
        }
    }

    private static async Task PostPeriodScoreAsync(HttpClient http, Guid matchId, int periodNumber)
    {
        AddHockeyPeriodScoreRequest request = new AddHockeyPeriodScoreRequest
        {
            PeriodNumber = periodNumber,
            PeriodType = HockeyPeriodType.RegularPeriod
        };
        HttpResponseMessage resp =
            await http.PostAsJsonAsync("api/HockeyMatch/" + matchId + "/period-scores", request);
        if (!resp.IsSuccessStatusCode)
        {
            Console.WriteLine("Warning: period score P" + periodNumber + " failed: " + await resp.Content.ReadAsStringAsync());
        }
    }

    private static async Task PostEventWarnAsync<T>(HttpClient http, string url, T body, string label)
    {
        HttpResponseMessage resp = await http.PostAsJsonAsync(url, body);
        if (!resp.IsSuccessStatusCode)
        {
            Console.WriteLine("Warning: " + label + " event failed: " + await resp.Content.ReadAsStringAsync());
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
