using Application.Features.Common.Divisions.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Hockey.Seasons.DTOs;
using JoomleagueImporter.Import;
using JoomleagueImporter.Models;
using Microsoft.Extensions.Configuration;

namespace JoomleagueImporter;

public static class Program
{
    private const string SportFloorball = "floorball";
    private const string SportFootball = "football";
    private const string SportHockey = "hockey";

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("==========================================================");
        Console.WriteLine("  JoomLeague SQL Dump Importer");
        Console.WriteLine("  Imports floorball, football, or hockey data from a JoomLeague dump");
        Console.WriteLine("==========================================================\n");

        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        string sport = ResolveSport(args, config);
        bool isFootball = string.Equals(sport, SportFootball, StringComparison.OrdinalIgnoreCase);
        bool isHockey = string.Equals(sport, SportHockey, StringComparison.OrdinalIgnoreCase);

        string dumpPath = config["JoomleagueImporter:DumpFilePath"] ?? "";
        string includeFilter;
        string? excludeFilter;
        if (isHockey)
        {
            includeFilter = config["JoomleagueImporter:Hockey:ProjectNameFilter"] ?? "jääkiekko|jaakiekko|hockey";
            excludeFilter = config["JoomleagueImporter:Hockey:ProjectNameExcludeFilter"] ?? "manager|jääpallo|jaapallo|kaukalo|nhl";
        }
        else if (isFootball)
        {
            includeFilter = config["JoomleagueImporter:Football:ProjectNameFilter"] ?? "jalkapallo|football|futis";
            excludeFilter = config["JoomleagueImporter:Football:ProjectNameExcludeFilter"] ?? "manager";
        }
        else
        {
            includeFilter = config["JoomleagueImporter:ProjectNameFilter"] ?? "salibandy|sähly";
            excludeFilter = config["JoomleagueImporter:ProjectNameExcludeFilter"];
        }
        string? projectIdFilter = GetArg(args, "project-id")
            ?? config["JoomleagueImporter:ProjectIdFilter"];
        bool dryRun = bool.TryParse(config["JoomleagueImporter:DryRun"], out bool dr) && dr;
        bool fillUnknownGoals = !bool.TryParse(config["JoomleagueImporter:FillUnknownGoals"], out bool fug) || fug;

        HashSet<int> repairMatchIds = (config["JoomleagueImporter:RepairMatches"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out int id) ? id : -1)
            .Where(id => id > 0)
            .ToHashSet();
        string? repairMatchesArg = GetArg(args, "repair-matches");
        if (!string.IsNullOrWhiteSpace(repairMatchesArg))
        {
            foreach (string part in repairMatchesArg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (int.TryParse(part, out int repairId) && repairId > 0)
                {
                    repairMatchIds.Add(repairId);
                }
            }
        }

        bool repairAll = bool.TryParse(config["JoomleagueImporter:RepairAll"], out bool ra) && ra;

        if (args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase))
            dryRun = true;
        if (args.Contains("--repair-all", StringComparer.OrdinalIgnoreCase))
            repairAll = true;
        bool autoConfirm = args.Contains("--yes", StringComparer.OrdinalIgnoreCase)
            || args.Contains("-y", StringComparer.OrdinalIgnoreCase);

        string? concurrencyArg = GetArg(args, "concurrency");
        if (int.TryParse(concurrencyArg, out int concurrency) && concurrency > 0)
            MatchImportParallel.Degree = concurrency;

        string? cliDump = GetArg(args, "dump");
        if (!string.IsNullOrWhiteSpace(cliDump))
            dumpPath = cliDump;

        if (string.IsNullOrWhiteSpace(dumpPath) || !File.Exists(dumpPath))
        {
            Console.Error.WriteLine($"Dump file not found: '{dumpPath}'. Set JoomleagueImporter:DumpFilePath or pass --dump=...");
            return 1;
        }

        Console.WriteLine($"Sport: {sport}");
        Console.WriteLine($"Parsing dump: {dumpPath}");
        DateTime parseStart = DateTime.Now;
        JoomleagueDatabase db = JoomleagueDatabase.Load(dumpPath);
        Console.WriteLine($"Parsed in {(DateTime.Now - parseStart).TotalSeconds:F1}s: " +
                          $"{db.Clubs.Count} clubs, {db.Teams.Count} teams, {db.Persons.Count} persons, " +
                          $"{db.Projects.Count} projects, {db.Matches.Count} matches, {db.MatchEvents.Count} match events.\n");

        FloorballImportSet set = db.BuildImportSet(includeFilter, excludeFilter, projectIdFilter);

        Console.WriteLine($"Selected {set.Projects.Count} {sport} projects " +
                          $"(filter: '{includeFilter}', exclude: '{excludeFilter}'):\n");
        Console.WriteLine($"  {"ID",4}  {"Project",-44} {"Cat",6} {"Teams",5} {"Match",6} {"Event",6}");
        foreach (ProjectImport pi in set.Projects)
        {
            string category = TeamCategoryResolver.InferFromName(pi.Project.Name).ToString();
            Console.WriteLine($"  {pi.Project.Id,4}  {Truncate(pi.Project.Name, 44),-44} {category,6} " +
                              $"{pi.Teams.Count,5} {pi.Matches.Count,6} {pi.Matches.Sum(m => m.Events.Count),6}");
        }
        Console.WriteLine();
        Console.WriteLine($"Totals: {set.UniqueTeams.Count} unique teams, {set.UniquePersons.Count} unique persons, " +
                          $"{set.TotalMatches} matches, {set.TotalEvents} events.");
        if (set.SkippedEmptyRosterTeams > 0)
            Console.WriteLine($"Skipped {set.SkippedEmptyRosterTeams} team(s) with 0 importable roster players.\n");
        else
            Console.WriteLine();

        if (dryRun)
        {
            Console.WriteLine("Dry run - nothing was imported.");
            return 0;
        }

        if (set.Projects.Count == 0)
        {
            Console.WriteLine("Nothing to import.");
            return 0;
        }

        string? cliApiUrl = GetArg(args, "api-url");
        string configuredUrl = cliApiUrl
            ?? config["JoomleagueImporter:ApiBaseUrl"]
            ?? "http://localhost:8080/";
        string apiBaseUrl = autoConfirm || !string.IsNullOrWhiteSpace(cliApiUrl)
            ? NormalizeApiUrl(configuredUrl)
            : PromptForApiUrl(configuredUrl);

        string? accessToken = GetArg(args, "access-token")
            ?? GetArg(args, "token")
            ?? config["JoomleagueImporter:AccessToken"];
        string? refreshToken = GetArg(args, "refresh-token")
            ?? config["JoomleagueImporter:RefreshToken"];
        bool useProvidedToken = !string.IsNullOrWhiteSpace(accessToken) || !string.IsNullOrWhiteSpace(refreshToken);

        string? loginEmail = null;
        if (!useProvidedToken)
        {
            loginEmail = autoConfirm
                ? ResolveLoginEmailNonInteractive(config)
                : ResolveLoginEmail(config);
        }

        if (!autoConfirm)
        {
            Console.Write("Start import? [y/N]: ");
            string? confirm = Console.ReadLine()?.Trim();
            if (!string.Equals(confirm, "y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Aborted.");
                return 0;
            }
        }
        else
        {
            Console.WriteLine($"Using API: {apiBaseUrl}");
            Console.WriteLine(useProvidedToken
                ? "Auth: provided access/refresh token"
                : $"Login email: {loginEmail}");
            Console.WriteLine($"Match concurrency: {MatchImportParallel.Degree}");
            Console.WriteLine("Starting import (--yes).");
        }
        Console.WriteLine();

        string idMapPath = ResolveIdMapPath(config, GetArg(args, "id-map"), apiBaseUrl, sport, isFootball, isHockey);
        IdMapStore idMap = IdMapStore.LoadOrCreate(idMapPath);
        Console.WriteLine($"Id map: {idMapPath} " +
                          $"({idMap.Persons.Count} persons, {idMap.Teams.Count} teams, {idMap.ProcessedMatches.Count} matches already imported)\n");

        string logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
        using ImportLogger log = new(logDir);

        try
        {
            if (isHockey)
            {
                using HockeyApiClient api = new(apiBaseUrl);
                await AuthenticateClientAsync(api, accessToken, refreshToken, loginEmail);
                return await RunHockeyImportAsync(api, idMap, log, db, set, fillUnknownGoals, repairMatchIds, repairAll);
            }

            if (isFootball)
            {
                using FootballApiClient api = new(apiBaseUrl);
                await AuthenticateClientAsync(api, accessToken, refreshToken, loginEmail);
                return await RunFootballImportAsync(api, idMap, log, db, set, fillUnknownGoals, repairMatchIds, repairAll);
            }

            using FloorballApiClient floorballApi = new(apiBaseUrl);
            await AuthenticateClientAsync(floorballApi, accessToken, refreshToken, loginEmail);
            return await RunFloorballImportAsync(floorballApi, idMap, log, db, set, fillUnknownGoals, repairMatchIds, repairAll);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\nFATAL: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private static async Task<int> RunFloorballImportAsync(
        FloorballApiClient api,
        IdMapStore idMap,
        ImportLogger log,
        JoomleagueDatabase db,
        FloorballImportSet set,
        bool fillUnknownGoals,
        HashSet<int> repairMatchIds,
        bool repairAll)
    {
        FloorballEntityImporter entities = new(api, idMap, log);

        DivisionDto division = await entities.GetOrCreateImportDivisionAsync();
        await entities.ImportClubsAsync(set, db);
        await entities.ImportPersonsAndPlayersAsync(set);
        await entities.ImportTeamsAsync(set, db, division);
        Guid refereeId = await entities.GetOrCreateImportRefereeAsync();

        FloorballMatchImporter matches = new(api, idMap, log, entities, db, fillUnknownGoals, repairMatchIds, repairAll);
        PrintRepairMode(repairAll, repairMatchIds);

        foreach (ProjectImport pi in set.Projects)
        {
            Console.WriteLine($"\n=== {pi.Project.Name} (JL project {pi.Project.Id}, {pi.Matches.Count} matches) ===");
            FloorballSeasonDto? season = await entities.ImportSeasonAsync(pi, division);
            if (season == null)
            {
                Console.WriteLine("  SKIP: season could not be created.");
                continue;
            }
            await matches.ImportProjectMatchesAsync(pi, season, refereeId);
        }

        PrintImportComplete(matches.Succeeded, matches.ScheduledOnly, matches.Skipped, matches.Repaired, matches.Failed, log);
        return 0;
    }

    private static async Task<int> RunFootballImportAsync(
        FootballApiClient api,
        IdMapStore idMap,
        ImportLogger log,
        JoomleagueDatabase db,
        FloorballImportSet set,
        bool fillUnknownGoals,
        HashSet<int> repairMatchIds,
        bool repairAll)
    {
        FootballEntityImporter entities = new(api, idMap, log);

        DivisionDto division = await entities.GetOrCreateImportDivisionAsync();
        await entities.ImportClubsAsync(set, db);
        await entities.ImportPersonsAndPlayersAsync(set);
        await entities.ImportTeamsAsync(set, db, division);
        Guid refereeId = await entities.GetOrCreateImportRefereeAsync();

        FootballMatchImporter matches = new(api, idMap, log, entities, db, fillUnknownGoals, repairMatchIds, repairAll);
        PrintRepairMode(repairAll, repairMatchIds);

        foreach (ProjectImport pi in set.Projects)
        {
            Console.WriteLine($"\n=== {pi.Project.Name} (JL project {pi.Project.Id}, {pi.Matches.Count} matches) ===");
            FootballSeasonDto? season = await entities.ImportSeasonAsync(pi, division);
            if (season == null)
            {
                Console.WriteLine("  SKIP: season could not be created.");
                continue;
            }
            await matches.ImportProjectMatchesAsync(pi, season, refereeId);
        }

        PrintImportComplete(matches.Succeeded, matches.ScheduledOnly, matches.Skipped, matches.Repaired, matches.Failed, log);
        return 0;
    }

    private static async Task<int> RunHockeyImportAsync(
        HockeyApiClient api,
        IdMapStore idMap,
        ImportLogger log,
        JoomleagueDatabase db,
        FloorballImportSet set,
        bool fillUnknownGoals,
        HashSet<int> repairMatchIds,
        bool repairAll)
    {
        HockeyEntityImporter entities = new(api, idMap, log);

        DivisionDto division = await entities.GetOrCreateImportDivisionAsync();
        await entities.ImportClubsAsync(set, db);
        await entities.ImportPersonsAndPlayersAsync(set);
        await entities.ImportTeamsAsync(set, db, division);
        Guid officialId = await entities.GetOrCreateImportOfficialAsync();

        HockeyMatchImporter matches = new(api, idMap, log, entities, db, fillUnknownGoals, repairMatchIds, repairAll);
        PrintRepairMode(repairAll, repairMatchIds);

        foreach (ProjectImport pi in set.Projects)
        {
            Console.WriteLine($"\n=== {pi.Project.Name} (JL project {pi.Project.Id}, {pi.Matches.Count} matches) ===");
            HockeySeasonDto? season = await entities.ImportSeasonAsync(pi, division);
            if (season == null)
            {
                Console.WriteLine("  SKIP: season could not be created.");
                continue;
            }
            await matches.ImportProjectMatchesAsync(pi, season, officialId);
            await api.RecalculateCompetitionAsync(season.Id);
        }

        PrintImportComplete(matches.Succeeded, matches.ScheduledOnly, matches.Skipped, matches.Repaired, matches.Failed, log);
        return 0;
    }

    private static void PrintRepairMode(bool repairAll, HashSet<int> repairMatchIds)
    {
        if (repairAll)
            Console.WriteLine("Repair mode: re-importing events for ALL previously processed matches.");
        else if (repairMatchIds.Count > 0)
            Console.WriteLine($"Repair mode: re-importing events for {repairMatchIds.Count} match(es): {string.Join(", ", repairMatchIds)}");
    }

    private static void PrintImportComplete(
        int succeeded,
        int scheduledOnly,
        int skipped,
        int repaired,
        int failed,
        ImportLogger log)
    {
        Console.WriteLine("\n==========================================================");
        Console.WriteLine("  Import complete!");
        Console.WriteLine($"  Matches: {succeeded} completed, {scheduledOnly} scheduled-only, " +
                          $"{skipped} skipped, {repaired} repaired, {failed} failed.");
        if (log.ErrorCount > 0)
            Console.WriteLine($"  {log.ErrorCount} errors logged to: {log.LogPath}");
        Console.WriteLine("==========================================================");
    }

    private static string ResolveSport(string[] args, IConfiguration config)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--sport", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                return NormalizeSport(args[i + 1]);
            if (args[i].StartsWith("--sport=", StringComparison.OrdinalIgnoreCase))
                return NormalizeSport(args[i]["--sport=".Length..]);
        }

        return NormalizeSport(config["JoomleagueImporter:Sport"]);
    }

    private static string NormalizeSport(string? value)
    {
        if (string.Equals(value, SportFootball, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "jalkapallo", StringComparison.OrdinalIgnoreCase))
            return SportFootball;
        if (string.Equals(value, SportHockey, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "jääkiekko", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "jaakiekko", StringComparison.OrdinalIgnoreCase))
            return SportHockey;
        return SportFloorball;
    }

    private static async Task AuthenticateClientAsync(
        ImportApiClient api,
        string? accessToken,
        string? refreshToken,
        string? loginEmail)
    {
        if (!string.IsNullOrWhiteSpace(accessToken) || !string.IsNullOrWhiteSpace(refreshToken))
        {
            await api.AuthenticateWithTokensAsync(accessToken, refreshToken);
            return;
        }

        await api.AuthenticateAsync(loginEmail);
    }

    private static string? GetArg(string[] args, string name)
    {
        string prefix = $"--{name}=";
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return args[i][prefix.Length..];
            if (string.Equals(args[i], $"--{name}", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                return args[i + 1];
        }

        return null;
    }

    private static string ResolveIdMapPath(
        IConfiguration config,
        string? cliPath,
        string apiBaseUrl,
        string sport,
        bool isFootball,
        bool isHockey)
    {
        if (!string.IsNullOrWhiteSpace(cliPath))
            return cliPath;

        if (isHockey)
        {
            string? hockeyPath = config["JoomleagueImporter:Hockey:IdMapPath"];
            if (!string.IsNullOrWhiteSpace(hockeyPath))
                return hockeyPath;
        }
        else if (isFootball)
        {
            string? footballPath = config["JoomleagueImporter:Football:IdMapPath"];
            if (!string.IsNullOrWhiteSpace(footballPath))
                return footballPath;
        }
        else if (config["JoomleagueImporter:IdMapPath"] is { Length: > 0 } configured)
        {
            return configured;
        }

        Uri host = new(NormalizeApiUrl(apiBaseUrl));
        bool isLocal = string.Equals(host.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase);
        if (isLocal)
        {
            if (isHockey)
                return Path.Combine(AppContext.BaseDirectory, "id-map-hockey.json");
            if (isFootball)
                return Path.Combine(AppContext.BaseDirectory, "id-map-football.json");
            return Path.Combine(AppContext.BaseDirectory, "id-map.json");
        }

        string safeHost = host.Host.Replace('.', '-');
        return Path.Combine(AppContext.BaseDirectory, $"id-map-{safeHost}-{sport}.json");
    }

    private static string PromptForApiUrl(string defaultUrl)
    {
        Console.Write($"API base URL [{defaultUrl}]: ");
        string? input = Console.ReadLine()?.Trim();
        string url = string.IsNullOrWhiteSpace(input) ? defaultUrl : input;
        url = NormalizeApiUrl(url);
        Console.WriteLine($"Using API: {url}");
        return url;
    }

    private static string NormalizeApiUrl(string url)
    {
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            url = "http://" + url;
        if (!url.EndsWith('/'))
            url += "/";
        return url;
    }

    private static string ResolveLoginEmailNonInteractive(IConfiguration config)
    {
        const string defaultEmail = "test@myleague.local";
        string? configEmail = config["JoomleagueImporter:LoginEmail"]?.Trim();
        return string.IsNullOrWhiteSpace(configEmail) ? defaultEmail : configEmail;
    }

    private static string ResolveLoginEmail(IConfiguration config)
    {
        string promptDefault = ResolveLoginEmailNonInteractive(config);

        Console.Write($"Login email [{promptDefault}]: ");
        string? input = Console.ReadLine()?.Trim();
        return string.IsNullOrWhiteSpace(input) ? promptDefault : input;
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..(max - 3)] + "...";
}
