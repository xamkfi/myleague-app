using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using WebAPI.Models.Floorball;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;

namespace Seeder;

public static class FloorballMatchesSeeder
{
    public static async Task<List<FloorballMatchDto>> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<FloorballMatchSeed> matches,
        List<FloorballSeasonDto> seasons,
        List<FloorballTeamDto> teams,
        List<FloorballRefereeDto> referees,
        Dictionary<string, Guid> emailToRefereeId)
    {
        List<FloorballMatchDto> created = new List<FloorballMatchDto>();

        foreach (FloorballMatchSeed match in matches)
        {
            Guid seasonId = ResolveSeasonId(match.SeasonName, seasons);
            Guid homeTeamId = ResolveTeamId(match.HomeTeamName, teams);
            Guid awayTeamId = ResolveTeamId(match.AwayTeamName, teams);
            Guid? refereeId = ResolveRefereeId(match.RefereeEmail, emailToRefereeId);

            // Idempotent check: look for existing match with same season, home team, away team and scheduled time
            HttpResponseMessage listResp = await http.GetAsync($"api/floorball-matches?CompetitionId={seasonId}&Page=1&PageSize=0");
            if (listResp.IsSuccessStatusCode)
            {
                PaginatedApiResponse<FloorballMatchDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FloorballMatchDto>>(jsonOptions);
                if (listApi != null && listApi.Success && listApi.Data != null)
                {
                    FloorballMatchDto? existing = listApi.Data.FirstOrDefault(m =>
                        m.HomeTeamId == homeTeamId &&
                        m.AwayTeamId == awayTeamId);
                    if (existing != null)
                    {
                        created.Add(existing);
                        Console.WriteLine($"Match exists, skipping: {match.HomeTeamName} vs {match.AwayTeamName} ({existing.Id})");
                        continue;
                    }
                }
            }

            CreateFloorballMatchRequest request = new CreateFloorballMatchRequest
            {
                CompetitionId = seasonId,
                HomeTeamId = homeTeamId,
                AwayTeamId = awayTeamId,
                RefereeId = refereeId,
                ScheduledDateTime = match.ScheduledDateTime,
                Venue = match.Venue
            };

            HttpResponseMessage response = await http.PostAsJsonAsync("api/floorball-matches", request);
            await SeederHttp.EnsureSuccessWithBody(response, "Create Floorball Match");

            ApiResponse<FloorballMatchDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FloorballMatchDto>>(jsonOptions);
            if (api == null || !api.Success || api.Data == null)
            {
                throw new InvalidOperationException("Create floorball match failed: " + (api != null ? api.Message : "null response"));
            }

            created.Add(api.Data);
            Console.WriteLine($"Created floorball match: {match.HomeTeamName} vs {match.AwayTeamName} ({api.Data.Id})");
        }

        return created;
    }

    private static Guid ResolveSeasonId(string seasonName, List<FloorballSeasonDto> seasons)
    {
        FloorballSeasonDto? season = seasons.FirstOrDefault(s => string.Equals(s.Name, seasonName, StringComparison.OrdinalIgnoreCase));
        if (season == null) throw new InvalidOperationException($"Season not found by name: {seasonName}");
        return season.Id;
    }

    private static Guid ResolveTeamId(string teamName, List<FloorballTeamDto> teams)
    {
        FloorballTeamDto? team = teams.FirstOrDefault(t => string.Equals(t.Name, teamName, StringComparison.OrdinalIgnoreCase));
        if (team == null) throw new InvalidOperationException($"Team not found by name: {teamName}");
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

        Console.WriteLine($"Warning: Referee not found for email: {refereeEmail}");
        return null;
    }

    /// <summary>
    /// Builds a mapping from seed email to referee ID.
    /// Uses the seed email to person ID mapping to handle cases where existing persons have different emails in the database.
    /// </summary>
    public static Dictionary<string, Guid> BuildEmailToRefereeIdMap(
        List<FloorballRefereeDto> referees,
        Dictionary<string, Guid> seedEmailToPersonId)
    {
        Dictionary<string, Guid> map = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, Guid> kvp in seedEmailToPersonId)
        {
            string seedEmail = kvp.Key;
            Guid personId = kvp.Value;

            FloorballRefereeDto? referee = referees.FirstOrDefault(r => r.PersonId == personId);
            if (referee != null)
            {
                map[seedEmail] = referee.Id;
            }
        }

        return map;
    }

    /// <summary>
    /// Fetches all referees from the API (paginating) so the email-to-referee map includes referees that already existed in the database.
    /// </summary>
    public static async Task<List<FloorballRefereeDto>> FetchAllRefereesFromApiAsync(HttpClient http, JsonSerializerOptions jsonOptions)
    {
        List<FloorballRefereeDto> all = new List<FloorballRefereeDto>();
        // 50 is the most restrictive MaxPageSize across environments (Development sets Global.MaxPageSize = 50).
        const int pageSize = 50;
        int page = 1;
        while (true)
        {
            HttpResponseMessage resp = await http.GetAsync($"api/floorballreferee?page={page}&pageSize={pageSize}");
            if (!resp.IsSuccessStatusCode)
            {
                string body = await resp.Content.ReadAsStringAsync();
                Console.Error.WriteLine(
                    $"WARNING: FetchAllRefereesFromApiAsync got {(int)resp.StatusCode} {resp.StatusCode} from GET /api/floorballreferee?page={page}&pageSize={pageSize}. " +
                    $"Body: {(body.Length > 500 ? body.Substring(0, 500) + "..." : body)}");
                break;
            }
            PaginatedApiResponse<FloorballRefereeDto>? api = await resp.Content.ReadFromJsonAsync<PaginatedApiResponse<FloorballRefereeDto>>(jsonOptions);
            if (api?.Data == null || api.Data.Count() == 0)
            {
                if (page == 1)
                {
                    Console.WriteLine($"FetchAllRefereesFromApiAsync: GET returned no referees on page 1 (pageSize={pageSize}).");
                }
                break;
            }
            all.AddRange(api.Data);
            if (api.Data.Count() < pageSize)
                break;
            page++;
            if (page > 50)
                break;
        }
        return all;
    }
}
