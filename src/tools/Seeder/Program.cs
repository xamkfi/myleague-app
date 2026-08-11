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
using Application.Features.Hockey.Players.DTOs;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Tournaments.DTOs;
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

		SeedScope requested = PromptForScope(args);
		SeedScope scope = SeedScopeResolver.Resolve(requested);
		PrintEffectiveScope(requested, scope);

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

			List<PersonDto> basePersons = scope.HasFlag(SeedScope.Persons)
				? await PersonsSeeder.SeedAsync(http, jsonOptions, config)
				: new List<PersonDto>();

			List<ClubDto> clubResults = scope.HasFlag(SeedScope.Clubs)
				? await ClubsSeeder.SeedAsync(http, jsonOptions, config)
				: new List<ClubDto>();

			List<DivisionDto> divisionResults = scope.HasFlag(SeedScope.Divisions)
				? await DivisionsSeeder.SeedAsync(http, jsonOptions, config)
				: new List<DivisionDto>();

			List<PersonDto> playerPersons = new List<PersonDto>();
			List<PersonDto> goaliePersons = new List<PersonDto>();
			List<PersonDto> refereePersons = new List<PersonDto>();
			Dictionary<string, Guid> playerEmailToPersonId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, Guid> goalieEmailToPersonId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, Guid> refereeEmailToPersonId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

			List<FloorballPlayerDto> players = new List<FloorballPlayerDto>();
			Dictionary<string, Guid> emailToPlayerId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
			List<FloorballRefereeDto> referees = new List<FloorballRefereeDto>();

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

				(players, emailToPlayerId) = await FloorballPlayersSeeder.SeedAsync(http, jsonOptions, playerPersons, goaliePersons, seedEmailToPersonId);
				referees = await FloorballRefereesSeeder.SeedAsync(http, jsonOptions, refereePersons.Select(p => p.Id).ToList());
			}

			List<FloorballSeasonDto> seasons = new List<FloorballSeasonDto>();
			List<FloorballTeamDto> teams = new List<FloorballTeamDto>();

			if (scope.HasFlag(SeedScope.Teams))
			{
				teams = await FloorballTeamsSeeder.SeedTeamsAsync(http, jsonOptions, config.FloorballTeams, divisionResults, clubResults);
			}

			if (scope.HasFlag(SeedScope.Seasons))
			{
				seasons = await FloorballSeasonsSeeder.SeedAsync(http, jsonOptions, config.FloorballSeasons, divisionResults);
				await FloorballTeamsSeeder.AssignTeamsToSeasonsAsync(http, jsonOptions, seasons, config.FloorballTeams, teams, divisionResults);
			}

			if (scope.HasFlag(SeedScope.Teams))
			{
				foreach (FloorballTeamSeed teamSeed in config.FloorballTeams)
				{
					FloorballTeamDto? team = teams.FirstOrDefault(t => string.Equals(t.Name, teamSeed.Name, StringComparison.OrdinalIgnoreCase));
					if (team != null)
					{
						await FloorballTeamsSeeder.AddPlayersAsync(http, jsonOptions, team.Id, teamSeed.Players, emailToPlayerId);
					}
				}
			}

			List<FloorballRefereeDto> allReferees = new List<FloorballRefereeDto>();
			List<FloorballMatchDto> matches = new List<FloorballMatchDto>();
			List<FloorballTournamentDto> tournaments = new List<FloorballTournamentDto>();
			int tournamentMatchesCreated = 0;

			bool needsAllReferees = scope.HasFlag(SeedScope.SeasonMatches) || scope.HasFlag(SeedScope.Tournaments);
			if (needsAllReferees)
			{
				allReferees = await FloorballMatchesSeeder.FetchAllRefereesFromApiAsync(http, jsonOptions);
			}

			if (scope.HasFlag(SeedScope.SeasonMatches))
			{
				Dictionary<string, Guid> emailToRefereeId = FloorballMatchesSeeder.BuildEmailToRefereeIdMap(allReferees, refereeEmailToPersonId);
				matches = await FloorballMatchesSeeder.SeedAsync(http, jsonOptions, config.FloorballMatches, seasons, teams, referees, emailToRefereeId);
			}

			if (scope.HasFlag(SeedScope.Tournaments))
			{
				tournaments = await FloorballTournamentsSeeder.SeedAsync(http, jsonOptions, config.FloorballTournaments, teams);

				List<FloorballRefereeDto> tournamentReferees = referees.Concat(allReferees)
					.GroupBy(r => r.Id)
					.Select(g => g.First())
					.ToList();

				if (tournamentReferees.Count == 0 && refereePersons.Count > 0)
				{
					Console.Error.WriteLine(
						$"WARNING: tournament match seeding has no referees available even though {refereePersons.Count} referee person(s) were configured. " +
						"Matches will be created without an assigned referee. See earlier WARNING lines for the underlying API response.");
				}
				tournamentMatchesCreated = await FloorballTournamentMatchesSeeder.SeedAsync(http, jsonOptions, tournaments, tournamentReferees, config.FloorballTournaments);
			}

			// --- Hockey phases (after Floorball; independent except shared Persons/Clubs/Divisions) ---
			List<HockeyPlayerDto> hockeyPlayers = new List<HockeyPlayerDto>();
			Dictionary<string, Guid> hockeyEmailToPlayerId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
			List<HockeyTeamDto> hockeyTeams = new List<HockeyTeamDto>();
			List<HockeySeasonDto> hockeySeasons = new List<HockeySeasonDto>();
			List<HockeyMatchDto> hockeyMatches = new List<HockeyMatchDto>();
			List<HockeyTournamentDto> hockeyTournaments = new List<HockeyTournamentDto>();
			int hockeyTournamentMatchesCreated = 0;

			if (scope.HasFlag(SeedScope.HockeyPlayers))
			{
				HashSet<string> hockeyEmails = config.HockeyTeams
					.SelectMany(t => t.Players)
					.Select(p => p.PersonEmail)
					.Where(e => !string.IsNullOrWhiteSpace(e))
					.ToHashSet(StringComparer.OrdinalIgnoreCase);

				List<PersonSeed> hockeyPersonSeeds = config.PlayerPersons
					.Concat(config.GoaliePersons)
					.Where(p => p.ContactInfo?.Email != null && hockeyEmails.Contains(p.ContactInfo.Email))
					.GroupBy(p => p.ContactInfo!.Email!, StringComparer.OrdinalIgnoreCase)
					.Select(g => g.First())
					.ToList();

				if (hockeyPersonSeeds.Count == 0 && hockeyEmails.Count > 0)
				{
					throw new InvalidOperationException(
						"HockeyTeams roster emails were not found in PlayerPersons/GoaliePersons. Add matching PersonSeed entries or reuse floorball person emails.");
				}

				(List<PersonDto> hockeyPersons, Dictionary<string, Guid> hockeyEmailToPersonId) =
					await PersonsSeeder.SeedListWithEmailMapAsync(http, jsonOptions, hockeyPersonSeeds);

				(hockeyPlayers, hockeyEmailToPlayerId) = await HockeyPlayersSeeder.SeedAsync(
					http, jsonOptions, hockeyPersons, hockeyEmailToPersonId, config.HockeyTeams);
			}

			if (scope.HasFlag(SeedScope.HockeyTeams))
			{
				hockeyTeams = await HockeyTeamsSeeder.SeedTeamsAsync(http, jsonOptions, config.HockeyTeams, divisionResults, clubResults);
			}

			if (scope.HasFlag(SeedScope.HockeySeasons))
			{
				hockeySeasons = await HockeySeasonsSeeder.SeedAsync(http, jsonOptions, config.HockeySeasons, divisionResults);
				await HockeyTeamsSeeder.AssignTeamsToSeasonsAsync(
					http, jsonOptions, hockeySeasons, config.HockeyTeams, hockeyTeams, divisionResults);
			}

			if (scope.HasFlag(SeedScope.HockeyTeams))
			{
				foreach (HockeyTeamSeed teamSeed in config.HockeyTeams)
				{
					HockeyTeamDto? team = hockeyTeams.FirstOrDefault(t => string.Equals(t.Name, teamSeed.Name, StringComparison.OrdinalIgnoreCase));
					if (team != null)
					{
						await HockeyTeamsSeeder.AddPlayersAsync(http, jsonOptions, team.Id, teamSeed.Players, hockeyEmailToPlayerId);
					}
				}
			}

			if (scope.HasFlag(SeedScope.HockeySeasonMatches))
			{
				hockeyMatches = await HockeyMatchesSeeder.SeedAsync(
					http, jsonOptions, config.HockeyMatches, hockeySeasons, hockeyTeams);
			}

			if (scope.HasFlag(SeedScope.HockeyTournaments))
			{
				hockeyTournaments = await HockeyTournamentsSeeder.SeedAsync(
					http, jsonOptions, config.HockeyTournaments, hockeyTeams);
				hockeyTournamentMatchesCreated = await HockeyTournamentMatchesSeeder.SeedAsync(
					http, jsonOptions, hockeyTournaments, hockeyTeams, config.HockeyTournaments);
			}

			Console.WriteLine("\nSummary:");
			WriteSummaryLine("Persons created:", scope.HasFlag(SeedScope.Persons), basePersons.Count);
			WriteSummaryLine("Clubs created:", scope.HasFlag(SeedScope.Clubs), clubResults.Count);
			WriteSummaryLine("Divisions created:", scope.HasFlag(SeedScope.Divisions), divisionResults.Count);
			WriteSummaryLine("Floorball players created:", scope.HasFlag(SeedScope.PlayersReferees), players.Count);
			WriteSummaryLine("Floorball referees created:", scope.HasFlag(SeedScope.PlayersReferees), referees.Count);
			WriteSummaryLine("Seasons created:", scope.HasFlag(SeedScope.Seasons), seasons.Count);
			WriteSummaryLine("Teams created:", scope.HasFlag(SeedScope.Teams), teams.Count);
			WriteSummaryLine("Matches created:", scope.HasFlag(SeedScope.SeasonMatches), matches.Count);
			WriteSummaryLine("Tournaments created:", scope.HasFlag(SeedScope.Tournaments), tournaments.Count);
			WriteSummaryLine("Tournament matches created:", scope.HasFlag(SeedScope.Tournaments), tournamentMatchesCreated);
			WriteSummaryLine("Hockey players created:", scope.HasFlag(SeedScope.HockeyPlayers), hockeyPlayers.Count);
			WriteSummaryLine("Hockey teams created:", scope.HasFlag(SeedScope.HockeyTeams), hockeyTeams.Count);
			WriteSummaryLine("Hockey seasons created:", scope.HasFlag(SeedScope.HockeySeasons), hockeySeasons.Count);
			WriteSummaryLine("Hockey matches created:", scope.HasFlag(SeedScope.HockeySeasonMatches), hockeyMatches.Count);
			WriteSummaryLine("Hockey tournaments created:", scope.HasFlag(SeedScope.HockeyTournaments), hockeyTournaments.Count);
			WriteSummaryLine("Hockey tournament matches:", scope.HasFlag(SeedScope.HockeyTournaments), hockeyTournamentMatchesCreated);

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

	private static void WriteSummaryLine(string label, bool ran, int value)
	{
		string display = ran ? value.ToString() : "(skipped)";
		Console.WriteLine($"  {label,-32}{display}");
	}
}
