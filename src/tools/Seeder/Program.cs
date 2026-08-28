using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Officials.DTOs;
using Application.Features.Hockey.Players.DTOs;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Tournaments.DTOs;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Tournaments.DTOs;
using WebAPI.Models.Common;

namespace Seeder;

public static class Program
{
    private const string DefaultAuthEmail = "test@myleague.local";

    public static SeederConfiguration Configuration { get; private set; } = new SeederConfiguration();
	public static async Task<int> Main(string[] args)
	{
		SeedSport sport = ParseSportOrExit(args);
		bool seedFloorball = sport == SeedSport.Floorball || sport == SeedSport.All;
		bool seedFootball = sport == SeedSport.Football || sport == SeedSport.All;

		SeederConfiguration urlConfig = seedFootball && !seedFloorball
			? SeederConfiguration.LoadFootball()
			: SeederConfiguration.Load();
		Configuration = urlConfig;

		string baseUrl = PromptForBaseUrl(urlConfig.BaseUrl);
		urlConfig.BaseUrl = baseUrl;

		SeedScope requested = PromptForScope(args);
		SeedScope scope = SeedScopeResolver.Resolve(requested);
		PrintEffectiveScope(requested, scope);
		Console.WriteLine($"Sport: {sport.ToString().ToLowerInvariant()}");

		HttpClient http = new HttpClient();
		http.BaseAddress = new Uri(baseUrl);
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

			FloorballSeedResult? floorballResult = null;
			FootballSeedResult? footballResult = null;
			HockeySeedResult? hockeyResult = null;

			if (seedFloorball)
			{
				SeederConfiguration floorballConfig = SeederConfiguration.Load();
				floorballConfig.BaseUrl = baseUrl;
				Configuration = floorballConfig;
				Console.WriteLine("\n--- Floorball seed ---");
				floorballResult = await SeedFloorballAsync(http, jsonOptions, floorballConfig, scope);
			}

			if (seedFootball)
			{
				SeederConfiguration footballConfig = SeederConfiguration.LoadFootball();
				footballConfig.BaseUrl = baseUrl;
				Configuration = footballConfig;
				Console.WriteLine("\n--- Football seed ---");
				footballResult = await SeedFootballAsync(http, jsonOptions, footballConfig, scope);
			}

			bool seedHockey = scope.HasFlag(SeedScope.HockeyPlayers)
				|| scope.HasFlag(SeedScope.HockeyTeams)
				|| scope.HasFlag(SeedScope.HockeySeasons)
				|| scope.HasFlag(SeedScope.HockeySeasonMatches)
				|| scope.HasFlag(SeedScope.HockeyTournaments);

			if (seedHockey)
			{
				SeederConfiguration hockeyConfig = SeederConfiguration.Load();
				hockeyConfig.BaseUrl = baseUrl;
				Configuration = hockeyConfig;
				Console.WriteLine("\n--- Hockey seed ---");
				hockeyResult = await SeedHockeyAsync(
					http,
					jsonOptions,
					hockeyConfig,
					scope,
					floorballResult?.Clubs ?? footballResult?.Clubs ?? [],
					floorballResult?.Divisions ?? footballResult?.Divisions ?? []);
			}

			WriteCombinedSummary(scope, seedFloorball, seedFootball, seedHockey, floorballResult, footballResult, hockeyResult);

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

		HttpResponseMessage loginResp = await http.PostAsJsonAsync("api/auth/login", new { email = DefaultAuthEmail });
		await SeederHttp.EnsureSuccessWithBody(loginResp, "Request login code");

		ApiResponse<LoginDevResponse>? loginApi = await loginResp.Content.ReadFromJsonAsync<ApiResponse<LoginDevResponse>>(jsonOptions);
		if (loginApi == null || !loginApi.Success || loginApi.Data == null || string.IsNullOrWhiteSpace(loginApi.Data.DevCode))
		{
			throw new InvalidOperationException("Failed to get dev login code. Make sure the API is running in Development mode.");
		}

		string code = loginApi.Data.DevCode;
		Console.WriteLine($"Received dev code: {code}");

		HttpResponseMessage verifyResp = await http.PostAsJsonAsync("api/auth/verify", new { email = DefaultAuthEmail, code });
		await SeederHttp.EnsureSuccessWithBody(verifyResp, "Verify login code");

		ApiResponse<AuthTokenResponse>? verifyApi = await verifyResp.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResponse>>(jsonOptions);
		if (verifyApi == null || !verifyApi.Success || verifyApi.Data == null || string.IsNullOrWhiteSpace(verifyApi.Data.AccessToken))
		{
			throw new InvalidOperationException("Failed to get access token from verify response.");
		}

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

		if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
			!input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			input = "http://" + input;
		}

		if (!input.EndsWith('/'))
		{
			input += "/";
		}

		Console.WriteLine($"Using custom URL: {input}");
		return input;
	}

	private static SeedScope PromptForScope(string[] args)
	{
		string? cliValue = TryGetScopeArg(args);
		if (cliValue != null)
		{
			if (TryParseScopeArgValue(cliValue, out SeedScope cliScope))
			{
				Console.WriteLine($"Using --scope from command line: \"{cliValue}\"");
				return cliScope;
			}
			Console.Error.WriteLine($"Invalid --scope value: '{cliValue}'. Valid tokens: all, hockey, hockeyall, persons, clubs, divisions, playersreferees, teams, seasons, seasonmatches, tournaments, hockeyplayers, hockeyteams, hockeyseasons, hockeyseasonmatches, hockeytournaments.");
			Environment.Exit(2);
			return SeedScope.None;
		}

		const int maxAttempts = 3;
		int attempts = 0;
		while (attempts < maxAttempts)
		{
			PrintScopeMenu();
			Console.Write("> ");
			string? line = Console.ReadLine();
			string trimmed = (line ?? string.Empty).Trim();

			SeedScope? parsed = TryParseMenuInput(trimmed);
			if (parsed == null)
			{
				attempts++;
				Console.WriteLine($"Invalid input. (attempt {attempts}/{maxAttempts})");
				Console.WriteLine();
				continue;
			}

			SeedScope requested = parsed.Value;
			SeedScope effective = SeedScopeResolver.Resolve(requested);
			Console.WriteLine();
			Console.WriteLine(SeedScopeResolver.Explain(effective, requested));

			while (true)
			{
				Console.Write("Proceed? (Y/n): ");
				string? proceedInput = Console.ReadLine();
				string proceedTrimmed = (proceedInput ?? string.Empty).Trim();
				if (string.IsNullOrEmpty(proceedTrimmed) || string.Equals(proceedTrimmed, "y", StringComparison.OrdinalIgnoreCase))
				{
					return requested;
				}
				if (string.Equals(proceedTrimmed, "n", StringComparison.OrdinalIgnoreCase))
				{
					Console.WriteLine("Aborted by user.");
					Environment.Exit(0);
				}
			}
		}

		Console.Error.WriteLine("Too many invalid attempts. Exiting.");
		Environment.Exit(2);
		return SeedScope.None;
	}

	private static void PrintScopeMenu()
	{
		Console.WriteLine("==========================================================");
		Console.WriteLine("Seeder - Scope Selection");
		Console.WriteLine("==========================================================");
		Console.WriteLine("What do you want to seed? (auto-resolves dependencies)");
		Console.WriteLine();
		Console.WriteLine("  1) Henkilöt (Persons)              — base persons + player/goalie/referee persons");
		Console.WriteLine("  2) Seurat (Clubs)");
		Console.WriteLine("  3) Divisioonat (Divisions)");
		Console.WriteLine("  4) Pelaajat ja tuomarit            — needs 1");
		Console.WriteLine("  5) Joukkueet (Teams + rosters)     — needs 1, 2, 3, 4");
		Console.WriteLine("  6) Kaudet (Seasons + team-to-season assignment)  — needs 1, 2, 3, 5");
		Console.WriteLine("  7) Kausi-ottelut (Season matches)  — needs 1, 2, 3, 4, 5, 6");
		Console.WriteLine("  8) Turnaukset ja turnausottelut    — needs 1, 2, 3, 4, 5");
		Console.WriteLine("  9) Kaikki Floorball (Everything floorball)");
		Console.WriteLine(" 10) Hockey kaikki (HockeyAll)       — Icehockey pipeline");
		Console.WriteLine();
		Console.WriteLine("Enter selection: comma-separated numbers (e.g. \"1,2,5\") or \"9\" / \"all\" / \"10\" / \"hockey\" / blank for floorball all.");
	}

	private static SeedScope? TryParseMenuInput(string trimmed)
	{
		if (string.IsNullOrEmpty(trimmed) ||
			string.Equals(trimmed, "9", StringComparison.OrdinalIgnoreCase) ||
			string.Equals(trimmed, "all", StringComparison.OrdinalIgnoreCase))
		{
			return SeedScope.All;
		}

		if (string.Equals(trimmed, "10", StringComparison.OrdinalIgnoreCase) ||
			string.Equals(trimmed, "hockey", StringComparison.OrdinalIgnoreCase) ||
			string.Equals(trimmed, "hockeyall", StringComparison.OrdinalIgnoreCase))
		{
			return SeedScope.HockeyAll;
		}

		string[] tokens = trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (tokens.Length == 0)
		{
			return null;
		}

		SeedScope result = SeedScope.None;
		foreach (string tok in tokens)
		{
			SeedScope? mapped = MapMenuNumber(tok);
			if (mapped == null)
			{
				return null;
			}
			result |= mapped.Value;
		}
		return result == SeedScope.None ? null : result;
	}

	private static SeedScope? MapMenuNumber(string token)
	{
		return token switch
		{
			"1" => SeedScope.Persons,
			"2" => SeedScope.Clubs,
			"3" => SeedScope.Divisions,
			"4" => SeedScope.PlayersReferees,
			"5" => SeedScope.Teams,
			"6" => SeedScope.Seasons,
			"7" => SeedScope.SeasonMatches,
			"8" => SeedScope.Tournaments,
			"9" => SeedScope.All,
			"10" => SeedScope.HockeyAll,
			_ => null
		};
	}

	private static string? TryGetScopeArg(string[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			string a = args[i];
			if (a.StartsWith("--scope=", StringComparison.OrdinalIgnoreCase))
			{
				return a.Substring("--scope=".Length);
			}
			if (string.Equals(a, "--scope", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
			{
				return args[i + 1];
			}
		}
		return null;
	}

	private static bool TryParseScopeArgValue(string value, out SeedScope scope)
	{
		scope = SeedScope.None;
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}
		string trimmed = value.Trim();
		if (string.Equals(trimmed, "all", StringComparison.OrdinalIgnoreCase))
		{
			scope = SeedScope.All;
			return true;
		}
		string[] tokens = trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (tokens.Length == 0)
		{
			return false;
		}
		foreach (string tok in tokens)
		{
			if (!SeedScopeResolver.TryParseToken(tok, out SeedScope parsed))
			{
				return false;
			}
			scope |= parsed;
		}
		return scope != SeedScope.None;
	}

	private static void PrintEffectiveScope(SeedScope requested, SeedScope scope)
	{
		Console.WriteLine();
		Console.WriteLine("==========================================================");
		Console.WriteLine("Effective seed scope");
		Console.WriteLine("==========================================================");
		Console.WriteLine(SeedScopeResolver.Explain(scope, requested));
		Console.WriteLine("==========================================================\n");
	}

	private static async Task<FloorballSeedResult> SeedFloorballAsync(
		HttpClient http,
		JsonSerializerOptions jsonOptions,
		SeederConfiguration config,
		SeedScope scope)
	{
		FloorballSeedResult result = new FloorballSeedResult();

		result.Persons = scope.HasFlag(SeedScope.Persons)
			? await PersonsSeeder.SeedAsync(http, jsonOptions, config)
			: new List<PersonDto>();

		result.Clubs = scope.HasFlag(SeedScope.Clubs)
			? await ClubsSeeder.SeedAsync(http, jsonOptions, config)
			: new List<ClubDto>();

		result.Divisions = scope.HasFlag(SeedScope.Divisions)
			? await DivisionsSeeder.SeedAsync(http, jsonOptions, config)
			: new List<DivisionDto>();

		List<PersonDto> playerPersons = new List<PersonDto>();
		List<PersonDto> goaliePersons = new List<PersonDto>();
		List<PersonDto> refereePersons = new List<PersonDto>();
		Dictionary<string, Guid> playerEmailToPersonId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, Guid> goalieEmailToPersonId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, Guid> refereeEmailToPersonId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, Guid> emailToPlayerId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

		if (scope.HasFlag(SeedScope.PlayersReferees))
		{
			(playerPersons, playerEmailToPersonId) = await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, config.PlayerPersons);
			(goaliePersons, goalieEmailToPersonId) = await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, config.GoaliePersons);
			(refereePersons, refereeEmailToPersonId) = await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, config.RefereePersons);

			Dictionary<string, Guid> seedEmailToPersonId = new Dictionary<string, Guid>(playerEmailToPersonId, StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, Guid> kvp in goalieEmailToPersonId)
			{
				seedEmailToPersonId[kvp.Key] = kvp.Value;
			}

			(result.Players, emailToPlayerId) = await FloorballPlayersSeeder.SeedAsync(http, jsonOptions, playerPersons, goaliePersons, seedEmailToPersonId);
			result.Referees = await FloorballRefereesSeeder.SeedAsync(http, jsonOptions, refereePersons.Select(p => p.Id).ToList());
		}

		if (scope.HasFlag(SeedScope.Teams))
		{
			result.Teams = await FloorballTeamsSeeder.SeedTeamsAsync(http, jsonOptions, config.FloorballTeams, result.Divisions, result.Clubs);
		}

		if (scope.HasFlag(SeedScope.Seasons))
		{
			result.Seasons = await FloorballSeasonsSeeder.SeedAsync(http, jsonOptions, config.FloorballSeasons, result.Divisions);
			await FloorballTeamsSeeder.AssignTeamsToSeasonsAsync(http, jsonOptions, result.Seasons, config.FloorballTeams, result.Teams, result.Divisions);
		}

		if (scope.HasFlag(SeedScope.Teams))
		{
			foreach (FloorballTeamSeed teamSeed in config.FloorballTeams)
			{
				FloorballTeamDto? team = result.Teams.FirstOrDefault(t => string.Equals(t.Name, teamSeed.Name, StringComparison.OrdinalIgnoreCase));
				if (team != null)
				{
					await FloorballTeamsSeeder.AddPlayersAsync(http, jsonOptions, team.Id, teamSeed.Players, emailToPlayerId);
				}
			}
		}

		List<FloorballRefereeDto> allReferees = new List<FloorballRefereeDto>();
		bool needsAllReferees = scope.HasFlag(SeedScope.SeasonMatches) || scope.HasFlag(SeedScope.Tournaments);
		if (needsAllReferees)
		{
			allReferees = await FloorballMatchesSeeder.FetchAllRefereesFromApiAsync(http, jsonOptions);
		}

		if (scope.HasFlag(SeedScope.SeasonMatches))
		{
			Dictionary<string, Guid> emailToRefereeId = FloorballMatchesSeeder.BuildEmailToRefereeIdMap(allReferees, refereeEmailToPersonId);
			result.Matches = await FloorballMatchesSeeder.SeedAsync(http, jsonOptions, config.FloorballMatches, result.Seasons, result.Teams, result.Referees, emailToRefereeId);
		}

		if (scope.HasFlag(SeedScope.Tournaments))
		{
			result.Tournaments = await FloorballTournamentsSeeder.SeedAsync(http, jsonOptions, config.FloorballTournaments, result.Teams);

			List<FloorballRefereeDto> tournamentReferees = result.Referees.Concat(allReferees)
				.GroupBy(r => r.Id)
				.Select(g => g.First())
				.ToList();

			if (tournamentReferees.Count == 0 && refereePersons.Count > 0)
			{
				Console.Error.WriteLine(
					$"WARNING: tournament match seeding has no referees available even though {refereePersons.Count} referee person(s) were configured. " +
					"Matches will be created without an assigned referee. See earlier WARNING lines for the underlying API response.");
			}
			result.TournamentMatchesCreated = await FloorballTournamentMatchesSeeder.SeedAsync(http, jsonOptions, result.Tournaments, tournamentReferees, config.FloorballTournaments);
		}

		return result;
	}

	private static async Task<FootballSeedResult> SeedFootballAsync(
		HttpClient http,
		JsonSerializerOptions jsonOptions,
		SeederConfiguration config,
		SeedScope scope)
	{
		FootballSeedResult result = new FootballSeedResult();

		result.Persons = scope.HasFlag(SeedScope.Persons)
			? await PersonsSeeder.SeedAsync(http, jsonOptions, config)
			: new List<PersonDto>();

		result.Clubs = scope.HasFlag(SeedScope.Clubs)
			? await ClubsSeeder.SeedAsync(http, jsonOptions, config)
			: new List<ClubDto>();

		result.Divisions = scope.HasFlag(SeedScope.Divisions)
			? await DivisionsSeeder.SeedAsync(http, jsonOptions, config)
			: new List<DivisionDto>();

		List<PersonDto> playerPersons = new List<PersonDto>();
		List<PersonDto> goaliePersons = new List<PersonDto>();
		List<PersonDto> refereePersons = new List<PersonDto>();
		Dictionary<string, Guid> playerEmailToPersonId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, Guid> goalieEmailToPersonId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, Guid> refereeEmailToPersonId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, Guid> emailToPlayerId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

		if (scope.HasFlag(SeedScope.PlayersReferees))
		{
			(playerPersons, playerEmailToPersonId) = await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, config.PlayerPersons);
			(goaliePersons, goalieEmailToPersonId) = await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, config.GoaliePersons);
			(refereePersons, refereeEmailToPersonId) = await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, config.RefereePersons);

			Dictionary<string, Guid> seedEmailToPersonId = new Dictionary<string, Guid>(playerEmailToPersonId, StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, Guid> kvp in goalieEmailToPersonId)
			{
				seedEmailToPersonId[kvp.Key] = kvp.Value;
			}

			(result.Players, emailToPlayerId) = await FootballPlayersSeeder.SeedAsync(http, jsonOptions, playerPersons, goaliePersons, seedEmailToPersonId);
			result.Referees = await FootballRefereesSeeder.SeedAsync(http, jsonOptions, refereePersons.Select(p => p.Id).ToList());
		}

		if (scope.HasFlag(SeedScope.Teams))
		{
			result.Teams = await FootballTeamsSeeder.SeedTeamsAsync(http, jsonOptions, config.FootballTeams, result.Divisions, result.Clubs);
		}

		if (scope.HasFlag(SeedScope.Seasons))
		{
			result.Seasons = await FootballSeasonsSeeder.SeedAsync(http, jsonOptions, config.FootballSeasons, result.Divisions);
			await FootballTeamsSeeder.AssignTeamsToSeasonsAsync(
				http,
				jsonOptions,
				result.Seasons,
				config.FootballSeasons,
				config.FootballTeams,
				result.Teams,
				result.Divisions);
		}

		if (scope.HasFlag(SeedScope.Teams))
		{
			foreach (FootballTeamSeed teamSeed in config.FootballTeams)
			{
				FootballTeamDto? team = result.Teams.FirstOrDefault(t => string.Equals(t.Name, teamSeed.Name, StringComparison.OrdinalIgnoreCase));
				if (team != null)
				{
					await FootballTeamsSeeder.AddPlayersAsync(http, jsonOptions, team.Id, teamSeed.Players, emailToPlayerId);
				}
			}
		}

		List<FootballRefereeDto> allReferees = new List<FootballRefereeDto>();
		bool needsAllReferees = scope.HasFlag(SeedScope.SeasonMatches) || scope.HasFlag(SeedScope.Tournaments);
		if (needsAllReferees)
		{
			allReferees = await FootballRefereesSeeder.FetchAllRefereesFromApiAsync(http, jsonOptions);
		}

		if (scope.HasFlag(SeedScope.SeasonMatches))
		{
			Dictionary<string, Guid> emailToRefereeId = FootballRefereesSeeder.BuildEmailToRefereeIdMap(allReferees, refereeEmailToPersonId);
			result.Matches = await FootballMatchesSeeder.SeedAsync(http, jsonOptions, config.FootballMatches, result.Seasons, result.Teams, result.Referees, emailToRefereeId);
		}

		if (scope.HasFlag(SeedScope.Tournaments))
		{
			result.Tournaments = await FootballTournamentsSeeder.SeedAsync(http, jsonOptions, config.FootballTournaments, result.Teams);

			List<FootballRefereeDto> tournamentReferees = result.Referees.Concat(allReferees)
				.GroupBy(r => r.Id)
				.Select(g => g.First())
				.ToList();

			if (tournamentReferees.Count == 0 && refereePersons.Count > 0)
			{
				Console.Error.WriteLine(
					$"WARNING: football tournament match seeding has no referees available even though {refereePersons.Count} referee person(s) were configured. " +
					"Matches will be created without an assigned referee. See earlier WARNING lines for the underlying API response.");
			}
			result.TournamentMatchesCreated = await FootballTournamentMatchesSeeder.SeedAsync(http, jsonOptions, result.Tournaments, tournamentReferees, config.FootballTournaments);
		}

		return result;
	}

	private static async Task<HockeySeedResult> SeedHockeyAsync(
		HttpClient http,
		JsonSerializerOptions jsonOptions,
		SeederConfiguration config,
		SeedScope scope,
		IReadOnlyList<ClubDto> existingClubs,
		IReadOnlyList<DivisionDto> existingDivisions)
	{
		HockeySeedResult result = new HockeySeedResult();
		List<ClubDto> clubs = existingClubs.Count > 0
			? existingClubs.ToList()
			: await ClubsSeeder.SeedAsync(http, jsonOptions, config);
		List<DivisionDto> divisions = existingDivisions.Count > 0
			? existingDivisions.ToList()
			: await DivisionsSeeder.SeedAsync(http, jsonOptions, config);

		Dictionary<string, Guid> hockeyEmailToPlayerId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, Guid> hockeyEmailToPersonId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

		if (scope.HasFlag(SeedScope.HockeyPlayers))
		{
			HashSet<string> rosterEmails = config.HockeyTeams
				.SelectMany(t => t.Players)
				.Select(p => p.PersonEmail)
				.Where(e => !string.IsNullOrWhiteSpace(e))
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			List<PersonSeed> rosterPersonSeeds = config.PlayerPersons
				.Concat(config.GoaliePersons)
				.Where(p => p.ContactInfo?.Email != null && rosterEmails.Contains(p.ContactInfo.Email))
				.GroupBy(p => p.ContactInfo!.Email!, StringComparer.OrdinalIgnoreCase)
				.Select(g => g.First())
				.ToList();

			if (rosterPersonSeeds.Count == 0 && rosterEmails.Count > 0)
			{
				throw new InvalidOperationException(
					"HockeyTeams roster emails were not found in PlayerPersons/GoaliePersons. Add matching PersonSeed entries or reuse floorball person emails.");
			}

			(List<PersonDto> hockeyPersons, Dictionary<string, Guid> personEmailMap) =
				await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, rosterPersonSeeds);
			hockeyEmailToPersonId = personEmailMap;

			(result.Players, hockeyEmailToPlayerId) = await HockeyPlayersSeeder.SeedAsync(
				http, jsonOptions, hockeyPersons, hockeyEmailToPersonId, config.HockeyTeams);

			HashSet<string> staffEmails = config.HockeyTeams
				.Select(t => t.StaffPersonEmail)
				.Where(e => !string.IsNullOrWhiteSpace(e))
				.ToHashSet(StringComparer.OrdinalIgnoreCase)!;
			List<PersonSeed> staffPersonSeeds = config.StaffPersons
				.Where(p => p.ContactInfo?.Email != null && staffEmails.Contains(p.ContactInfo.Email))
				.GroupBy(p => p.ContactInfo!.Email!, StringComparer.OrdinalIgnoreCase)
				.Select(g => g.First())
				.ToList();
			if (staffPersonSeeds.Count > 0)
			{
				(_, Dictionary<string, Guid> staffEmailMap) =
					await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, staffPersonSeeds);
				foreach (KeyValuePair<string, Guid> pair in staffEmailMap)
				{
					hockeyEmailToPersonId[pair.Key] = pair.Value;
				}
			}

			List<PersonSeed> officialPersonSeeds = config.RefereePersons.Take(4).ToList();
			if (officialPersonSeeds.Count > 0)
			{
				(_, Dictionary<string, Guid> officialEmailMap) =
					await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, officialPersonSeeds);
				foreach (KeyValuePair<string, Guid> pair in officialEmailMap)
				{
					hockeyEmailToPersonId[pair.Key] = pair.Value;
				}

				List<Guid> officialPersonIds = officialPersonSeeds
					.Select(p => p.ContactInfo?.Email)
					.Where(e => !string.IsNullOrWhiteSpace(e) && officialEmailMap.ContainsKey(e!))
					.Select(e => officialEmailMap[e!])
					.ToList();
				result.Officials = await HockeyOfficialsSeeder.SeedAsync(http, jsonOptions, officialPersonIds);
			}
		}

		if (scope.HasFlag(SeedScope.HockeyTeams))
		{
			result.Teams = await HockeyTeamsSeeder.SeedTeamsAsync(http, jsonOptions, config.HockeyTeams, divisions, clubs);
		}

		if (scope.HasFlag(SeedScope.HockeySeasons))
		{
			result.Seasons = await HockeySeasonsSeeder.SeedAsync(http, jsonOptions, config.HockeySeasons, divisions);
			await HockeyTeamsSeeder.AssignTeamsToSeasonsAsync(
				http, jsonOptions, result.Seasons, config.HockeyTeams, result.Teams, divisions);
		}

		if (scope.HasFlag(SeedScope.HockeyTeams))
		{
			foreach (HockeyTeamSeed teamSeed in config.HockeyTeams)
			{
				HockeyTeamDto? team = result.Teams.FirstOrDefault(t => string.Equals(t.Name, teamSeed.Name, StringComparison.OrdinalIgnoreCase));
				if (team != null)
				{
					await HockeyTeamsSeeder.AddPlayersAsync(http, jsonOptions, team.Id, teamSeed.Players, hockeyEmailToPlayerId);
				}
			}

			await HockeyTeamsSeeder.SeedLinesAndStaffAsync(
				http, jsonOptions, config.HockeyTeams, result.Teams, hockeyEmailToPersonId);
		}

		if (scope.HasFlag(SeedScope.HockeySeasonMatches))
		{
			result.Matches = await HockeyMatchesSeeder.SeedAsync(
				http, jsonOptions, config.HockeyMatches, result.Seasons, result.Teams, result.Officials);
		}

		if (scope.HasFlag(SeedScope.HockeyTournaments))
		{
			result.Tournaments = await HockeyTournamentsSeeder.SeedAsync(
				http, jsonOptions, config.HockeyTournaments, result.Teams);
			result.TournamentMatchesCreated = await HockeyTournamentMatchesSeeder.SeedAsync(
				http, jsonOptions, result.Tournaments, result.Teams, config.HockeyTournaments, result.Officials);
		}

		return result;
	}

	private static SeedSport ParseSportOrExit(string[] args)
	{
		string? cliValue = TryGetNamedArg(args, "--sport");
		if (cliValue == null)
		{
			Console.WriteLine("Using default sport: floorball");
			return SeedSport.Floorball;
		}

		if (string.Equals(cliValue, "floorball", StringComparison.OrdinalIgnoreCase))
		{
			Console.WriteLine("Using --sport from command line: floorball");
			return SeedSport.Floorball;
		}
		if (string.Equals(cliValue, "football", StringComparison.OrdinalIgnoreCase))
		{
			Console.WriteLine("Using --sport from command line: football");
			return SeedSport.Football;
		}
		if (string.Equals(cliValue, "all", StringComparison.OrdinalIgnoreCase))
		{
			Console.WriteLine("Using --sport from command line: all");
			return SeedSport.All;
		}

		Console.Error.WriteLine($"Invalid --sport value: '{cliValue}'. Valid values: floorball, football, all.");
		Environment.Exit(2);
		return SeedSport.Floorball;
	}

	private static string? TryGetNamedArg(string[] args, string name)
	{
		string prefix = name + "=";
		for (int i = 0; i < args.Length; i++)
		{
			string a = args[i];
			if (a.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				return a.Substring(prefix.Length);
			}
			if (string.Equals(a, name, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
			{
				return args[i + 1];
			}
		}
		return null;
	}

	private static void WriteCombinedSummary(
		SeedScope scope,
		bool seedFloorball,
		bool seedFootball,
		bool seedHockey,
		FloorballSeedResult? floorball,
		FootballSeedResult? football,
		HockeySeedResult? hockey)
	{
		Console.WriteLine("\nSummary:");

		int persons = (floorball?.Persons.Count ?? 0) + (football?.Persons.Count ?? 0);
		int clubs = (floorball?.Clubs.Count ?? 0) + (football?.Clubs.Count ?? 0);
		int divisions = (floorball?.Divisions.Count ?? 0) + (football?.Divisions.Count ?? 0);
		WriteSummaryLine("Persons created:", scope.HasFlag(SeedScope.Persons), persons);
		WriteSummaryLine("Clubs created:", scope.HasFlag(SeedScope.Clubs), clubs);
		WriteSummaryLine("Divisions created:", scope.HasFlag(SeedScope.Divisions), divisions);

		if (seedFloorball)
		{
			WriteSummaryLine("Floorball players created:", scope.HasFlag(SeedScope.PlayersReferees), floorball?.Players.Count ?? 0);
			WriteSummaryLine("Floorball referees created:", scope.HasFlag(SeedScope.PlayersReferees), floorball?.Referees.Count ?? 0);
			WriteSummaryLine("Seasons created:", scope.HasFlag(SeedScope.Seasons), floorball?.Seasons.Count ?? 0);
			WriteSummaryLine("Teams created:", scope.HasFlag(SeedScope.Teams), floorball?.Teams.Count ?? 0);
			WriteSummaryLine("Matches created:", scope.HasFlag(SeedScope.SeasonMatches), floorball?.Matches.Count ?? 0);
			WriteSummaryLine("Tournaments created:", scope.HasFlag(SeedScope.Tournaments), floorball?.Tournaments.Count ?? 0);
			WriteSummaryLine("Tournament matches created:", scope.HasFlag(SeedScope.Tournaments), floorball?.TournamentMatchesCreated ?? 0);
		}

		if (seedFootball)
		{
			WriteSummaryLine("Football players created:", scope.HasFlag(SeedScope.PlayersReferees), football?.Players.Count ?? 0);
			WriteSummaryLine("Football referees created:", scope.HasFlag(SeedScope.PlayersReferees), football?.Referees.Count ?? 0);
			WriteSummaryLine("Football seasons created:", scope.HasFlag(SeedScope.Seasons), football?.Seasons.Count ?? 0);
			WriteSummaryLine("Football teams created:", scope.HasFlag(SeedScope.Teams), football?.Teams.Count ?? 0);
			WriteSummaryLine("Football matches created:", scope.HasFlag(SeedScope.SeasonMatches), football?.Matches.Count ?? 0);
			WriteSummaryLine("Football tournaments created:", scope.HasFlag(SeedScope.Tournaments), football?.Tournaments.Count ?? 0);
			WriteSummaryLine("Football tournament matches:", scope.HasFlag(SeedScope.Tournaments), football?.TournamentMatchesCreated ?? 0);
		}

		if (seedHockey)
		{
			WriteSummaryLine("Hockey players created:", scope.HasFlag(SeedScope.HockeyPlayers), hockey?.Players.Count ?? 0);
			WriteSummaryLine("Hockey officials created:", scope.HasFlag(SeedScope.HockeyPlayers), hockey?.Officials.Count ?? 0);
			WriteSummaryLine("Hockey teams created:", scope.HasFlag(SeedScope.HockeyTeams), hockey?.Teams.Count ?? 0);
			WriteSummaryLine("Hockey seasons created:", scope.HasFlag(SeedScope.HockeySeasons), hockey?.Seasons.Count ?? 0);
			WriteSummaryLine("Hockey matches created:", scope.HasFlag(SeedScope.HockeySeasonMatches), hockey?.Matches.Count ?? 0);
			WriteSummaryLine("Hockey tournaments created:", scope.HasFlag(SeedScope.HockeyTournaments), hockey?.Tournaments.Count ?? 0);
			WriteSummaryLine("Hockey tournament matches:", scope.HasFlag(SeedScope.HockeyTournaments), hockey?.TournamentMatchesCreated ?? 0);
		}
	}

	private sealed class FloorballSeedResult
	{
		public List<PersonDto> Persons { get; set; } = new List<PersonDto>();
		public List<ClubDto> Clubs { get; set; } = new List<ClubDto>();
		public List<DivisionDto> Divisions { get; set; } = new List<DivisionDto>();
		public List<FloorballPlayerDto> Players { get; set; } = new List<FloorballPlayerDto>();
		public List<FloorballRefereeDto> Referees { get; set; } = new List<FloorballRefereeDto>();
		public List<FloorballSeasonDto> Seasons { get; set; } = new List<FloorballSeasonDto>();
		public List<FloorballTeamDto> Teams { get; set; } = new List<FloorballTeamDto>();
		public List<FloorballMatchDto> Matches { get; set; } = new List<FloorballMatchDto>();
		public List<FloorballTournamentDto> Tournaments { get; set; } = new List<FloorballTournamentDto>();
		public int TournamentMatchesCreated { get; set; }
	}

	private sealed class FootballSeedResult
	{
		public List<PersonDto> Persons { get; set; } = new List<PersonDto>();
		public List<ClubDto> Clubs { get; set; } = new List<ClubDto>();
		public List<DivisionDto> Divisions { get; set; } = new List<DivisionDto>();
		public List<FootballPlayerDto> Players { get; set; } = new List<FootballPlayerDto>();
		public List<FootballRefereeDto> Referees { get; set; } = new List<FootballRefereeDto>();
		public List<FootballSeasonDto> Seasons { get; set; } = new List<FootballSeasonDto>();
		public List<FootballTeamDto> Teams { get; set; } = new List<FootballTeamDto>();
		public List<FootballMatchDto> Matches { get; set; } = new List<FootballMatchDto>();
		public List<FootballTournamentDto> Tournaments { get; set; } = new List<FootballTournamentDto>();
		public int TournamentMatchesCreated { get; set; }
	}

	private sealed class HockeySeedResult
	{
		public List<HockeyPlayerDto> Players { get; set; } = new List<HockeyPlayerDto>();
		public List<HockeyOfficialDto> Officials { get; set; } = new List<HockeyOfficialDto>();
		public List<HockeyTeamDto> Teams { get; set; } = new List<HockeyTeamDto>();
		public List<HockeySeasonDto> Seasons { get; set; } = new List<HockeySeasonDto>();
		public List<HockeyMatchDto> Matches { get; set; } = new List<HockeyMatchDto>();
		public List<HockeyTournamentDto> Tournaments { get; set; } = new List<HockeyTournamentDto>();
		public int TournamentMatchesCreated { get; set; }
	}

	private static void WriteSummaryLine(string label, bool ran, int value)
	{
		string display = ran ? value.ToString() : "(skipped)";
		Console.WriteLine($"  {label,-32}{display}");
	}
}
