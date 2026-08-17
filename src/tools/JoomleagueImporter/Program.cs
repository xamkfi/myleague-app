using Application.Features.Common.Divisions.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Football.Seasons.DTOs;
using JoomleagueImporter.Import;
using JoomleagueImporter.Models;
using Microsoft.Extensions.Configuration;

namespace JoomleagueImporter;

public static class Program
{
    private const string SportFloorball = "floorball";
    private const string SportFootball = "football";

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("==========================================================");
        Console.WriteLine("  JoomLeague SQL Dump Importer");
        Console.WriteLine("  Imports floorball or football data from a JoomLeague dump");
        Console.WriteLine("==========================================================\n");

        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        string sport = ResolveSport(args, config);
        bool isFootball = string.Equals(sport, SportFootball, StringComparison.OrdinalIgnoreCase);

        string dumpPath = config["JoomleagueImporter:DumpFilePath"] ?? "";
        string includeFilter = isFootball
            ? (config["JoomleagueImporter:Football:ProjectNameFilter"] ?? "jalkapallo|football|futis")
            : (config["JoomleagueImporter:ProjectNameFilter"] ?? "salibandy|sähly");
        string? excludeFilter = isFootball
            ? (config["JoomleagueImporter:Football:ProjectNameExcludeFilter"] ?? "manager")
            : config["JoomleagueImporter:ProjectNameExcludeFilter"];
        string? projectIdFilter = config["JoomleagueImporter:ProjectIdFilter"];
        bool dryRun = bool.TryParse(config["JoomleagueImporter:DryRun"], out bool dr) && dr;
        bool fillUnknownGoals = !bool.TryParse(config["JoomleagueImporter:FillUnknownGoals"], out bool fug) || fug;

        HashSet<int> repairMatchIds = (config["JoomleagueImporter:RepairMatches"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out int id) ? id : -1)
            .Where(id => id > 0)
            .ToHashSet();
        bool repairAll = bool.TryParse(config["JoomleagueImporter:RepairAll"], out bool ra) && ra;

        if (args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase))
            dryRun = true;
        if (args.Contains("--repair-all", StringComparer.OrdinalIgnoreCase))
            repairAll = true;

        if (string.IsNullOrWhiteSpace(dumpPath) || !File.Exists(dumpPath))
        {
            Console.Error.WriteLine($"Dump file not found: '{dumpPath}'. Set JoomleagueImporter:DumpFilePath in appsettings.json.");
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

        string apiBaseUrl = PromptForApiUrl(config["JoomleagueImporter:ApiBaseUrl"] ?? "http://localhost:8080/");
        string loginEmail = ResolveLoginEmail(config);

        Console.Write("Start import? [y/N]: ");
        string? confirm = Console.ReadLine()?.Trim();
        if (!string.Equals(confirm, "y", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Aborted.");
            return 0;
        }
        Console.WriteLine();

        string idMapPath = ResolveIdMapPath(config, isFootball);
        IdMapStore idMap = IdMapStore.LoadOrCreate(idMapPath);
        Console.WriteLine($"Id map: {idMapPath} " +
                          $"({idMap.Persons.Count} persons, {idMap.Teams.Count} teams, {idMap.ProcessedMatches.Count} matches already imported)\n");

        string logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
        using ImportLogger log = new(logDir);

        try
        {
            using ApiClient api = new(apiBaseUrl);
            await api.AuthenticateAsync(loginEmail);

            if (isFootball)
                return await RunFootballImportAsync(api, idMap, log, db, set, fillUnknownGoals, repairMatchIds, repairAll);

            return await RunFloorballImportAsync(api, idMap, log, db, set, fillUnknownGoals, repairMatchIds, repairAll);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\nFATAL: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private static async Task<int> RunFloorballImportAsync(
        ApiClient api,
        IdMapStore idMap,
        ImportLogger log,
        JoomleagueDatabase db,
        FloorballImportSet set,
        bool fillUnknownGoals,
        HashSet<int> repairMatchIds,
        bool repairAll)
    {
        EntityImporter entities = new(api, idMap, log);

        DivisionDto division = await entities.GetOrCreateImportDivisionAsync();
        await entities.ImportClubsAsync(set, db);
        await entities.ImportPersonsAndPlayersAsync(set);
        await entities.ImportTeamsAsync(set, db, division);
        Guid refereeId = await entities.GetOrCreateImportRefereeAsync();

        MatchImporter matches = new(api, idMap, log, entities, db, fillUnknownGoals, repairMatchIds, repairAll);
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
        ApiClient api,
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
        return SportFloorball;
    }

    private static string ResolveIdMapPath(IConfiguration config, bool isFootball)
    {
        if (isFootball)
        {
            string? footballPath = config["JoomleagueImporter:Football:IdMapPath"];
            if (!string.IsNullOrWhiteSpace(footballPath))
                return footballPath;
            return Path.Combine(AppContext.BaseDirectory, "id-map-football.json");
        }

        return config["JoomleagueImporter:IdMapPath"] is { Length: > 0 } p
            ? p
            : Path.Combine(AppContext.BaseDirectory, "id-map.json");
    }

    private static string PromptForApiUrl(string defaultUrl)
    {
        Console.Write($"API base URL [{defaultUrl}]: ");
        string? input = Console.ReadLine()?.Trim();
        string url = string.IsNullOrWhiteSpace(input) ? defaultUrl : input;

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            url = "http://" + url;
        if (!url.EndsWith('/'))
            url += "/";

        Console.WriteLine($"Using API: {url}");
        return url;
    }

    private static string ResolveLoginEmail(IConfiguration config)
    {
        const string defaultEmail = "test@myleague.local";
        string? configEmail = config["JoomleagueImporter:LoginEmail"]?.Trim();
        string promptDefault = string.IsNullOrWhiteSpace(configEmail) ? defaultEmail : configEmail;

        Console.Write($"Login email [{promptDefault}]: ");
        string? input = Console.ReadLine()?.Trim();
        return string.IsNullOrWhiteSpace(input) ? promptDefault : input;
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..(max - 3)] + "...";
}
