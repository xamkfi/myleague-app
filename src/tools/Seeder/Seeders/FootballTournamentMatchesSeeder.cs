using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Tournaments.DTOs;
using Domain.Enums.Football;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Football;

namespace Seeder;

public static class FootballTournamentMatchesSeeder
{
    public static async Task<int> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<FootballTournamentDto> tournaments,
        List<FootballRefereeDto> allReferees,
        IReadOnlyList<FootballTournamentSeed>? tournamentSeeds = null)
    {
        int createdCount = 0;
        if (tournaments.Count == 0)
        {
            return 0;
        }

        HashSet<string> allCompletedNames = tournamentSeeds == null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : tournamentSeeds
                .Where(s => s.AllGroupMatchesCompleted)
                .Select(s => s.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (allReferees.Count == 0)
        {
            Console.WriteLine("Football tournament match seeding: no referees available, creating matches without an assigned referee.");
        }

        Random rng = new Random(42);

        foreach (FootballTournamentDto tournament in tournaments)
        {
            bool allCompleted = allCompletedNames.Contains(tournament.Name);

            DateTime windowStartUtc;
            DateTime windowEndUtc;
            if (allCompleted)
            {
                windowStartUtc = DateTime.UtcNow.AddDays(-12);
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

            List<FootballMatchDto> existingMatches = await LoadExistingMatchesAsync(http, jsonOptions, tournament.Id);
            HashSet<(Guid Home, Guid Away)> existingPairs = existingMatches
                .Where(m => m.HomeTeamId.HasValue && m.AwayTeamId.HasValue)
                .Select(m => (m.HomeTeamId!.Value, m.AwayTeamId!.Value))
                .ToHashSet();
            Dictionary<Guid, FootballTeamDto> rosterCache = new Dictionary<Guid, FootballTeamDto>();

            for (int i = 0; i < plans.Count; i++)
            {
                TournamentMatchPlan plan = plans[i];

                FootballMatchDto? existingMatch = existingMatches.FirstOrDefault(m =>
                    m.HomeTeamId == plan.HomeTeamId && m.AwayTeamId == plan.AwayTeamId);
                existingMatch ??= existingMatches.FirstOrDefault(m =>
                    m.HomeTeamId == plan.AwayTeamId && m.AwayTeamId == plan.HomeTeamId);

                if (existingMatch != null ||
                    existingPairs.Contains((plan.HomeTeamId, plan.AwayTeamId)) ||
                    existingPairs.Contains((plan.AwayTeamId, plan.HomeTeamId)))
                {
                    Console.WriteLine($"Football tournament match exists, skipping create: {plan.HomeTeamName} vs {plan.AwayTeamName}");
                    if (existingMatch != null && plan.ScheduledUtc <= DateTime.UtcNow.AddMinutes(-5))
                    {
                        try
                        {
                            await FootballMatchSimulator.SimulateCompletedMatchAsync(
                                http,
                                jsonOptions,
                                existingMatch,
                                plan.HomeTeamName,
                                plan.AwayTeamName,
                                rosterCache,
                                rng);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: failed to simulate completion for {plan.HomeTeamName} vs {plan.AwayTeamName}: {ex.Message}");
                        }
                    }
                    continue;
                }

                Guid? refereeId = allReferees.Count > 0
                    ? allReferees[i % allReferees.Count].Id
                    : null;

                CreateFootballMatchRequest createReq = new CreateFootballMatchRequest
                {
                    CompetitionId = tournament.Id,
                    HomeTeamId = plan.HomeTeamId,
                    AwayTeamId = plan.AwayTeamId,
                    RefereeId = refereeId,
                    ScheduledDateTime = plan.ScheduledUtc.ToString("o"),
                    Venue = tournament.Venue,
                    TournamentGroupId = plan.GroupId,
                    TournamentStage = nameof(FootballTournamentStage.GroupStage)
                };

                HttpResponseMessage createResp = await http.PostAsJsonAsync("api/football-matches", createReq);
                await SeederHttp.EnsureSuccessWithBody(createResp, "Create Football Tournament Match");

                ApiResponse<FootballMatchDto>? createdApi = await createResp.Content.ReadFromJsonAsync<ApiResponse<FootballMatchDto>>(jsonOptions);
                if (createdApi == null || !createdApi.Success || createdApi.Data == null)
                {
                    throw new InvalidOperationException("Create football tournament match failed: " + (createdApi != null ? createdApi.Message : "null response"));
                }

                FootballMatchDto createdMatch = createdApi.Data;
                createdCount++;
                Console.WriteLine($"Created football tournament match: {plan.HomeTeamName} vs {plan.AwayTeamName} on {plan.ScheduledUtc:u} (group {plan.GroupName})");

                if (plan.ScheduledUtc <= DateTime.UtcNow.AddMinutes(-5))
                {
                    try
                    {
                        await FootballMatchSimulator.SimulateCompletedMatchAsync(
                            http,
                            jsonOptions,
                            createdMatch,
                            plan.HomeTeamName,
                            plan.AwayTeamName,
                            rosterCache,
                            rng);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: failed to simulate completion for {plan.HomeTeamName} vs {plan.AwayTeamName}: {ex.Message}");
                    }
                }
            }
        }

        return createdCount;
    }

    private static async Task<List<FootballMatchDto>> LoadExistingMatchesAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        Guid competitionId)
    {
        List<FootballMatchDto> matches = new List<FootballMatchDto>();
        const int pageSize = 50;
        int page = 1;
        while (true)
        {
            HttpResponseMessage listResp = await http.GetAsync(
                $"api/football-matches?CompetitionId={competitionId}&Page={page}&PageSize={pageSize}");
            if (!listResp.IsSuccessStatusCode)
            {
                string body = await listResp.Content.ReadAsStringAsync();
                Console.Error.WriteLine(
                    $"WARNING: GET /api/football-matches?CompetitionId={competitionId}&Page={page}&PageSize={pageSize} returned {(int)listResp.StatusCode}. " +
                    $"Existing-match check may be incomplete. Body: {(body.Length > 300 ? body.Substring(0, 300) + "..." : body)}");
                break;
            }

            PaginatedApiResponse<FootballMatchDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FootballMatchDto>>(jsonOptions);
            if (listApi?.Data == null)
            {
                break;
            }

            List<FootballMatchDto> pageItems = listApi.Data.ToList();
            matches.AddRange(pageItems);
            if (pageItems.Count < pageSize)
            {
                break;
            }
            page++;
            if (page > 50)
            {
                break;
            }
        }
        return matches;
    }

    private static List<TournamentMatchPlan> BuildGroupRoundRobinPlans(
        FootballTournamentDto tournament,
        DateTime windowStartUtc,
        DateTime windowEndUtc)
    {
        List<TournamentMatchPlan> plans = new List<TournamentMatchPlan>();

        List<(FootballTournamentGroupDto Group, FootballTournamentGroupTeamDto Home, FootballTournamentGroupTeamDto Away)> pairings =
            new List<(FootballTournamentGroupDto, FootballTournamentGroupTeamDto, FootballTournamentGroupTeamDto)>();
        foreach (FootballTournamentGroupDto group in tournament.Groups.OrderBy(g => g.Order))
        {
            List<FootballTournamentGroupTeamDto> teams = group.Teams.OrderBy(t => t.TeamName).ToList();
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

        TimeSpan window = windowEndUtc - windowStartUtc;
        TimeSpan step = window > TimeSpan.FromHours(pairings.Count + 1)
            ? TimeSpan.FromTicks(window.Ticks / (pairings.Count + 1))
            : TimeSpan.FromHours(1);

        DateTime cursor = windowStartUtc + step;
        for (int idx = 0; idx < pairings.Count; idx++)
        {
            (FootballTournamentGroupDto group, FootballTournamentGroupTeamDto home, FootballTournamentGroupTeamDto away) = pairings[idx];

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
