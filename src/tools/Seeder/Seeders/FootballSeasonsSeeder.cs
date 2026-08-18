using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Football.Seasons.DTOs;
using WebAPI.Models.Common;
using WebAPI.Models.Football;

namespace Seeder;

public static class FootballSeasonsSeeder
{
    public static async Task<List<FootballSeasonDto>> SeedAsync(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        List<FootballSeasonSeed> seasons,
        List<DivisionDto> divisions)
    {
        List<FootballSeasonDto> created = new List<FootballSeasonDto>();

        foreach (FootballSeasonSeed season in seasons)
        {
            if (season.DivisionNames.Count == 0)
            {
                throw new InvalidOperationException($"Season '{season.Name}' must have at least one division in DivisionNames.");
            }

            List<Guid> divisionIds = season.DivisionNames
                .Select(name => ResolveDivisionId(name, divisions))
                .Distinct()
                .ToList();

            HttpResponseMessage listResp = await http.GetAsync("api/FootballSeason");
            if (listResp.IsSuccessStatusCode)
            {
                ApiResponse<List<FootballSeasonDto>>? listApi = await listResp.Content.ReadFromJsonAsync<ApiResponse<List<FootballSeasonDto>>>(jsonOptions);
                if (listApi != null && listApi.Success && listApi.Data != null)
                {
                    FootballSeasonDto? existing = listApi.Data.FirstOrDefault(s =>
                        string.Equals(s.Name, season.Name, StringComparison.OrdinalIgnoreCase)
                        && s.SeasonDivisions != null
                        && s.SeasonDivisions.Any(sd => sd.DivisionId == divisionIds[0]));
                    if (existing != null)
                    {
                        created.Add(existing);
                        Console.WriteLine("Football season exists, skipping: " + existing.Name + " (" + existing.Id + ")");
                        continue;
                    }
                }
            }

            CreateFootballSeasonRequest request = new CreateFootballSeasonRequest
            {
                Name = season.Name,
                StartDate = season.StartDate,
                EndDate = season.EndDate,
                DivisionIds = divisionIds,
                NumberOfHalves = season.NumberOfHalves,
                HalfDurationMinutes = season.HalfDurationMinutes,
                PlayersOnField = season.PlayersOnField,
                RequireGoalkeeper = season.RequireGoalkeeper,
                MaxSubstitutions = season.MaxSubstitutions,
                RequireOfficialsToStart = season.RequireOfficialsToStart,
                AllowExtraTime = season.AllowExtraTime,
                ExtraTimeHalfCount = season.ExtraTimeHalfCount,
                ExtraTimeHalfDurationMinutes = season.ExtraTimeHalfDurationMinutes,
                AllowPenaltyShootout = season.AllowPenaltyShootout,
                WinPoints = season.WinPoints,
                DrawPoints = season.DrawPoints,
                LossPoints = season.LossPoints,
                TeamCategory = season.TeamCategory
            };

            HttpResponseMessage response = await http.PostAsJsonAsync("api/FootballSeason", request);
            await SeederHttp.EnsureSuccessWithBody(response, "Create Football Season");

            ApiResponse<FootballSeasonDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FootballSeasonDto>>(jsonOptions);
            if (api == null || !api.Success || api.Data == null)
            {
                throw new InvalidOperationException("Create football season failed: " + (api != null ? api.Message : "null response"));
            }

            created.Add(api.Data);
            Console.WriteLine("Created football season " + api.Data.Name + " (" + api.Data.Id + ") with " + divisionIds.Count + " division(s)");
        }

        return created;
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
}
