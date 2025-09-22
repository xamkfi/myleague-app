using System.Net.Http.Json;
using System.Text.Json;

namespace Seeder;

public static class FloorballTeamsSeeder
{
	public static async Task<List<FloorballTeamDto>> SeedTeamsAsync(HttpClient http, JsonSerializerOptions jsonOptions, List<FloorballTeamSeed> teams, List<DivisionDto> divisions, List<ClubDto> clubs)
	{
		List<FloorballTeamDto> created = new List<FloorballTeamDto>();

		foreach (FloorballTeamSeed team in teams)
		{
			Guid divisionId = ResolveDivisionId(team.DivisionName, divisions);
			Guid clubId = ResolveClubId(team.ClubName, clubs);

			FloorballTeamRequest request = new FloorballTeamRequest
			{
				Name = team.Name,
				DivisionId = divisionId,
				ClubId = clubId,
				HomeArena = team.HomeArena,
				PrimaryJerseyColor = team.PrimaryJerseyColor,
				SecondaryJerseyColor = team.SecondaryJerseyColor,
				Category = team.Category
			};

			HttpResponseMessage response = await http.PostAsJsonAsync("api/floorballteam", request);
			await SeederHttp.EnsureSuccessWithBody(response, "Create Floorball Team");

			ApiResponse<FloorballTeamDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FloorballTeamDto>>(jsonOptions);
			if (api == null || !api.Success || api.Data == null)
			{
				throw new InvalidOperationException("Create floorball team failed: " + (api != null ? api.Message : "null response"));
			}

			created.Add(api.Data);
			Console.WriteLine("Created floorball team " + api.Data.Name + " (" + api.Data.Id + ")");
		}

		return created;
	}

	public static async Task AddPlayersAsync(HttpClient http, JsonSerializerOptions jsonOptions, Guid teamId, List<TeamPlayerByEmailSeed> players, Dictionary<string, Guid> emailToPlayerId)
	{
		foreach (TeamPlayerByEmailSeed player in players)
		{
			if (!emailToPlayerId.TryGetValue(player.PersonEmail, out Guid playerId))
			{
				throw new InvalidOperationException("No player entity found for email: " + player.PersonEmail);
			}

			int positionValue = (int)player.Position;
			HttpResponseMessage response = await http.PostAsync($"api/floorballteam/{teamId}/players/{playerId}?position={positionValue}&jerseyNumber={player.JerseyNumber}", null);
			await SeederHttp.EnsureSuccessWithBody(response, "Add Player To Team");
		}
	}

	private static Guid ResolveDivisionId(string divisionName, List<DivisionDto> divisions)
	{
		DivisionDto? division = divisions.FirstOrDefault(d => string.Equals(d.Name, divisionName, StringComparison.OrdinalIgnoreCase));
		if (division == null) throw new InvalidOperationException("Division not found by name: " + divisionName);
		return division.Id;
	}

	private static Guid ResolveClubId(string clubName, List<ClubDto> clubs)
	{
		ClubDto? club = clubs.FirstOrDefault(c => string.Equals(c.Name, clubName, StringComparison.OrdinalIgnoreCase));
		if (club == null) throw new InvalidOperationException("Club not found by name: " + clubName);
		return club.Id;
	}
}

