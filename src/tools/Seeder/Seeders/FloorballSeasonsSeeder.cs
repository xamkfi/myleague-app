using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Floorball;
using Application.DTOs.Common;
using WebAPI.Models.Floorball;
using WebAPI.Models.Common;
using Application.Features.Floorball.Seasons.DTOs;

namespace Seeder;

public static class FloorballSeasonsSeeder
{
	public static async Task<List<FloorballSeasonDto>> SeedAsync(HttpClient http, JsonSerializerOptions jsonOptions, List<FloorballSeasonSeed> seasons, List<DivisionDto> divisions)
	{
		List<FloorballSeasonDto> created = new List<FloorballSeasonDto>();

		foreach (FloorballSeasonSeed season in seasons)
		{
			if (season.DivisionNames.Count == 0)
			{
				throw new InvalidOperationException($"Season '{season.Name}' must have at least one division in DivisionNames.");
			}

			List<Guid> divisionIds = season.DivisionNames
				.Select(name => ResolveDivisionId(name, divisions))
				.Distinct()
				.ToList();

			// Idempotent: check if season with same name exists and contains the first division
			HttpResponseMessage listResp = await http.GetAsync("api/floorballseason");
			if (listResp.IsSuccessStatusCode)
			{
				ApiResponse<List<FloorballSeasonDto>>? listApi = await listResp.Content.ReadFromJsonAsync<ApiResponse<List<FloorballSeasonDto>>>(jsonOptions);
				if (listApi != null && listApi.Success && listApi.Data != null)
				{
					FloorballSeasonDto? existing = listApi.Data.FirstOrDefault(s => string.Equals(s.Name, season.Name, StringComparison.OrdinalIgnoreCase)
						&& s.SeasonDivisions != null && s.SeasonDivisions.Any(sd => sd.DivisionId == divisionIds[0]));
					if (existing != null)
					{
						created.Add(existing);
						Console.WriteLine("Season exists, skipping: " + existing.Name + " (" + existing.Id + ")");
						continue;
					}
				}
			}

			CreateFloorballSeasonRequest request = new CreateFloorballSeasonRequest
			{
				Name = season.Name,
				StartDate = season.StartDate,
				EndDate = season.EndDate,
				DivisionIds = divisionIds,
				NumberOfPeriods = season.NumberOfPeriods,
				PeriodDurationMinutes = season.PeriodDurationMinutes,
				AllowOvertime = season.AllowOvertime,
				OvertimeDurationMinutes = season.OvertimeDurationMinutes,
				AllowShootout = season.AllowShootout
			};

			HttpResponseMessage response = await http.PostAsJsonAsync("api/floorballseason", request);
			await SeederHttp.EnsureSuccessWithBody(response, "Create Floorball Season");

			ApiResponse<FloorballSeasonDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FloorballSeasonDto>>(jsonOptions);
			if (api == null || !api.Success || api.Data == null)
			{
				throw new InvalidOperationException("Create floorball season failed: " + (api != null ? api.Message : "null response"));
			}

			created.Add(api.Data);
			Console.WriteLine("Created floorball season " + api.Data.Name + " (" + api.Data.Id + ") with " + divisionIds.Count + " division(s)");
		}

		return created;
	}

	private static Guid ResolveDivisionId(string divisionName, List<DivisionDto> divisions)
	{
		DivisionDto? division = divisions.FirstOrDefault(d => string.Equals(d.Name, divisionName, StringComparison.OrdinalIgnoreCase));
		if (division == null) throw new InvalidOperationException("Division not found by name: " + divisionName);
		return division.Id;
	}
}

