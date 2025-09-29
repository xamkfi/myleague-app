using System.Net.Http.Headers;
using System.Text.Json;
using Application.DTOs.Common;
using Application.DTOs.Floorball;

namespace Seeder;

public static class Program
{
	public static async Task<int> Main(string[] args)
	{
		SeederConfiguration config = SeederConfiguration.Load();

		HttpClient http = new HttpClient();
		http.BaseAddress = new Uri(config.BaseUrl);
		http.DefaultRequestHeaders.Accept.Clear();
		http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		Console.WriteLine($"Seeding against {http.BaseAddress}");

		JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
		jsonOptions.PropertyNameCaseInsensitive = true;

		try
		{
			List<PersonDto> basePersons = await PersonsSeeder.SeedAsync(http, jsonOptions, config);
			List<ClubDto> clubResults = await ClubsSeeder.SeedAsync(http, jsonOptions, config);
			List<DivisionDto> divisionResults = await DivisionsSeeder.SeedAsync(http, jsonOptions, config);

			// Create separate persons for players, goalies, referees and then create corresponding entities using their person IDs
			List<PersonDto> playerPersons = await PersonsSeeder.SeedListAsync(http, jsonOptions, config.PlayerPersons);
			List<PersonDto> goaliePersons = await PersonsSeeder.SeedListAsync(http, jsonOptions, config.GoaliePersons);
			List<PersonDto> refereePersons = await PersonsSeeder.SeedListAsync(http, jsonOptions, config.RefereePersons);

			(List<FloorballPlayerDto> players, Dictionary<string, Guid> emailToPlayerId) = await FloorballPlayersSeeder.SeedAsync(http, jsonOptions, playerPersons, goaliePersons);
			List<FloorballRefereeDto> referees = await FloorballRefereesSeeder.SeedAsync(http, jsonOptions, refereePersons.Select(p => p.Id).ToList());

			// Optional: create seasons and teams (requires Divisions and Clubs)
			List<FloorballSeasonDto> seasons = await FloorballSeasonsSeeder.SeedAsync(http, jsonOptions, config.FloorballSeasons, divisionResults);
			List<FloorballTeamDto> teams = await FloorballTeamsSeeder.SeedTeamsAsync(http, jsonOptions, config.FloorballTeams, divisionResults, clubResults);

			// Add players to teams per config
			foreach (FloorballTeamSeed teamSeed in config.FloorballTeams)
			{
				FloorballTeamDto? team = teams.FirstOrDefault(t => string.Equals(t.Name, teamSeed.Name, StringComparison.OrdinalIgnoreCase));
				if (team != null)
				{
					await FloorballTeamsSeeder.AddPlayersAsync(http, jsonOptions, team.Id, teamSeed.Players, emailToPlayerId);
				}
			}

			Console.WriteLine("\nSummary:");
			Console.WriteLine($"  Persons created: {basePersons.Count}");
			Console.WriteLine($"  Clubs created: {clubResults.Count}");
			Console.WriteLine($"  Divisions created: {divisionResults.Count}");
			Console.WriteLine($"  Floorball players created: {players.Count}");
			Console.WriteLine($"  Floorball referees created: {referees.Count}");
			Console.WriteLine($"  Seasons created: {seasons.Count}");
			Console.WriteLine($"  Teams created: {teams.Count}");

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
