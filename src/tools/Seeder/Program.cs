using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.DTOs.Common;
using Application.DTOs.Floorball;

namespace Seeder;

public static class Program
{
    public static SeederConfiguration Configuration { get; private set; } = new SeederConfiguration();
	public static async Task<int> Main(string[] args)
	{
        SeederConfiguration config = SeederConfiguration.Load();
        Configuration = config;

		HttpClient http = new HttpClient();
		http.BaseAddress = new Uri(config.BaseUrl);
		http.DefaultRequestHeaders.Accept.Clear();
		http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		Console.WriteLine($"Seeding against {http.BaseAddress}");

		JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
		jsonOptions.PropertyNameCaseInsensitive = true;
		jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
		jsonOptions.Converters.Add(new JsonStringEnumConverter());

		try
		{
			List<PersonDto> basePersons = await PersonsSeeder.SeedAsync(http, jsonOptions, config);
			List<ClubDto> clubResults = await ClubsSeeder.SeedAsync(http, jsonOptions, config);
			List<DivisionDto> divisionResults = await DivisionsSeeder.SeedAsync(http, jsonOptions, config);

			// Create separate persons for players, goalies, referees and then create corresponding entities using their person IDs
			// Use the new method that returns seed email to person ID mapping
			(List<PersonDto> playerPersons, Dictionary<string, Guid> playerEmailToPersonId) = await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, config.PlayerPersons);
			(List<PersonDto> goaliePersons, Dictionary<string, Guid> goalieEmailToPersonId) = await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, config.GoaliePersons);
			(List<PersonDto> refereePersons, Dictionary<string, Guid> refereeEmailToPersonId) = await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, config.RefereePersons);

			// Merge player and goalie email mappings
			Dictionary<string, Guid> seedEmailToPersonId = new Dictionary<string, Guid>(playerEmailToPersonId, StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, Guid> kvp in goalieEmailToPersonId)
			{
				seedEmailToPersonId[kvp.Key] = kvp.Value;
			}

			(List<FloorballPlayerDto> players, Dictionary<string, Guid> emailToPlayerId) = await FloorballPlayersSeeder.SeedAsync(http, jsonOptions, playerPersons, goaliePersons, seedEmailToPersonId);
			List<FloorballRefereeDto> referees = await FloorballRefereesSeeder.SeedAsync(http, jsonOptions, refereePersons.Select(p => p.Id).ToList());

			// Optional: create seasons and teams (requires Divisions and Clubs)
			List<FloorballSeasonDto> seasons = await FloorballSeasonsSeeder.SeedAsync(http, jsonOptions, config.FloorballSeasons, divisionResults);
			List<FloorballTeamDto> teams = await FloorballTeamsSeeder.SeedTeamsAsync(http, jsonOptions, config.FloorballTeams, divisionResults, clubResults);

			// Assign teams to season divisions based on their division names
			await FloorballTeamsSeeder.AssignTeamsToSeasonsAsync(http, jsonOptions, seasons, config.FloorballTeams, teams, divisionResults);

			// Add players to teams per config
			foreach (FloorballTeamSeed teamSeed in config.FloorballTeams)
			{
				FloorballTeamDto? team = teams.FirstOrDefault(t => string.Equals(t.Name, teamSeed.Name, StringComparison.OrdinalIgnoreCase));
				if (team != null)
				{
					await FloorballTeamsSeeder.AddPlayersAsync(http, jsonOptions, team.Id, teamSeed.Players, emailToPlayerId);
				}
			}

			// Create matches for seasons
			Dictionary<string, Guid> emailToRefereeId = FloorballMatchesSeeder.BuildEmailToRefereeIdMap(referees, refereeEmailToPersonId);
			List<FloorballMatchDto> matches = await FloorballMatchesSeeder.SeedAsync(http, jsonOptions, config.FloorballMatches, seasons, teams, referees, emailToRefereeId);

			Console.WriteLine("\nSummary:");
			Console.WriteLine($"  Persons created: {basePersons.Count}");
			Console.WriteLine($"  Clubs created: {clubResults.Count}");
			Console.WriteLine($"  Divisions created: {divisionResults.Count}");
			Console.WriteLine($"  Floorball players created: {players.Count}");
			Console.WriteLine($"  Floorball referees created: {referees.Count}");
			Console.WriteLine($"  Seasons created: {seasons.Count}");
			Console.WriteLine($"  Teams created: {teams.Count}");
			Console.WriteLine($"  Matches created: {matches.Count}");

			http.Dispose();
			return 0;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"Seeder failed: {ex.Message}\n{ex}");
			http.Dispose();
			return 1;
		}
	}
}
