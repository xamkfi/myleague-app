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

		if (personIds.Count == 0)
		{
			Console.WriteLine("Floorball referees seed: no person IDs provided, nothing to seed.");
			return created;
		}

		Console.WriteLine($"Floorball referees seed: processing {personIds.Count} person ID(s).");

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
						// This is the silent-empty path that previously caused tournament match seeding to fail.
						// Surface it loudly so the cause is obvious in seeder output.
						Console.Error.WriteLine(
							$"WARNING: person {personId} is already a referee in the database, but the GET /api/floorballreferee listing did not return them. " +
							"This usually means the referee listing endpoint is failing silently (non-2xx, empty page, or the referee was filtered out). " +
							"Tournament match seeding will fall back to creating matches without an assigned referee.");
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

		Console.WriteLine($"Floorball referees seed: produced {created.Count} referee record(s) (created or pre-existing).");
		return created;
	}

	/// <summary>
	/// Fetches all referee pages until the referee with the given personId is found (handles backend pagination).
	/// Logs to stderr if the API returns a non-success status or an unexpected payload, so silent failures
	/// (e.g. a 500 from the listing endpoint) don't masquerade as "no referees in DB".
	/// </summary>
	private static async Task<FloorballRefereeDto?> FindRefereeByPersonIdAsync(HttpClient http, JsonSerializerOptions jsonOptions, Guid personId)
	{
		// Use 50 to stay within the most restrictive MaxPageSize across environments
		// (Development overrides Global.MaxPageSize to 50 in appsettings.Development.json).
		const int pageSize = 50;
		int page = 1;
		while (true)
		{
			HttpResponseMessage listResp = await http.GetAsync($"api/floorballreferee?page={page}&pageSize={pageSize}");
			if (!listResp.IsSuccessStatusCode)
			{
				string body = await listResp.Content.ReadAsStringAsync();
				Console.Error.WriteLine(
					$"WARNING: GET /api/floorballreferee?page={page}&pageSize={pageSize} returned {(int)listResp.StatusCode} {listResp.StatusCode}. " +
					$"Body: {Truncate(body, 500)}");
				return null;
			}
			PaginatedApiResponse<FloorballRefereeDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FloorballRefereeDto>>(jsonOptions);
			if (listApi == null || listApi.Data == null)
			{
				Console.Error.WriteLine(
					$"WARNING: GET /api/floorballreferee?page={page}&pageSize={pageSize} returned a payload with no Data field — cannot detect existing referees.");
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

	private static string Truncate(string value, int maxLength)
	{
		if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
		{
			return value ?? string.Empty;
		}
		return value.Substring(0, maxLength) + "...";
	}
}

