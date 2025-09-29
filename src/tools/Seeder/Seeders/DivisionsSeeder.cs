using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Common;
using WebAPI.Models.Common;

namespace Seeder;

public static class DivisionsSeeder
{
	public static async Task<List<DivisionDto>> SeedAsync(HttpClient http, JsonSerializerOptions jsonOptions, SeederConfiguration config)
	{
		List<DivisionDto> created = new List<DivisionDto>();

		foreach (DivisionSeed division in config.Divisions)
		{
			CreateDivisionRequest request = new CreateDivisionRequest
			{
				Name = division.Name,
				Description = division.Description,
				Level = division.Level,
				SportType = division.SportType
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

