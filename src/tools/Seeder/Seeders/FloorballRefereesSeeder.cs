using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Floorball.Referees.DTOs;
using WebAPI.Models.Floorball;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;

namespace Seeder;

public static class FloorballRefereesSeeder
{
	public static async Task<List<FloorballRefereeDto>> SeedAsync(HttpClient http, JsonSerializerOptions jsonOptions, List<Guid> personIds)
	{
		List<FloorballRefereeDto> created = new List<FloorballRefereeDto>();

		foreach (Guid personId in personIds)
		{
			// Idempotent: find referee by personId (paginates through all pages)
			FloorballRefereeDto? existing = await FindRefereeByPersonIdAsync(http, jsonOptions, personId);
			if (existing != null)
			{
				created.Add(existing);
				Console.WriteLine("Referee exists for personId, skipping: " + personId + " (refereeId: " + existing.Id + ")");
				continue;
			}
			CreateFloorballRefereeRequest request = new CreateFloorballRefereeRequest
			{
				PersonId = personId,
				LicenseIssueDate = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"),
				LicenseExpiryDate = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-dd")
			};

			HttpResponseMessage response = await http.PostAsJsonAsync("api/floorballreferee", request);

			if (!response.IsSuccessStatusCode)
			{
				string body = await response.Content.ReadAsStringAsync();
				// Backend returns 400 "This person is already a referee" — find existing referee and add so match seeder can assign them
				if (response.StatusCode == System.Net.HttpStatusCode.BadRequest &&
					body.Contains("already a referee", StringComparison.OrdinalIgnoreCase))
				{
					FloorballRefereeDto? existingRef = await FindRefereeByPersonIdAsync(http, jsonOptions, personId);
					if (existingRef != null)
					{
						created.Add(existingRef);
						Console.WriteLine("Person already is a referee, using existing: " + personId + " (refereeId: " + existingRef.Id + ")");
					}
					else
					{
						Console.WriteLine("Person already is a referee but could not find in list, skipping: " + personId);
					}
					continue;
				}
				throw new HttpRequestException("Create Floorball Referee failed with " + (int)response.StatusCode + " " + response.StatusCode + ": " + body);
			}

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

	/// <summary>
	/// Fetches all referee pages until the referee with the given personId is found (handles backend pagination).
	/// </summary>
	private static async Task<FloorballRefereeDto?> FindRefereeByPersonIdAsync(HttpClient http, JsonSerializerOptions jsonOptions, Guid personId)
	{
		const int pageSize = 100;
		int page = 1;
		while (true)
		{
			HttpResponseMessage listResp = await http.GetAsync($"api/floorballreferee?page={page}&pageSize={pageSize}");
			if (!listResp.IsSuccessStatusCode)
			{
				return null;
			}
			PaginatedApiResponse<FloorballRefereeDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FloorballRefereeDto>>(jsonOptions);
			if (listApi?.Data == null)
			{
				return null;
			}
			FloorballRefereeDto? found = listApi.Data.FirstOrDefault(r => r.PersonId == personId);
			if (found != null)
			{
				return found;
			}
			// If we got fewer items than pageSize, no more pages
			int count = listApi.Data.Count();
			if (count < pageSize)
			{
				return null;
			}
			page++;
			// Safety: don't loop forever (e.g. if API always returns full page)
			if (page > 50)
			{
				return null;
			}
		}
	}
}

