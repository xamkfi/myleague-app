using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using MahlImporter.Import;
using MahlImporter.Models;
using MahlImporter.Scraping;
using Microsoft.Extensions.Configuration;

namespace MahlImporter;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("==========================================================");
        Console.WriteLine("  MAHL Data Importer");
        Console.WriteLine("  Migrates season data from mahl.fi to the new system");
        Console.WriteLine("==========================================================\n");

        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        string apiBaseUrl = config["MahlImporter:ApiBaseUrl"] ?? "http://localhost:8080/";
        string mahlBaseUrl = config["MahlImporter:MahlBaseUrl"] ?? "http://mahl.fi/";
        string scheduleUrl = config["MahlImporter:ScheduleUrl"] ?? "index.php?option=com_joomleague&view=teamplan&p=219&Itemid=103";
        int yearsToAdd = int.TryParse(config["MahlImporter:YearsToAdd"], out int y) ? y : 2;

        apiBaseUrl = PromptForUrl("API Base URL", apiBaseUrl);
        string scrapedDataDir = FindScrapedDataDir();

        try
        {
            // ── Phase 1: Scrape ─────────────────────────────────────────
            MahlScraper scraper = new(mahlBaseUrl, scheduleUrl, scrapedDataDir);
            ScrapedSeason season = await scraper.ScrapeAllAsync();

            if (season.Teams.Count == 0 || season.Matches.Count == 0)
            {
                Console.WriteLine("No data scraped. Exiting.");
                return 1;
            }

            Console.WriteLine($"\nScraped: {season.Teams.Count} teams, {season.Matches.Count} matches, " +
                              $"{season.Teams.Sum(t => t.Players.Count)} total players\n");

            // ── Phase 2: Import entities ─────────────────────────────────
            Console.WriteLine("=== Phase 2: Importing to new system ===\n");

            using ApiClient api = new(apiBaseUrl);
            await api.AuthenticateAsync();

            EntityImporter entityImporter = new(api);

            Dictionary<string, ClubDto> clubMap = await entityImporter.ImportClubsAsync(season.Teams);
            DivisionDto division = await entityImporter.GetOrCreateLiigaDivisionAsync();
            Dictionary<string, (Guid PersonId, Guid PlayerId)> playerMap = await entityImporter.ImportPlayersAsync(season.Teams);
            Dictionary<string, FloorballTeamDto> teamMap = await entityImporter.ImportTeamsAsync(season.Teams, clubMap, division, playerMap);
            Guid refereeId = await entityImporter.GetOrCreateImportRefereeAsync(playerMap);
            FloorballSeasonDto newSeason = await entityImporter.ImportSeasonAsync(season.Name, division, teamMap, yearsToAdd);

            // ── Phase 3: Import matches ──────────────────────────────────
            Console.WriteLine("\n=== Phase 3: Importing Matches ===");

            string logDir = Path.Combine(scrapedDataDir, "..", "Logs");
            using ImportLogger importLog = new(logDir);
            MatchImporter matchImporter = new(api, importLog, yearsToAdd);
            await matchImporter.ImportAllMatchesAsync(season.Matches, newSeason, teamMap, playerMap, season.Teams, refereeId);

            Console.WriteLine("\n==========================================================");
            Console.WriteLine("  Import complete!");
            if (importLog.ErrorCount > 0)
                Console.WriteLine($"  {importLog.ErrorCount} errors logged to: {importLog.LogPath}");
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

    private static string PromptForUrl(string label, string defaultUrl)
    {
        Console.Write($"{label} [{defaultUrl}]: ");
        string? input = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input))
            return defaultUrl;

        if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            input = "http://" + input;

        if (!input.EndsWith('/'))
            input += "/";

        return input;
    }

    private static string FindScrapedDataDir()
    {
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, "ScrapedData"),
            Path.Combine(Directory.GetCurrentDirectory(), "ScrapedData"),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "tools", "MahlImporter", "ScrapedData"),
        ];

        foreach (string candidate in candidates)
        {
            if (Directory.Exists(candidate))
                return candidate;
        }

        string dir = candidates[0];
        Directory.CreateDirectory(dir);
        return dir;
    }
}
