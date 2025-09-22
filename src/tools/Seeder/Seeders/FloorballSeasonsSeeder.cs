using System.Net.Http.Json;
using System.Text.Json;

namespace Seeder;

public static class FloorballSeasonsSeeder
{
	public static async Task<List<FloorballSeasonDto>> SeedAsync(HttpClient http, JsonSerializerOptions jsonOptions, List<FloorballSeasonSeed> seasons, List<DivisionDto> divisions)
	{
		List<FloorballSeasonDto> created = new List<FloorballSeasonDto>();

		foreach (FloorballSeasonSeed season in seasons)
		{
			Guid divisionId = ResolveDivisionId(season.DivisionName, divisions);
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

