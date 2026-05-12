using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Tournaments.DTOs;
using WebAPI.Models.Floorball;
using WebAPI.Models.Common;

namespace Seeder;

public static class FloorballTournamentsSeeder
{
	public static async Task<List<FloorballTournamentDto>> SeedAsync(
		HttpClient http,
		JsonSerializerOptions jsonOptions,
		List<FloorballTournamentSeed> tournaments,
		List<FloorballTeamDto> teams)
	{
		List<FloorballTournamentDto> created = new List<FloorballTournamentDto>();

		foreach (FloorballTournamentSeed tournamentSeed in tournaments)
		{
			FloorballTournamentDto? tournament = await FindOrCreateTournamentAsync(http, jsonOptions, tournamentSeed);

			foreach (FloorballTournamentGroupSeed groupSeed in tournamentSeed.Groups)
			{
				tournament = await EnsureGroupAsync(http, jsonOptions, tournament, groupSeed);

				FloorballTournamentGroupDto? group = tournament.Groups.FirstOrDefault(g => string.Equals(g.Name, groupSeed.Name, StringComparison.OrdinalIgnoreCase));
				if (group == null)
				{
					throw new InvalidOperationException($"Group '{groupSeed.Name}' not present after creation in tournament '{tournament.Name}'.");
				}

				foreach (string teamName in groupSeed.TeamNames)
				{
					Guid teamId = ResolveTeamId(teamName, teams);

					if (group.Teams.Any(t => t.TeamId == teamId))
					{
						Console.WriteLine($"Team '{teamName}' already in group '{group.Name}' of tournament '{tournament.Name}', skipping");
						continue;
					}

					AddTeamToTournamentGroupRequest addTeamReq = new AddTeamToTournamentGroupRequest { TeamId = teamId };
					HttpResponseMessage addTeamResp = await http.PostAsJsonAsync($"api/floorballtournament/{tournament.Id}/groups/{group.Id}/teams", addTeamReq);
					await SeederHttp.EnsureSuccessWithBody(addTeamResp, "Add Team To Tournament Group");

					ApiResponse<FloorballTournamentDto>? addTeamApi = await addTeamResp.Content.ReadFromJsonAsync<ApiResponse<FloorballTournamentDto>>(jsonOptions);
					if (addTeamApi == null || !addTeamApi.Success || addTeamApi.Data == null)
					{
						throw new InvalidOperationException($"Add team to tournament group failed: {(addTeamApi != null ? addTeamApi.Message : "null response")}");
					}

					tournament = addTeamApi.Data;
					group = tournament.Groups.First(g => g.Id == group.Id);
					Console.WriteLine($"Added team '{teamName}' to group '{group.Name}' of tournament '{tournament.Name}'");
				}
			}

			created.Add(tournament);
			Console.WriteLine($"Tournament ready: {tournament.Name} ({tournament.Id}) with {tournament.Groups.Count} group(s) and {tournament.TeamCount} team(s)");
		}

		return created;
	}

	private static async Task<FloorballTournamentDto> FindOrCreateTournamentAsync(
		HttpClient http,
		JsonSerializerOptions jsonOptions,
		FloorballTournamentSeed seed)
	{
		HttpResponseMessage listResp = await http.GetAsync("api/floorballtournament");
		if (listResp.IsSuccessStatusCode)
		{
			ApiResponse<List<FloorballTournamentDto>>? listApi = await listResp.Content.ReadFromJsonAsync<ApiResponse<List<FloorballTournamentDto>>>(jsonOptions);
			if (listApi != null && listApi.Success && listApi.Data != null)
			{
				FloorballTournamentDto? existing = listApi.Data.FirstOrDefault(t => string.Equals(t.Name, seed.Name, StringComparison.OrdinalIgnoreCase));
				if (existing != null)
				{
					Console.WriteLine($"Tournament exists, skipping create: {existing.Name} ({existing.Id})");
					// The list endpoint does not eagerly load groups, so re-fetch full details by ID
					// to make subsequent idempotency checks (groups, group teams) work correctly.
					HttpResponseMessage detailResp = await http.GetAsync($"api/floorballtournament/{existing.Id}");
					if (detailResp.IsSuccessStatusCode)
					{
						ApiResponse<FloorballTournamentDto>? detailApi = await detailResp.Content.ReadFromJsonAsync<ApiResponse<FloorballTournamentDto>>(jsonOptions);
						if (detailApi != null && detailApi.Success && detailApi.Data != null)
						{
							return detailApi.Data;
						}
					}
					return existing;
				}
			}
		}

		CreateFloorballTournamentRequest request = new CreateFloorballTournamentRequest
		{
			Name = seed.Name,
			StartDate = ParseDate(seed.StartDate, nameof(seed.StartDate)),
			EndDate = ParseDate(seed.EndDate, nameof(seed.EndDate)),
			Venue = seed.Venue,
			ContentHtml = seed.ContentHtml,
			GroupStageNumberOfPeriods = seed.GroupStageNumberOfPeriods,
			GroupStagePeriodDurationMinutes = seed.GroupStagePeriodDurationMinutes,
			GroupStageAllowOvertime = seed.GroupStageAllowOvertime,
			GroupStageOvertimeDurationMinutes = seed.GroupStageOvertimeDurationMinutes,
			GroupStageAllowShootout = seed.GroupStageAllowShootout,
			PlayoffNumberOfPeriods = seed.PlayoffNumberOfPeriods,
			PlayoffPeriodDurationMinutes = seed.PlayoffPeriodDurationMinutes,
			PlayoffAllowOvertime = seed.PlayoffAllowOvertime,
			PlayoffOvertimeDurationMinutes = seed.PlayoffOvertimeDurationMinutes,
			PlayoffAllowShootout = seed.PlayoffAllowShootout,
			TeamsAdvancingPerGroup = seed.TeamsAdvancingPerGroup,
			HasPlayoffStage = seed.HasPlayoffStage,
			HasThirdPlaceMatch = seed.HasThirdPlaceMatch
		};

		HttpResponseMessage response = await http.PostAsJsonAsync("api/floorballtournament", request);
		await SeederHttp.EnsureSuccessWithBody(response, "Create Floorball Tournament");

		ApiResponse<FloorballTournamentDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FloorballTournamentDto>>(jsonOptions);
		if (api == null || !api.Success || api.Data == null)
		{
			throw new InvalidOperationException("Create floorball tournament failed: " + (api != null ? api.Message : "null response"));
		}

		Console.WriteLine($"Created floorball tournament {api.Data.Name} ({api.Data.Id})");
		return api.Data;
	}

	private static async Task<FloorballTournamentDto> EnsureGroupAsync(
		HttpClient http,
		JsonSerializerOptions jsonOptions,
		FloorballTournamentDto tournament,
		FloorballTournamentGroupSeed groupSeed)
	{
		if (tournament.Groups.Any(g => string.Equals(g.Name, groupSeed.Name, StringComparison.OrdinalIgnoreCase)))
		{
			Console.WriteLine($"Group '{groupSeed.Name}' already exists in tournament '{tournament.Name}', skipping");
			return tournament;
		}

		AddGroupToTournamentRequest request = new AddGroupToTournamentRequest { GroupName = groupSeed.Name };
		HttpResponseMessage response = await http.PostAsJsonAsync($"api/floorballtournament/{tournament.Id}/groups", request);
		await SeederHttp.EnsureSuccessWithBody(response, "Add Group To Tournament");

		ApiResponse<FloorballTournamentDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<FloorballTournamentDto>>(jsonOptions);
		if (api == null || !api.Success || api.Data == null)
		{
			throw new InvalidOperationException("Add group to tournament failed: " + (api != null ? api.Message : "null response"));
		}

		Console.WriteLine($"Added group '{groupSeed.Name}' to tournament '{tournament.Name}'");
		return api.Data;
	}

	private static Guid ResolveTeamId(string teamName, List<FloorballTeamDto> teams)
	{
		FloorballTeamDto? team = teams.FirstOrDefault(t => string.Equals(t.Name, teamName, StringComparison.OrdinalIgnoreCase));
		if (team == null)
		{
			throw new InvalidOperationException($"Team not found by name: {teamName}");
		}
		return team.Id;
	}

	private static DateTime ParseDate(string value, string fieldName)
	{
		if (!DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime parsed))
		{
			throw new InvalidOperationException($"Invalid {fieldName}: '{value}' (expected ISO-8601 date)");
		}
		return parsed;
	}
}
