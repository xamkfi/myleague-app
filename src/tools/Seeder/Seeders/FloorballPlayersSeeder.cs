using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Floorball;
using Application.DTOs.Common;
using WebAPI.Models.Floorball;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;

namespace Seeder;

public static class FloorballPlayersSeeder
{
	public static async Task<(List<FloorballPlayerDto> players, Dictionary<string, Guid> emailToPlayerId)> SeedAsync(
		HttpClient http,
		JsonSerializerOptions jsonOptions,
		List<PersonDto> playerPersons,
		List<PersonDto> goaliePersons,
		Dictionary<string, Guid> seedEmailToPersonId)
	{
		List<FloorballPlayerDto> created = new List<FloorballPlayerDto>();
		Dictionary<string, Guid> emailToPlayerId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

		// Build a reverse mapping: person ID -> list of seed emails
		Dictionary<Guid, List<string>> personIdToSeedEmails = new Dictionary<Guid, List<string>>();
		foreach (KeyValuePair<string, Guid> kvp in seedEmailToPersonId)
		{
			if (!personIdToSeedEmails.ContainsKey(kvp.Value))
			{
				personIdToSeedEmails[kvp.Value] = new List<string>();
			}
			personIdToSeedEmails[kvp.Value].Add(kvp.Key);
		}

		List<PersonDto> all = new List<PersonDto>();
		all.AddRange(playerPersons);
		all.AddRange(goaliePersons);

		foreach (PersonDto person in all)
		{
			// Idempotent: if player already exists for person, skip
			HttpResponseMessage listResp = await http.GetAsync("api/floorballplayer?Page=1&PageSize=0&IsActive=");
			if (listResp.IsSuccessStatusCode)
			{
				PaginatedApiResponse<FloorballPlayerDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FloorballPlayerDto>>(jsonOptions);
				if (listApi != null && listApi.Success && listApi.Data != null)
				{
					FloorballPlayerDto? existing = listApi.Data.FirstOrDefault(p => p.PersonId == person.Id);
					if (existing != null)
					{
						created.Add(existing);
						// Map all seed emails for this person to the player ID
						if (personIdToSeedEmails.TryGetValue(person.Id, out List<string>? seedEmails))
						{
							foreach (string seedEmail in seedEmails)
							{
								emailToPlayerId[seedEmail] = existing.Id;
							}
						}
						Console.WriteLine("Player exists for person, skipping: " + person.FullName + " (playerId: " + existing.Id + ")");
						continue;
					}
				}
			}
			CreateFloorballPlayerRequest request = new CreateFloorballPlayerRequest
			{
				PersonId = person.Id
			};

			HttpResponseMessage response = await http.PostAsJsonAsync("api/floorballplayer", request);
			await SeederHttp.EnsureSuccessWithBody(response, "Create Floorball Player");

			ApiResponse<FloorballPlayerDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FloorballPlayerDto>>(jsonOptions);
			if (api == null || !api.Success || api.Data == null)
			{
				throw new InvalidOperationException("Create floorball player failed: " + (api != null ? api.Message : "null response"));
			}

			created.Add(api.Data);
			// Map all seed emails for this person to the player ID
			if (personIdToSeedEmails.TryGetValue(person.Id, out List<string>? emails))
			{
				foreach (string seedEmail in emails)
				{
					emailToPlayerId[seedEmail] = api.Data.Id;
				}
			}
			Console.WriteLine("Created floorball player for personId " + person.Id + " (playerId: " + api.Data.Id + ")");
		}

		return (created, emailToPlayerId);
	}
}

