using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Teams.DTOs;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Football;

namespace Seeder;

public static class FootballMatchesSeeder
{
    public static async Task<List<FootballMatchDto>> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<FootballMatchSeed> matches,
        List<FootballSeasonDto> seasons,
        List<FootballTeamDto> teams,
        List<FootballRefereeDto> referees,
        Dictionary<string, Guid> emailToRefereeId)
    {
        List<FootballMatchDto> created = new List<FootballMatchDto>();
        Dictionary<Guid, FootballTeamDto> rosterCache = new Dictionary<Guid, FootballTeamDto>();
        Random rng = new Random(42);

        foreach (FootballMatchSeed match in matches)
        {
            Guid seasonId = ResolveSeasonId(match.SeasonName, seasons);
            Guid homeTeamId = ResolveTeamId(match.HomeTeamName, teams);
            Guid awayTeamId = ResolveTeamId(match.AwayTeamName, teams);
            Guid? refereeId = ResolveRefereeId(match.RefereeEmail, emailToRefereeId);

            FootballMatchDto? existing = await FindExistingMatchAsync(http, jsonOptions, seasonId, homeTeamId, awayTeamId);
            FootballMatchDto matchToSimulate;
            if (existing != null)
            {
                created.Add(existing);
                Console.WriteLine($"Football match exists, skipping create: {match.HomeTeamName} vs {match.AwayTeamName} ({existing.Id})");
                matchToSimulate = existing;
            }
            else
            {
                CreateFootballMatchRequest request = new CreateFootballMatchRequest
                {
                    CompetitionId = seasonId,
                    HomeTeamId = homeTeamId,
                    AwayTeamId = awayTeamId,
                    RefereeId = refereeId,
                    ScheduledDateTime = match.ScheduledDateTime,
                    Venue = match.Venue
                };

                HttpResponseMessage response = await http.PostAsJsonAsync("api/football-matches", request);
                await SeederHttp.EnsureSuccessWithBody(response, "Create Football Match");

                ApiResponse<FootballMatchDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FootballMatchDto>>(jsonOptions);
                if (api == null || !api.Success || api.Data == null)
                {
                    throw new InvalidOperationException("Create football match failed: " + (api != null ? api.Message : "null response"));
                }

                matchToSimulate = api.Data;
                created.Add(matchToSimulate);
                Console.WriteLine($"Created football match: {match.HomeTeamName} vs {match.AwayTeamName} ({matchToSimulate.Id})");
            }

            if (FootballMatchSimulator.IsPastScheduled(matchToSimulate.ScheduledDateTime)
                || (DateTime.TryParse(match.ScheduledDateTime, out DateTime seedScheduled) && FootballMatchSimulator.IsPastScheduled(seedScheduled)))
            {
                try
                {
                    await FootballMatchSimulator.SimulateCompletedMatchAsync(
                        http,
                        jsonOptions,
                        matchToSimulate,
                        match.HomeTeamName,
                        match.AwayTeamName,
                        rosterCache,
                        rng);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: failed to simulate completion for {match.HomeTeamName} vs {match.AwayTeamName}: {ex.Message}");
                }
            }
        }

        return created;
    }

    private static async Task<FootballMatchDto?> FindExistingMatchAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        Guid seasonId,
        Guid homeTeamId,
        Guid awayTeamId)
    {
        const int pageSize = 50;
        int page = 1;
        while (true)
        {
            HttpResponseMessage listResp = await http.GetAsync($"api/football-matches?CompetitionId={seasonId}&Page={page}&PageSize={pageSize}");
            if (!listResp.IsSuccessStatusCode)
            {
                return null;
            }

            PaginatedApiResponse<FootballMatchDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FootballMatchDto>>(jsonOptions);
            if (listApi == null || !listApi.Success || listApi.Data == null)
            {
                return null;
            }

            FootballMatchDto? existing = listApi.Data.FirstOrDefault(m => m.HomeTeamId == homeTeamId && m.AwayTeamId == awayTeamId);
            if (existing != null)
            {
                return existing;
            }

            if (listApi.Data.Count() < pageSize)
            {
                return null;
            }

            page++;
            if (page > 50)
            {
                return null;
            }
        }
    }

    private static Guid ResolveSeasonId(string seasonName, List<FootballSeasonDto> seasons)
    {
        FootballSeasonDto? season = seasons.FirstOrDefault(s => string.Equals(s.Name, seasonName, StringComparison.OrdinalIgnoreCase));
        if (season == null)
        {
            throw new InvalidOperationException($"Football season not found by name: {seasonName}");
        }
        return season.Id;
    }

    private static Guid ResolveTeamId(string teamName, List<FootballTeamDto> teams)
    {
        FootballTeamDto? team = teams.FirstOrDefault(t => string.Equals(t.Name, teamName, StringComparison.OrdinalIgnoreCase));
        if (team == null)
        {
            throw new InvalidOperationException($"Football team not found by name: {teamName}");
        }
        return team.Id;
    }

    private static Guid? ResolveRefereeId(string? refereeEmail, Dictionary<string, Guid> emailToRefereeId)
    {
        if (string.IsNullOrWhiteSpace(refereeEmail))
        {
            return null;
        }

        if (emailToRefereeId.TryGetValue(refereeEmail, out Guid refereeId))
        {
            return refereeId;
        }

        Console.WriteLine($"Warning: Football referee not found for email: {refereeEmail}");
        return null;
    }
}
