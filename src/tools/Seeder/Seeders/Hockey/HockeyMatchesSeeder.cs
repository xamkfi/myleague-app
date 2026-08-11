using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Matches;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace Seeder;

public static class HockeyMatchesSeeder
{
    public static async Task<List<HockeyMatchDto>> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<HockeyMatchSeed> matches,
        List<HockeySeasonDto> seasons,
        List<HockeyTeamDto> teams)
    {
        List<HockeyMatchDto> created = new List<HockeyMatchDto>();
        HashSet<Guid> competitionsNeedingRecalc = new HashSet<Guid>();
        Dictionary<Guid, HockeyTeamDto> rosterCache = new Dictionary<Guid, HockeyTeamDto>();

        foreach (HockeyMatchSeed match in matches)
        {
            HockeySeasonDto season = ResolveSeason(match.SeasonName, seasons);
            Guid homeTeamId = ResolveTeamId(match.HomeTeamName, teams);
            Guid awayTeamId = ResolveTeamId(match.AwayTeamName, teams);

            HockeyCompetitionDivisionDto? division = season.Divisions.FirstOrDefault(d => d.IsActive);
            Guid? competitionDivisionId = division?.Id;

            List<HockeyMatchDto> existingMatches = await LoadCompetitionMatchesAsync(http, jsonOptions, season.Id);
            HockeyMatchDto? existing = existingMatches.FirstOrDefault(m =>
                m.HomeTeamId == homeTeamId && m.AwayTeamId == awayTeamId);
            if (existing != null)
            {
                created.Add(existing);
                Console.WriteLine("Hockey match exists, skipping: " + match.HomeTeamName + " vs " + match.AwayTeamName + " (" + existing.Id + ")");
                continue;
            }

            if (!DateTime.TryParse(match.ScheduledDateTime, out DateTime scheduled))
            {
                throw new InvalidOperationException("Invalid ScheduledDateTime for hockey match " + match.HomeTeamName + " vs " + match.AwayTeamName);
            }

            scheduled = DateTime.SpecifyKind(scheduled, DateTimeKind.Utc);

            CreateHockeyMatchRequest createReq = new CreateHockeyMatchRequest
            {
                ScheduledStartTime = scheduled,
                MatchType = HockeyMatchType.League,
                CompetitionId = season.Id,
                CompetitionDivisionId = competitionDivisionId,
                Venue = match.Venue
            };

            HttpResponseMessage createResp = await http.PostAsJsonAsync("api/HockeyMatch", createReq);
            await SeederHttp.EnsureSuccessWithBody(createResp, "Create Hockey Match");

            ApiResponse<HockeyMatchDto>? createApi = await createResp.Content.ReadFromJsonAsync<ApiResponse<HockeyMatchDto>>(jsonOptions);
            if (createApi?.Data == null)
            {
                throw new InvalidOperationException("Create hockey match failed: " + (createApi != null ? createApi.Message : "null response"));
            }

            HockeyMatchDto matchDto = createApi.Data;

            AddHomeAwayTeamsToHockeyMatchRequest teamsReq = new AddHomeAwayTeamsToHockeyMatchRequest
            {
                HomeTeamId = homeTeamId,
                AwayTeamId = awayTeamId
            };
            HttpResponseMessage teamsResp = await http.PutAsJsonAsync("api/HockeyMatch/" + matchDto.Id + "/teams", teamsReq);
            await SeederHttp.EnsureSuccessWithBody(teamsResp, "Assign Hockey Match Teams");

            ApiResponse<HockeyMatchDto>? teamsApi = await teamsResp.Content.ReadFromJsonAsync<ApiResponse<HockeyMatchDto>>(jsonOptions);
            if (teamsApi?.Data != null)
            {
                matchDto = teamsApi.Data;
            }

            created.Add(matchDto);
            Console.WriteLine("Created hockey match: " + match.HomeTeamName + " vs " + match.AwayTeamName + " (" + matchDto.Id + ")");

            if (match.SimulateCompleted || scheduled <= DateTime.UtcNow.AddMinutes(-5))
            {
                try
                {
                    matchDto = await HockeyMatchSimulation.SimulateCompletedAsync(http, jsonOptions, matchDto, rosterCache);
                    competitionsNeedingRecalc.Add(season.Id);
                    Console.WriteLine("Simulated finished hockey match " + matchDto.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Warning: failed to simulate hockey match " + matchDto.Id + ": " + ex.Message);
                }
            }
        }

        foreach (Guid competitionId in competitionsNeedingRecalc)
        {
            await HockeyMatchSimulation.RecalculateCompetitionAsync(http, competitionId);
        }

        return created;
    }

    public static async Task<List<HockeyMatchDto>> LoadCompetitionMatchesAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        Guid competitionId)
    {
        HttpResponseMessage resp = await http.GetAsync("api/HockeyMatch/competition/" + competitionId);
        if (!resp.IsSuccessStatusCode)
        {
            return new List<HockeyMatchDto>();
        }

        ApiResponse<List<HockeyMatchDto>>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<List<HockeyMatchDto>>>(jsonOptions);
        return api?.Data?.ToList() ?? new List<HockeyMatchDto>();
    }

    private static HockeySeasonDto ResolveSeason(string seasonName, List<HockeySeasonDto> seasons)
    {
        HockeySeasonDto? season = seasons.FirstOrDefault(s => string.Equals(s.Name, seasonName, StringComparison.OrdinalIgnoreCase));
        if (season == null)
        {
            throw new InvalidOperationException("Hockey season not found by name: " + seasonName);
        }
        return season;
    }

    private static Guid ResolveTeamId(string teamName, List<HockeyTeamDto> teams)
    {
        HockeyTeamDto? team = teams.FirstOrDefault(t => string.Equals(t.Name, teamName, StringComparison.OrdinalIgnoreCase));
        if (team == null)
        {
            throw new InvalidOperationException("Hockey team not found by name: " + teamName);
        }
        return team.Id;
    }
}
