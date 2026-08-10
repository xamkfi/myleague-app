using Application.Features.Common.Divisions.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using JoomleagueImporter.Import;
using JoomleagueImporter.Models;
using Microsoft.Extensions.Configuration;

namespace JoomleagueImporter;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("==========================================================");
        Console.WriteLine("  JoomLeague SQL Dump Importer");
        Console.WriteLine("  Imports floorball data from an old JoomLeague MySQL dump");
        Console.WriteLine("==========================================================\n");

        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        string dumpPath = config["JoomleagueImporter:DumpFilePath"] ?? "";
        string includeFilter = config["JoomleagueImporter:ProjectNameFilter"] ?? "salibandy|sähly";
        string? excludeFilter = config["JoomleagueImporter:ProjectNameExcludeFilter"];
        string? projectIdFilter = config["JoomleagueImporter:ProjectIdFilter"];
        bool dryRun = bool.TryParse(config["JoomleagueImporter:DryRun"], out bool dr) && dr;
        bool fillUnknownGoals = !bool.TryParse(config["JoomleagueImporter:FillUnknownGoals"], out bool fug) || fug;

        // Old JoomLeague match ids whose events should be wiped and re-imported (repair mode).
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

        // ── Phase 1: Parse the dump ──────────────────────────
        Console.WriteLine($"Parsing dump: {dumpPath}");
        DateTime parseStart = DateTime.Now;
        JoomleagueDatabase db = JoomleagueDatabase.Load(dumpPath);
        Console.WriteLine($"Parsed in {(DateTime.Now - parseStart).TotalSeconds:F1}s: " +
                          $"{db.Clubs.Count} clubs, {db.Teams.Count} teams, {db.Persons.Count} persons, " +
                          $"{db.Projects.Count} projects, {db.Matches.Count} matches, {db.MatchEvents.Count} match events.\n");

        FloorballImportSet set = db.BuildImportSet(includeFilter, excludeFilter, projectIdFilter);

        Console.WriteLine($"Selected {set.Projects.Count} floorball projects " +
                          $"(filter: '{includeFilter}', exclude: '{excludeFilter}'):\n");
        Console.WriteLine($"  {"ID",4}  {"Project",-50} {"Teams",5} {"Match",6} {"Event",6}");
        foreach (ProjectImport pi in set.Projects)
        {
            Console.WriteLine($"  {pi.Project.Id,4}  {Truncate(pi.Project.Name, 50),-50} " +
                              $"{pi.Teams.Count,5} {pi.Matches.Count,6} {pi.Matches.Sum(m => m.Events.Count),6}");
        }
        Console.WriteLine();
        Console.WriteLine($"Totals: {set.UniqueTeams.Count} unique teams, {set.UniquePersons.Count} unique persons, " +
                          $"{set.TotalMatches} matches, {set.TotalEvents} events.\n");

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

        // ── Phase 2: Confirm and authenticate ────────────────
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

        string idMapPath = config["JoomleagueImporter:IdMapPath"] is { Length: > 0 } p
            ? p
            : Path.Combine(AppContext.BaseDirectory, "id-map.json");
        IdMapStore idMap = IdMapStore.LoadOrCreate(idMapPath);
        Console.WriteLine($"Id map: {idMapPath} " +
                          $"({idMap.Persons.Count} persons, {idMap.Teams.Count} teams, {idMap.ProcessedMatches.Count} matches already imported)\n");

        string logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
        using ImportLogger log = new(logDir);

        try
        {
            using ApiClient api = new(apiBaseUrl);
            await api.AuthenticateAsync(loginEmail);

            EntityImporter entities = new(api, idMap, log);

            // ── Phase 3: Base entities ───────────────────────
            DivisionDto division = await entities.GetOrCreateImportDivisionAsync();
            await entities.ImportClubsAsync(set, db);
            await entities.ImportPersonsAndPlayersAsync(set);
            await entities.ImportTeamsAsync(set, db, division);
            Guid refereeId = await entities.GetOrCreateImportRefereeAsync();

            // ── Phase 4: Seasons & matches ───────────────────
            MatchImporter matches = new(api, idMap, log, entities, db, fillUnknownGoals, repairMatchIds, repairAll);
            if (repairAll)
                Console.WriteLine("Repair mode: re-importing events for ALL previously processed matches.");
            else if (repairMatchIds.Count > 0)
                Console.WriteLine($"Repair mode: re-importing events for {repairMatchIds.Count} match(es): {string.Join(", ", repairMatchIds)}");

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

            Console.WriteLine("\n==========================================================");
            Console.WriteLine("  Import complete!");
            Console.WriteLine($"  Matches: {matches.Succeeded} completed, {matches.ScheduledOnly} scheduled-only, " +
                              $"{matches.Skipped} skipped, {matches.Repaired} repaired, {matches.Failed} failed.");
            if (log.ErrorCount > 0)
                Console.WriteLine($"  {log.ErrorCount} errors logged to: {log.LogPath}");
            Console.WriteLine("==========================================================");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\nFATAL: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
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
