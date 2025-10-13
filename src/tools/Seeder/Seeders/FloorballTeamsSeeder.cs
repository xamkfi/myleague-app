using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Floorball;
using Application.DTOs.Common;
using WebAPI.Models.Floorball;
using WebAPI.Models.Common;

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

			// Idempotent: check by name + club + division
			HttpResponseMessage listResp = await http.GetAsync("api/floorballteam?Page=1&PageSize=0");
			if (listResp.IsSuccessStatusCode)
			{
				PaginatedApiResponse<FloorballTeamDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FloorballTeamDto>>(jsonOptions);
				if (listApi != null && listApi.Success && listApi.Data != null)
				{
					FloorballTeamDto? existing = listApi.Data.FirstOrDefault(t => string.Equals(t.Name, team.Name, StringComparison.OrdinalIgnoreCase)
						&& t.DivisionId == divisionId && t.Club != null && t.Club.Id == clubId);
					if (existing != null)
					{
						created.Add(existing);
						Console.WriteLine("Team exists, skipping: " + existing.Name + " (" + existing.Id + ")");
						continue;
					}
				}
			}

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
                // Fallback: resolve person by email -> ensure player exists -> cache id
                HttpResponseMessage personResp = await http.GetAsync("api/persons/by-email?email=" + Uri.EscapeDataString(player.PersonEmail));
                if (!personResp.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("No person found for email: " + player.PersonEmail);
                }

                ApiResponse<PersonDto>? personApi = await personResp.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>(jsonOptions);
                if (personApi == null || !personApi.Success || personApi.Data == null)
                {
                    throw new InvalidOperationException("Failed to fetch person for email: " + player.PersonEmail);
                }

                Guid personId = personApi.Data.Id;

                // Check existing players for this person
                HttpResponseMessage listResp = await http.GetAsync("api/floorballplayer?Page=1&PageSize=0&IsActive=");
                if (listResp.IsSuccessStatusCode)
                {
                    PaginatedApiResponse<FloorballPlayerDto>? listApi = await listResp.Content.ReadFromJsonAsync<PaginatedApiResponse<FloorballPlayerDto>>(jsonOptions);
                    if (listApi != null && listApi.Success && listApi.Data != null)
                    {
                        FloorballPlayerDto? existing = listApi.Data.FirstOrDefault(p => p.PersonId == personId);
                        if (existing != null)
                        {
                            emailToPlayerId[player.PersonEmail] = existing.Id;
                            playerId = existing.Id;
                        }
                    }
                }

                if (playerId == Guid.Empty)
                {
                    CreateFloorballPlayerRequest createReq = new CreateFloorballPlayerRequest { PersonId = personId };
                    HttpResponseMessage createResp = await http.PostAsJsonAsync("api/floorballplayer", createReq);
                    await SeederHttp.EnsureSuccessWithBody(createResp, "Create Floorball Player (fallback)");

                    ApiResponse<FloorballPlayerDto>? createApi = await createResp.Content.ReadFromJsonAsync<ApiResponse<FloorballPlayerDto>>(jsonOptions);
                    if (createApi == null || !createApi.Success || createApi.Data == null)
                    {
                        throw new InvalidOperationException("Create floorball player (fallback) failed for email: " + player.PersonEmail);
                    }

                    playerId = createApi.Data.Id;
                    emailToPlayerId[player.PersonEmail] = playerId;
                }
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

