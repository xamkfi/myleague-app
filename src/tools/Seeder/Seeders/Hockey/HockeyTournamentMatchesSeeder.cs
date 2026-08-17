using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Enums.Hockey.Matches;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace Seeder;

/// <summary>
/// Generates round-robin group-stage matches for hockey tournaments around "now".
/// Past-dated matches are simulated to Finished with stats recalculation.
/// </summary>
public static class HockeyTournamentMatchesSeeder
{
    public static async Task<int> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<HockeyTournamentDto> tournaments,
        List<HockeyTeamDto> teams,
        IReadOnlyList<HockeyTournamentSeed>? tournamentSeeds = null,
        IReadOnlyList<Application.Features.Hockey.Officials.DTOs.HockeyOfficialDto>? officials = null)
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

        Dictionary<Guid, string> teamNames = teams.ToDictionary(t => t.Id, t => t.Name);
        Dictionary<Guid, HockeyTeamDto> rosterCache = new Dictionary<Guid, HockeyTeamDto>();
        HashSet<Guid> competitionsNeedingRecalc = new HashSet<Guid>();

        foreach (HockeyTournamentDto tournament in tournaments)
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

            List<TournamentMatchPlan> plans = BuildGroupRoundRobinPlans(tournament, teamNames, windowStartUtc, windowEndUtc);
            if (plans.Count == 0)
            {
                continue;
            }

            HashSet<(Guid Home, Guid Away)> existingPairs = await LoadExistingMatchPairsAsync(http, jsonOptions, tournament);

            for (int i = 0; i < plans.Count; i++)
            {
                TournamentMatchPlan plan = plans[i];

                if (existingPairs.Contains((plan.HomeTeamId, plan.AwayTeamId)) ||
                    existingPairs.Contains((plan.AwayTeamId, plan.HomeTeamId)))
                {
                    Console.WriteLine("Hockey tournament match exists, skipping: " + plan.HomeTeamName + " vs " + plan.AwayTeamName);
                    continue;
                }

                CreateHockeyMatchRequest createReq = new CreateHockeyMatchRequest
                {
                    ScheduledStartTime = plan.ScheduledUtc,
                    MatchType = HockeyMatchType.TournamentGroup,
                    CompetitionId = tournament.Id,
                    TournamentGroupId = plan.GroupId,
                    Venue = tournament.Venue
                };

                HttpResponseMessage createResp = await http.PostAsJsonAsync("api/HockeyMatch", createReq);
                await SeederHttp.EnsureSuccessWithBody(createResp, "Create Hockey Tournament Match");

                ApiResponse<HockeyMatchDto>? createdApi = await createResp.Content.ReadFromJsonAsync<ApiResponse<HockeyMatchDto>>(jsonOptions);
                if (createdApi?.Data == null)
                {
                    throw new InvalidOperationException("Create hockey tournament match failed.");
                }

                HockeyMatchDto matchDto = createdApi.Data;

                AddHomeAwayTeamsToHockeyMatchRequest teamsReq = new AddHomeAwayTeamsToHockeyMatchRequest
                {
                    HomeTeamId = plan.HomeTeamId,
                    AwayTeamId = plan.AwayTeamId
                };
                HttpResponseMessage teamsResp = await http.PutAsJsonAsync("api/HockeyMatch/" + matchDto.Id + "/teams", teamsReq);
                await SeederHttp.EnsureSuccessWithBody(teamsResp, "Assign Hockey Tournament Match Teams");

                ApiResponse<HockeyMatchDto>? teamsApi = await teamsResp.Content.ReadFromJsonAsync<ApiResponse<HockeyMatchDto>>(jsonOptions);
                if (teamsApi?.Data != null)
                {
                    matchDto = teamsApi.Data;
                }

                createdCount++;
                existingPairs.Add((plan.HomeTeamId, plan.AwayTeamId));
                Console.WriteLine("Created hockey tournament match: " + plan.HomeTeamName + " vs " + plan.AwayTeamName + " on " + plan.ScheduledUtc.ToString("u") + " (group " + plan.GroupName + ")");

                if (plan.ScheduledUtc <= DateTime.UtcNow.AddMinutes(-5))
                {
                    try
                    {
                        await HockeyMatchSimulation.SimulateCompletedAsync(
                            http, jsonOptions, matchDto, rosterCache, officials);
                        competitionsNeedingRecalc.Add(tournament.Id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Warning: failed to simulate tournament match " + plan.HomeTeamName + " vs " + plan.AwayTeamName + ": " + ex.Message);
                    }
                }

            }
        }

        foreach (Guid competitionId in competitionsNeedingRecalc)
        {
            await HockeyMatchSimulation.RecalculateCompetitionAsync(http, competitionId);
        }

        return createdCount;
    }

    private static async Task<HashSet<(Guid, Guid)>> LoadExistingMatchPairsAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeyTournamentDto tournament)
    {
        HashSet<(Guid, Guid)> pairs = new HashSet<(Guid, Guid)>();
        List<HockeyMatchDto> matches = await HockeyMatchesSeeder.LoadCompetitionMatchesAsync(http, jsonOptions, tournament.Id);
        foreach (HockeyMatchDto m in matches)
        {
            if (m.HomeTeamId.HasValue && m.AwayTeamId.HasValue)
            {
                pairs.Add((m.HomeTeamId.Value, m.AwayTeamId.Value));
            }
        }
        return pairs;
    }

    private static List<TournamentMatchPlan> BuildGroupRoundRobinPlans(
        HockeyTournamentDto tournament,
        Dictionary<Guid, string> teamNames,
        DateTime windowStartUtc,
        DateTime windowEndUtc)
    {
        List<TournamentMatchPlan> plans = new List<TournamentMatchPlan>();
        Dictionary<Guid, Guid> competitionTeamToTeamId = tournament.Teams
            .Where(t => t.IsActive)
            .ToDictionary(t => t.Id, t => t.TeamId);

        List<(HockeyTournamentGroupDto Group, Guid HomeTeamId, Guid AwayTeamId, string HomeName, string AwayName)> pairings =
            new List<(HockeyTournamentGroupDto, Guid, Guid, string, string)>();

        foreach (HockeyTournamentGroupDto group in tournament.Groups.OrderBy(g => g.SortOrder))
        {
            List<Guid> teamIds = group.Teams
                .Where(t => t.IsActive)
                .Select(t => competitionTeamToTeamId.TryGetValue(t.CompetitionTeamId, out Guid teamId) ? teamId : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            for (int i = 0; i < teamIds.Count; i++)
            {
                for (int j = i + 1; j < teamIds.Count; j++)
                {
                    string homeName = teamNames.TryGetValue(teamIds[i], out string? hn) ? hn : teamIds[i].ToString("N").Substring(0, 8);
                    string awayName = teamNames.TryGetValue(teamIds[j], out string? an) ? an : teamIds[j].ToString("N").Substring(0, 8);
                    pairings.Add((group, teamIds[i], teamIds[j], homeName, awayName));
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
            (HockeyTournamentGroupDto group, Guid homeId, Guid awayId, string homeName, string awayName) = pairings[idx];
            DateTime scheduled = SnapToReasonableHour(cursor);
            if (scheduled >= windowEndUtc)
            {
                scheduled = windowEndUtc.AddHours(-1);
            }

            plans.Add(new TournamentMatchPlan
            {
                GroupId = group.Id,
                GroupName = group.Name,
                HomeTeamId = homeId,
                HomeTeamName = homeName,
                AwayTeamId = awayId,
                AwayTeamName = awayName,
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
