using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.DTOs.Common;
using Application.DTOs.Floorball;
using Application.Features.Floorball.Seasons.DTOs;
using WebAPI.Models.Common;

namespace Seeder;

public static class Program
{
    private const string DefaultAuthEmail = "test@myleague.local";

    public static SeederConfiguration Configuration { get; private set; } = new SeederConfiguration();
	public static async Task<int> Main(string[] args)
	{
        SeederConfiguration config = SeederConfiguration.Load();
        Configuration = config;

		string baseUrl = PromptForBaseUrl(config.BaseUrl);
		config.BaseUrl = baseUrl;

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
			await AuthenticateAsync(http, jsonOptions);

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

			// Build referee map from all referees in the API so existing referees are found
			List<FloorballRefereeDto> allReferees = await FloorballMatchesSeeder.FetchAllRefereesFromApiAsync(http, jsonOptions);
			Dictionary<string, Guid> emailToRefereeId = FloorballMatchesSeeder.BuildEmailToRefereeIdMap(allReferees, refereeEmailToPersonId);
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

	private static async Task AuthenticateAsync(HttpClient http, JsonSerializerOptions jsonOptions)
	{
		Console.WriteLine("==========================================================");
		Console.WriteLine("Authentication");
		Console.WriteLine("==========================================================");
		Console.WriteLine($"Requesting login code for {DefaultAuthEmail}...");

		// Step 1: Request login code
		HttpResponseMessage loginResp = await http.PostAsJsonAsync("api/auth/login", new { email = DefaultAuthEmail });
		await SeederHttp.EnsureSuccessWithBody(loginResp, "Request login code");

		ApiResponse<LoginDevResponse>? loginApi = await loginResp.Content.ReadFromJsonAsync<ApiResponse<LoginDevResponse>>(jsonOptions);
		if (loginApi == null || !loginApi.Success || loginApi.Data == null || string.IsNullOrWhiteSpace(loginApi.Data.DevCode))
		{
			throw new InvalidOperationException("Failed to get dev login code. Make sure the API is running in Development mode.");
		}

		string code = loginApi.Data.DevCode;
		Console.WriteLine($"Received dev code: {code}");

		// Step 2: Verify code to get tokens
		HttpResponseMessage verifyResp = await http.PostAsJsonAsync("api/auth/verify", new { email = DefaultAuthEmail, code });
		await SeederHttp.EnsureSuccessWithBody(verifyResp, "Verify login code");

		ApiResponse<AuthTokenResponse>? verifyApi = await verifyResp.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResponse>>(jsonOptions);
		if (verifyApi == null || !verifyApi.Success || verifyApi.Data == null || string.IsNullOrWhiteSpace(verifyApi.Data.AccessToken))
		{
			throw new InvalidOperationException("Failed to get access token from verify response.");
		}

		// Step 3: Set Bearer token on HttpClient for all subsequent requests
		http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", verifyApi.Data.AccessToken);
		Console.WriteLine($"Authenticated successfully. Token expires at {verifyApi.Data.ExpiresAt:u}");
		Console.WriteLine("==========================================================\n");
	}

	private static string PromptForBaseUrl(string defaultUrl)
	{
		Console.WriteLine("==========================================================");
		Console.WriteLine("Seeder - API URL Configuration");
		Console.WriteLine("==========================================================");
		Console.WriteLine($"Default API URL: {defaultUrl}");
		Console.Write("Enter API URL (press Enter to use default): ");

		string? input = Console.ReadLine()?.Trim();

		if (string.IsNullOrWhiteSpace(input))
		{
			Console.WriteLine($"Using default: {defaultUrl}");
			return defaultUrl;
		}

		// Ensure URL has a scheme
		if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
			!input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			input = "http://" + input;
		}

		// Ensure URL ends with /
		if (!input.EndsWith('/'))
		{
			input += "/";
		}

		Console.WriteLine($"Using custom URL: {input}");
		return input;
	}
}
