using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Hockey.Seasons.DTOs;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace Seeder;

public static class HockeySeasonsSeeder
{
    public static async Task<List<HockeySeasonDto>> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<HockeySeasonSeed> seasons,
        List<DivisionDto> divisions)
    {
        List<HockeySeasonDto> created = new List<HockeySeasonDto>();

        HttpResponseMessage listResp = await http.GetAsync("api/HockeySeason");
        List<HockeySeasonDto> existingSeasons = new List<HockeySeasonDto>();
        if (listResp.IsSuccessStatusCode)
        {
            ApiResponse<List<HockeySeasonDto>>? listApi = await listResp.Content.ReadFromJsonAsync<ApiResponse<List<HockeySeasonDto>>>(jsonOptions);
            if (listApi?.Data != null)
            {
                existingSeasons.AddRange(listApi.Data);
            }
        }

        foreach (HockeySeasonSeed season in seasons)
        {
            if (season.DivisionNames.Count == 0)
            {
                throw new InvalidOperationException("Season '" + season.Name + "' must have at least one division in DivisionNames.");
            }

            HockeySeasonDto? existing = existingSeasons.FirstOrDefault(s =>
                string.Equals(s.Name, season.Name, StringComparison.OrdinalIgnoreCase));

            HockeySeasonDto seasonDto;
            if (existing != null)
            {
                seasonDto = await GetByIdAsync(http, jsonOptions, existing.Id) ?? existing;
                Console.WriteLine("Hockey season exists, skipping create: " + seasonDto.Name + " (" + seasonDto.Id + ")");
            }
            else
            {
                if (!DateTime.TryParse(season.StartDate, out DateTime startDate) ||
                    !DateTime.TryParse(season.EndDate, out DateTime endDate))
                {
                    throw new InvalidOperationException("Invalid date range for hockey season '" + season.Name + "'.");
                }

                CreateHockeySeasonRequest request = new CreateHockeySeasonRequest
                {
                    Name = season.Name,
                    StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc),
                    SeasonCode = season.SeasonCode,
                    TeamCategory = season.TeamCategory
                };

                HttpResponseMessage response = await http.PostAsJsonAsync("api/HockeySeason", request);
                await SeederHttp.EnsureSuccessWithBody(response, "Create Hockey Season");

                ApiResponse<HockeySeasonDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<HockeySeasonDto>>(jsonOptions);
                if (api?.Data == null)
                {
                    throw new InvalidOperationException("Create hockey season failed: " + (api != null ? api.Message : "null response"));
                }

                seasonDto = api.Data;
                Console.WriteLine("Created hockey season " + seasonDto.Name + " (" + seasonDto.Id + ")");
            }

            seasonDto = await EnsureDivisionsAsync(http, jsonOptions, seasonDto, season, divisions);
            seasonDto = await EnsurePublishedAndActiveAsync(http, jsonOptions, seasonDto);
            await SeasonContentBlocksSeeder.EnsureAsync(
                http, jsonOptions, "api/HockeySeason", seasonDto.Id, season.ContentBlocks, season.Name);
            created.Add(seasonDto);
        }

        return created;
    }

    private static async Task<HockeySeasonDto> EnsureDivisionsAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeySeasonDto season,
        HockeySeasonSeed seed,
        List<DivisionDto> divisions)
    {
        int sortOrder = 0;
        foreach (string divisionName in seed.DivisionNames)
        {
            Guid divisionId = ResolveDivisionId(divisionName, divisions);
            bool exists = season.Divisions.Any(d => d.DivisionId == divisionId && d.IsActive);
            if (exists)
            {
                continue;
            }

            AddDivisionToHockeySeasonRequest request = new AddDivisionToHockeySeasonRequest
            {
                DivisionId = divisionId,
                Name = divisionName,
                SortOrder = sortOrder++
            };

            HttpResponseMessage response = await http.PostAsJsonAsync("api/HockeySeason/" + season.Id + "/divisions", request);
            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                if (body.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                    body.Contains("already", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Division '" + divisionName + "' already on hockey season " + season.Name + ", skipping");
                    season = await GetByIdAsync(http, jsonOptions, season.Id) ?? season;
                    continue;
                }

                await SeederHttp.EnsureSuccessWithBody(response, "Add Division To Hockey Season");
            }

            ApiResponse<HockeySeasonDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<HockeySeasonDto>>(jsonOptions);
            if (api?.Data != null)
            {
                season = api.Data;
            }
            else
            {
                season = await GetByIdAsync(http, jsonOptions, season.Id) ?? season;
            }

            Console.WriteLine("Added division '" + divisionName + "' to hockey season " + season.Name);
        }

        return season;
    }

    private static async Task<HockeySeasonDto> EnsurePublishedAndActiveAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        HockeySeasonDto season)
    {
        if (string.Equals(season.Status, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            season = await PostLifecycleAsync(http, jsonOptions, season.Id, "publish", "Publish Hockey Season") ?? season;
        }

        if (string.Equals(season.Status, "Published", StringComparison.OrdinalIgnoreCase))
        {
            season = await PostLifecycleAsync(http, jsonOptions, season.Id, "open-registration", "Open Hockey Season Registration") ?? season;
        }

        if (string.Equals(season.Status, "Published", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(season.Status, "RegistrationOpen", StringComparison.OrdinalIgnoreCase))
        {
            season = await PostLifecycleAsync(http, jsonOptions, season.Id, "activate", "Activate Hockey Season") ?? season;
        }

        return season;
    }

    private static async Task<HockeySeasonDto?> PostLifecycleAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        Guid seasonId,
        string action,
        string operation)
    {
        HttpResponseMessage response = await http.PostAsync("api/HockeySeason/" + seasonId + "/" + action, content: null);
        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Warning: " + operation + " returned " + (int)response.StatusCode + ": " + Truncate(body));
            return await GetByIdAsync(http, jsonOptions, seasonId);
        }

        ApiResponse<HockeySeasonDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<HockeySeasonDto>>(jsonOptions);
        if (api?.Data != null)
        {
            Console.WriteLine(operation + " → status " + api.Data.Status);
            return api.Data;
        }

        return await GetByIdAsync(http, jsonOptions, seasonId);
    }

    private static async Task<HockeySeasonDto?> GetByIdAsync(HttpClient http, JsonSerializerOptions jsonOptions, Guid id)
    {
        HttpResponseMessage resp = await http.GetAsync("api/HockeySeason/" + id);
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        ApiResponse<HockeySeasonDto>? api = await resp.Content.ReadFromJsonAsync<ApiResponse<HockeySeasonDto>>(jsonOptions);
        return api?.Data;
    }

    private static Guid ResolveDivisionId(string divisionName, List<DivisionDto> divisions)
    {
        DivisionDto? division = divisions.FirstOrDefault(d => string.Equals(d.Name, divisionName, StringComparison.OrdinalIgnoreCase));
        if (division == null)
        {
            throw new InvalidOperationException("Division not found by name: " + divisionName);
        }
        return division.Id;
    }

    private static string Truncate(string body) => body.Length > 300 ? body.Substring(0, 300) + "..." : body;
}
