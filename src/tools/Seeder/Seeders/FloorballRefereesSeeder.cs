using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Floorball;
using WebAPI.Models.Floorball;
using WebAPI.Models.Common;

namespace Seeder;

public static class FloorballRefereesSeeder
{
	public static async Task<List<FloorballRefereeDto>> SeedAsync(HttpClient http, JsonSerializerOptions jsonOptions, List<Guid> personIds)
	{
		List<FloorballRefereeDto> created = new List<FloorballRefereeDto>();

		foreach (Guid personId in personIds)
		{
			// Idempotent: list referees and skip if already exists for person
			HttpResponseMessage listResp = await http.GetAsync("api/floorballreferee?page=1&pageSize=0");
			if (listResp.IsSuccessStatusCode)
			{
				PaginatedApiResponse<FloorballRefereeDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FloorballRefereeDto>>(jsonOptions);
				if (listApi != null && listApi.Success && listApi.Data != null)
				{
					FloorballRefereeDto? existing = listApi.Data.FirstOrDefault(r => r.PersonId == personId);
					if (existing != null)
					{
						created.Add(existing);
						Console.WriteLine("Referee exists for personId, skipping: " + personId + " (refereeId: " + existing.Id + ")");
						continue;
					}
				}
			}
			CreateFloorballRefereeRequest request = new CreateFloorballRefereeRequest
			{
				PersonId = personId,
				LicenseIssueDate = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"),
				LicenseExpiryDate = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-dd")
			};

			HttpResponseMessage response = await http.PostAsJsonAsync("api/floorballreferee", request);
			await SeederHttp.EnsureSuccessWithBody(response, "Create Floorball Referee");

			ApiResponse<FloorballRefereeDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FloorballRefereeDto>>(jsonOptions);
			if (api == null || !api.Success || api.Data == null)
			{
				throw new InvalidOperationException("Create floorball referee failed: " + (api != null ? api.Message : "null response"));
			}

			created.Add(api.Data);
			Console.WriteLine("Created floorball referee for personId " + personId + " (refereeId: " + api.Data.Id + ")");
		}

		return created;
	}
}

