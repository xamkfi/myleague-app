using System.Net.Http.Json;
using System.Text.Json;

namespace Seeder;

public static class FloorballPlayersSeeder
{
	public static async Task<(List<FloorballPlayerDto> players, Dictionary<string, Guid> emailToPlayerId)> SeedAsync(HttpClient http, JsonSerializerOptions jsonOptions, List<PersonDto> playerPersons, List<PersonDto> goaliePersons)
	{
		List<FloorballPlayerDto> created = new List<FloorballPlayerDto>();
		Dictionary<string, Guid> emailToPlayerId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

		List<PersonDto> all = new List<PersonDto>();
		all.AddRange(playerPersons);
		all.AddRange(goaliePersons);

		foreach (PersonDto person in all)
		{
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
			string? email = person.ContactInfo != null ? person.ContactInfo.Email : null;
			if (!string.IsNullOrWhiteSpace(email))
			{
				emailToPlayerId[email!] = api.Data.Id;
			}
			Console.WriteLine("Created floorball player for personId " + person.Id + " (playerId: " + api.Data.Id + ")");
		}

		return (created, emailToPlayerId);
	}
}

