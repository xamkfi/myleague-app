using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Floorball;
using Application.DTOs.Common;
using WebAPI.Models.Floorball;
using WebAPI.Models.Common;

namespace Seeder;

public static class FloorballSeasonsSeeder
{
    public static async Task<List<FloorballSeasonDto>> SeedAsync(HttpClient http, JsonSerializerOptions jsonOptions, List<FloorballSeasonSeed> seasons, List<DivisionDto> divisions)
	{
		List<FloorballSeasonDto> created = new List<FloorballSeasonDto>();

		foreach (FloorballSeasonSeed season in seasons)
		{
			Guid divisionId = ResolveDivisionId(season.DivisionName, divisions);
			// Idempotent: check if season with same name exists
			HttpResponseMessage listResp = await http.GetAsync("api/floorballseason");
			if (listResp.IsSuccessStatusCode)
			{
				ApiResponse<List<FloorballSeasonDto>>? listApi = await listResp.Content.ReadFromJsonAsync<ApiResponse<List<FloorballSeasonDto>>>(jsonOptions);
				if (listApi != null && listApi.Success && listApi.Data != null)
				{
					FloorballSeasonDto? existing = listApi.Data.FirstOrDefault(s => string.Equals(s.Name, season.Name, StringComparison.OrdinalIgnoreCase)
						&& s.DivisionId == divisionId);
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
				DivisionId = divisionId
			};

			HttpResponseMessage response = await http.PostAsJsonAsync("api/floorballseason", request);
			await SeederHttp.EnsureSuccessWithBody(response, "Create Floorball Season");

			ApiResponse<FloorballSeasonDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FloorballSeasonDto>>(jsonOptions);
			if (api == null || !api.Success || api.Data == null)
			{
				throw new InvalidOperationException("Create floorball season failed: " + (api != null ? api.Message : "null response"));
			}

			created.Add(api.Data);
			Console.WriteLine("Created floorball season " + api.Data.Name + " (" + api.Data.Id + ")");

            // Add additional divisions to this season if defined
            if (season.AdditionalDivisionNames != null && season.AdditionalDivisionNames.Count > 0)
            {
                foreach (string divName in season.AdditionalDivisionNames)
                {
                    Guid extraDivisionId = ResolveDivisionId(divName, divisions);
                    if (extraDivisionId != divisionId)
                    {
                        HttpResponseMessage addDivResp = await http.PostAsync("api/floorballseason/" + api.Data.Id + "/divisions/" + extraDivisionId, null);
                        await SeederHttp.EnsureSuccess(addDivResp, "Add Division to Season");
                        Console.WriteLine("  Added division to season: " + divName + " (" + extraDivisionId + ")");
                    }
                }
            }
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

