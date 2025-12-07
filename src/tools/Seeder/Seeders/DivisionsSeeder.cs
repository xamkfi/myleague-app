using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Common;
using Domain.Enums.Common;
using WebAPI.Models.Common;

namespace Seeder;

public static class DivisionsSeeder
{
	public static async Task<List<DivisionDto>> SeedAsync(HttpClient http, JsonSerializerOptions jsonOptions, SeederConfiguration config)
	{
		List<DivisionDto> created = new List<DivisionDto>();

		foreach (DivisionSeed division in config.Divisions)
		{
            if (!Enum.TryParse<SportsCategory>(division.SportType, true, out SportsCategory seedSportType) ||
                seedSportType == SportsCategory.None)
            {
                throw new InvalidOperationException($"Invalid sport type '{division.SportType}' for division seed '{division.Name}'.");
            }

            // Idempotent check by name + sport type
            HttpResponseMessage listResp = await http.GetAsync("api/divisions");
            if (listResp.IsSuccessStatusCode)
            {
                ApiResponse<List<DivisionDto>>? listApi = await listResp.Content.ReadFromJsonAsync<ApiResponse<List<DivisionDto>>>(jsonOptions);
                if (listApi != null && listApi.Success && listApi.Data != null)
                {
                    DivisionDto? existingDiv = listApi.Data.FirstOrDefault(d =>
                        string.Equals(d.Name, division.Name, StringComparison.OrdinalIgnoreCase) &&
                        d.SportType == seedSportType);
                    if (existingDiv != null)
                    {
                        created.Add(existingDiv);
                        Console.WriteLine("Division exists, skipping: " + existingDiv.Name + " (" + existingDiv.Id + ")");
                        continue;
                    }
                }
            }

			CreateDivisionRequest request = new CreateDivisionRequest
			{
				Name = division.Name,
				Description = division.Description,
				Level = division.Level,
				SportType = seedSportType
			};

			HttpResponseMessage response = await http.PostAsJsonAsync("api/divisions", request);
			await SeederHttp.EnsureSuccessWithBody(response, "Create Division");

			ApiResponse<DivisionDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<DivisionDto>>(jsonOptions);
			if (api == null || !api.Success || api.Data == null)
			{
				throw new InvalidOperationException("Create division failed: " + (api != null ? api.Message : "null response"));
			}

			created.Add(api.Data);
			Console.WriteLine("Created division " + api.Data.Name + " (" + api.Data.Id + ")");
		}

		return created;
	}
}

