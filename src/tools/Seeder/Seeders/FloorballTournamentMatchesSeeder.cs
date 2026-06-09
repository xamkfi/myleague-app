using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Tournaments.DTOs;
using Domain.Enums.Floorball;
using WebAPI.Models.Floorball;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;

namespace Seeder;

/// <summary>
/// Generates round-robin group-stage matches for each tournament group, distributes them across the tournament window,
/// completes the matches that are scheduled in the past (with a few simulated goals to populate statistics), and leaves
/// the remaining matches in the Scheduled state so the tournament has a believable mix of finished and upcoming matches.
/// </summary>
public static class FloorballTournamentMatchesSeeder
{
    private const int GoalsPerMatchMin = 3;
    private const int GoalsPerMatchMax = 8;

    public static async Task<int> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<FloorballTournamentDto> tournaments,
        List<FloorballRefereeDto> allReferees,
        IReadOnlyList<FloorballTournamentSeed>? tournamentSeeds = null)
    {
        int createdCount = 0;
        if (tournaments.Count == 0)
        {
            return 0;
        }

        // Build a fast lookup of "completed group stage" tournaments by name so we can adjust scheduling.
        HashSet<string> allCompletedNames = tournamentSeeds == null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : tournamentSeeds
                .Where(s => s.AllGroupMatchesCompleted)
                .Select(s => s.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // RefereeId is optional on FloorballMatch — if no referees are available, we still create the matches
        // (just without an assigned official). This keeps tournament-match seeding resilient even if the
        // referee endpoints momentarily return empty lists on re-runs.
        if (allReferees.Count == 0)
        {
            Console.WriteLine("Tournament match seeding: no referees available, creating matches without an assigned referee.");
        }

        // Deterministic randomness so re-runs behave the same.
        Random rng = new Random(42);

        foreach (FloorballTournamentDto tournament in tournaments)
        {
            bool allCompleted = allCompletedNames.Contains(tournament.Name);

            // Match-date window strategy:
            //   - Default: a believable mix of completed + upcoming matches around "now".
            //   - allCompleted == true: every match is in the past so all of them simulate to completion,
            //     leaving the tournament in GroupStage status and ready for the playoff transition test.
            DateTime windowStartUtc;
            DateTime windowEndUtc;
            if (allCompleted)
            {
                windowStartUtc = DateTime.UtcNow.AddDays(-12);
                // Leave a small buffer so SnapToReasonableHour cannot accidentally push past "now".
                windowEndUtc = DateTime.UtcNow.AddHours(-2);
            }
            else
            {
                windowStartUtc = DateTime.UtcNow.AddDays(-6);
                windowEndUtc = DateTime.UtcNow.AddDays(12);
            }

            List<TournamentMatchPlan> plans = BuildGroupRoundRobinPlans(tournament, windowStartUtc, windowEndUtc);
            if (plans.Count == 0)
            {
                continue;
            }

            // Cache existing tournament matches once per tournament for idempotency.
            HashSet<(Guid Home, Guid Away)> existingPairs = await LoadExistingMatchPairsAsync(http, jsonOptions, tournament.Id);

            // Cache full team rosters (with Position) so we can pick goalies / field players for completion simulation.
            Dictionary<Guid, FloorballTeamDto> rosterCache = new Dictionary<Guid, FloorballTeamDto>();

            for (int i = 0; i < plans.Count; i++)
            {
                TournamentMatchPlan plan = plans[i];

                if (existingPairs.Contains((plan.HomeTeamId, plan.AwayTeamId)) ||
                    existingPairs.Contains((plan.AwayTeamId, plan.HomeTeamId)))
                {
                    Console.WriteLine($"Tournament match exists, skipping: {plan.HomeTeamName} vs {plan.AwayTeamName}");
                    continue;
                }

                Guid? refereeId = allReferees.Count > 0
                    ? allReferees[i % allReferees.Count].Id
                    : (Guid?)null;

                CreateFloorballMatchRequest createReq = new CreateFloorballMatchRequest
                {
                    CompetitionId = tournament.Id,
                    HomeTeamId = plan.HomeTeamId,
                    AwayTeamId = plan.AwayTeamId,
                    RefereeId = refereeId,
                    ScheduledDateTime = plan.ScheduledUtc.ToString("o"),
                    Venue = tournament.Venue,
                    TournamentGroupId = plan.GroupId,
                    TournamentStage = nameof(FloorballTournamentStage.GroupStage)
                };

                HttpResponseMessage createResp = await http.PostAsJsonAsync("api/floorballmatch", createReq);
                await SeederHttp.EnsureSuccessWithBody(createResp, "Create Tournament Match");

                ApiResponse<FloorballMatchDto>? createdApi = await createResp.Content.ReadFromJsonAsync<ApiResponse<FloorballMatchDto>>(jsonOptions);
                if (createdApi == null || !createdApi.Success || createdApi.Data == null)
                {
                    throw new InvalidOperationException("Create tournament match failed: " + (createdApi != null ? createdApi.Message : "null response"));
                }

                FloorballMatchDto createdMatch = createdApi.Data;
                createdCount++;
                Console.WriteLine($"Created tournament match: {plan.HomeTeamName} vs {plan.AwayTeamName} on {plan.ScheduledUtc:u} (group {plan.GroupName})");

                // If the match is scheduled in the past, simulate it through to completion so it produces stats.
                if (plan.ScheduledUtc <= DateTime.UtcNow.AddMinutes(-5))
                {
                    try
                    {
                        await SimulateCompletedMatchAsync(http, jsonOptions, createdMatch, plan, rosterCache, rng);
                    }
                    catch (Exception ex)
                    {
                        // Don't abort the entire seed run if a single simulation fails — keep the match in Scheduled state.
                        Console.WriteLine($"Warning: failed to simulate completion for {plan.HomeTeamName} vs {plan.AwayTeamName}: {ex.Message}");
                    }
                }
            }
        }

        return createdCount;
    }

    private static async Task<HashSet<(Guid, Guid)>> LoadExistingMatchPairsAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        Guid competitionId)
    {
        // Page through results to avoid running into MaxPageSize limits.
        // Development overrides Global.MaxPageSize = 50 in appsettings.Development.json, so we stay at/below that.
        HashSet<(Guid, Guid)> pairs = new HashSet<(Guid, Guid)>();
        const int pageSize = 50;
        int page = 1;
        while (true)
        {
            HttpResponseMessage listResp = await http.GetAsync(
                $"api/floorballmatch?CompetitionId={competitionId}&Page={page}&PageSize={pageSize}");
            if (!listResp.IsSuccessStatusCode)
            {
                string body = await listResp.Content.ReadAsStringAsync();
                Console.Error.WriteLine(
                    $"WARNING: GET /api/floorballmatch?CompetitionId={competitionId}&Page={page}&PageSize={pageSize} returned {(int)listResp.StatusCode}. " +
                    $"Existing-match check may be incomplete. Body: {(body.Length > 300 ? body.Substring(0, 300) + "..." : body)}");
                break;
            }

            PaginatedApiResponse<FloorballMatchDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FloorballMatchDto>>(jsonOptions);
            if (listApi?.Data == null)
            {
                break;
            }

            int countOnPage = 0;
            foreach (FloorballMatchDto m in listApi.Data)
            {
                // Match team IDs are nullable to support placeholder fixtures. Skip rows where
                // either side is unassigned because they can never collide with a (home, away) pair
                // we'd generate from concrete teams.
                if (m.HomeTeamId.HasValue && m.AwayTeamId.HasValue)
                {
                    pairs.Add((m.HomeTeamId.Value, m.AwayTeamId.Value));
                }
                countOnPage++;
            }

            if (countOnPage < pageSize)
            {
                break;
            }
            page++;
            if (page > 50)
            {
                break;
            }
        }
        return pairs;
    }

    private static List<TournamentMatchPlan> BuildGroupRoundRobinPlans(
        FloorballTournamentDto tournament,
        DateTime windowStartUtc,
        DateTime windowEndUtc)
    {
        List<TournamentMatchPlan> plans = new List<TournamentMatchPlan>();

        // First, gather all (group, homeTeam, awayTeam) pairings.
        List<(FloorballTournamentGroupDto Group, FloorballTournamentGroupTeamDto Home, FloorballTournamentGroupTeamDto Away)> pairings =
            new List<(FloorballTournamentGroupDto, FloorballTournamentGroupTeamDto, FloorballTournamentGroupTeamDto)>();
        foreach (FloorballTournamentGroupDto group in tournament.Groups.OrderBy(g => g.Order))
        {
            List<FloorballTournamentGroupTeamDto> teams = group.Teams.OrderBy(t => t.TeamName).ToList();
            for (int i = 0; i < teams.Count; i++)
            {
                for (int j = i + 1; j < teams.Count; j++)
                {
                    pairings.Add((group, teams[i], teams[j]));
                }
            }
        }

        if (pairings.Count == 0)
        {
            return plans;
        }

        // Spread the pairings evenly across the tournament window. We anchor the first match a few hours after windowStart
        // and keep matches inside the window. If the window is short, we simply step by 1 hour.
        TimeSpan window = windowEndUtc - windowStartUtc;
        TimeSpan step = window > TimeSpan.FromHours(pairings.Count + 1)
            ? TimeSpan.FromTicks(window.Ticks / (pairings.Count + 1))
            : TimeSpan.FromHours(1);

        DateTime cursor = windowStartUtc + step;
        for (int idx = 0; idx < pairings.Count; idx++)
        {
            (FloorballTournamentGroupDto group, FloorballTournamentGroupTeamDto home, FloorballTournamentGroupTeamDto away) = pairings[idx];

            // Snap to "interesting" times (14:00 / 16:00 / 18:00 UTC) so they look realistic, but never push past the window end.
            DateTime scheduled = SnapToReasonableHour(cursor);
            if (scheduled >= windowEndUtc)
            {
                scheduled = windowEndUtc.AddHours(-1);
            }

            plans.Add(new TournamentMatchPlan
            {
                GroupId = group.Id,
                GroupName = group.Name,
                HomeTeamId = home.TeamId,
                HomeTeamName = home.TeamName,
                AwayTeamId = away.TeamId,
                AwayTeamName = away.TeamName,
                ScheduledUtc = scheduled
            });

            cursor += step;
        }

        return plans;
    }

    private static DateTime SnapToReasonableHour(DateTime utc)
    {
        int[] preferred = new[] { 14, 16, 18 };
        int chosenHour = preferred[(utc.DayOfYear + utc.Hour) % preferred.Length];
        return new DateTime(utc.Year, utc.Month, utc.Day, chosenHour, 0, 0, DateTimeKind.Utc);
    }

    private static async Task<FloorballTeamDto?> GetTeamWithRosterAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        Guid teamId,
        Dictionary<Guid, FloorballTeamDto> cache)
    {
        if (cache.TryGetValue(teamId, out FloorballTeamDto? cached))
        {
            return cached;
        }

        HttpResponseMessage resp = await http.GetAsync($"api/floorballteam/{teamId}");
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        ApiResponse<FloorballTeamDto>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<FloorballTeamDto>>(jsonOptions);
        if (api?.Data == null)
        {
            return null;
        }

        cache[teamId] = api.Data;
        return api.Data;
    }

    private static async Task SimulateCompletedMatchAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        FloorballMatchDto match,
        TournamentMatchPlan plan,
        Dictionary<Guid, FloorballTeamDto> rosterCache,
        Random rng)
    {
        // Seeder operates on freshly created matches that always have both teams set.
        if (!match.HomeTeamId.HasValue || !match.AwayTeamId.HasValue)
        {
            Console.WriteLine($"  Skipping simulation for {plan.HomeTeamName} vs {plan.AwayTeamName}: match is missing a team assignment.");
            return;
        }
        Guid homeTeamId = match.HomeTeamId.Value;
        Guid awayTeamId = match.AwayTeamId.Value;

        FloorballTeamDto? homeTeam = await GetTeamWithRosterAsync(http, jsonOptions, homeTeamId, rosterCache);
        FloorballTeamDto? awayTeam = await GetTeamWithRosterAsync(http, jsonOptions, awayTeamId, rosterCache);
        if (homeTeam == null || awayTeam == null)
        {
            Console.WriteLine($"  Skipping simulation for {plan.HomeTeamName} vs {plan.AwayTeamName}: missing team rosters.");
            return;
        }

        Guid? homeGoalieId = PickGoalie(homeTeam);
        Guid? awayGoalieId = PickGoalie(awayTeam);
        if (homeGoalieId == null || awayGoalieId == null)
        {
            Console.WriteLine($"  Skipping simulation for {plan.HomeTeamName} vs {plan.AwayTeamName}: missing goalie on roster.");
            return;
        }

        List<Guid> homeFieldPlayers = PickFieldPlayers(homeTeam);
        List<Guid> awayFieldPlayers = PickFieldPlayers(awayTeam);
        if (homeFieldPlayers.Count == 0 || awayFieldPlayers.Count == 0)
        {
            Console.WriteLine($"  Skipping simulation for {plan.HomeTeamName} vs {plan.AwayTeamName}: no field players on roster.");
            return;
        }

        // Assign goalies (idempotent — endpoint accepts both Scheduled and InProgress states).
        await SetActiveGoalieAsync(http, match.Id, homeTeamId, homeGoalieId.Value);
        await SetActiveGoalieAsync(http, match.Id, awayTeamId, awayGoalieId.Value);

        // Start the match.
        HttpResponseMessage startResp = await http.PutAsync($"api/floorballmatch/start-match/{match.Id}", content: null);
        await SeederHttp.EnsureSuccessWithBody(startResp, "Start Tournament Match");

        // Record a believable number of goals split between teams.
        int totalGoals = rng.Next(GoalsPerMatchMin, GoalsPerMatchMax + 1);
        int homeGoals = rng.Next(0, totalGoals + 1);
        int awayGoals = totalGoals - homeGoals;

        await RecordGoalsAsync(http, match.Id, homeTeamId, homeFieldPlayers, homeGoals, rng);
        await RecordGoalsAsync(http, match.Id, awayTeamId, awayFieldPlayers, awayGoals, rng);

        // Complete the match.
        HttpResponseMessage completeResp = await http.PutAsync($"api/floorballmatch/complete-match/{match.Id}", content: null);
        await SeederHttp.EnsureSuccessWithBody(completeResp, "Complete Tournament Match");

        Console.WriteLine($"  Completed: {plan.HomeTeamName} {homeGoals} - {awayGoals} {plan.AwayTeamName}");
    }

    private static async Task SetActiveGoalieAsync(HttpClient http, Guid matchId, Guid teamId, Guid goalieId)
    {
        HttpResponseMessage resp = await http.PutAsync($"api/floorballmatch/{matchId}/team/{teamId}/goalie/{goalieId}", content: null);
        await SeederHttp.EnsureSuccessWithBody(resp, "Set Active Goalie");
    }

    private static async Task RecordGoalsAsync(
        HttpClient http,
        Guid matchId,
        Guid teamId,
        List<Guid> fieldPlayers,
        int goalCount,
        Random rng)
    {
        for (int g = 0; g < goalCount; g++)
        {
            Guid scorerId = fieldPlayers[rng.Next(fieldPlayers.Count)];
            Guid? assistId = fieldPlayers.Count > 1 && rng.Next(0, 100) < 70
                ? fieldPlayers.Where(p => p != scorerId).Skip(rng.Next(fieldPlayers.Count - 1)).FirstOrDefault()
                : null;

            int periodNumber = rng.Next(1, 4); // 1..3
            int timeInSeconds = rng.Next(30, 19 * 60); // within a 20-min period

            RecordGoalRequest goalReq = new RecordGoalRequest
            {
                MatchId = matchId,
                ScoringTeamId = teamId,
                ScoringPlayerId = scorerId,
                AssistingPlayerId = assistId == Guid.Empty ? null : assistId,
                SecondaryAssistingPlayerIs = null,
                PeriodNumber = periodNumber,
                TimeInSeconds = timeInSeconds,
                Description = "Seed goal",
                GoalType = null
            };

            HttpResponseMessage resp = await http.PostAsJsonAsync("api/floorballmatch/record-goal", goalReq);
            await SeederHttp.EnsureSuccessWithBody(resp, "Record Goal");
        }
    }

    private static Guid? PickGoalie(FloorballTeamDto team)
    {
        FloorballTeamPlayerDto? goalie = team.Roster
            .Where(p => p.Position == FloorballPosition.Goalkeeper && p.IsActive)
            .OrderBy(p => p.JerseyNumber ?? int.MaxValue)
            .FirstOrDefault()
            ?? team.Roster
                .Where(p => p.Position == FloorballPosition.Goalkeeper)
                .OrderBy(p => p.JerseyNumber ?? int.MaxValue)
                .FirstOrDefault();

        return goalie?.PlayerId;
    }

    private static List<Guid> PickFieldPlayers(FloorballTeamDto team)
    {
        return team.Roster
            .Where(p => p.Position != FloorballPosition.Goalkeeper)
            .Select(p => p.PlayerId)
            .Distinct()
            .ToList();
    }

    private sealed class TournamentMatchPlan
    {
        public Guid GroupId { get; init; }
        public string GroupName { get; init; } = string.Empty;
        public Guid HomeTeamId { get; init; }
        public string HomeTeamName { get; init; } = string.Empty;
        public Guid AwayTeamId { get; init; }
        public string AwayTeamName { get; init; } = string.Empty;
        public DateTime ScheduledUtc { get; init; }
    }
}
