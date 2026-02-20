using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using MahlImporter.Import;
using MahlImporter.Models;
using MahlImporter.Scraping;
using Microsoft.Extensions.Configuration;

namespace MahlImporter;

public static class Program
{
    private const string LocalUrl = "http://localhost:8080/";
    private const string AzureDevUrl = "https://myleague-dev-api.azurewebsites.net/";

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

        string mahlBaseUrl = config["MahlImporter:MahlBaseUrl"] ?? "http://mahl.fi/";
        string scheduleUrl = config["MahlImporter:ScheduleUrl"] ?? "index.php?option=com_joomleague&view=teamplan&p=219&Itemid=103";

        string apiBaseUrl = PromptForApiUrl();
        string loginEmail = ResolveLoginEmail(config);
        int operation = PromptForOperation();
        string scrapedDataDir = FindScrapedDataDir();

        try
        {
            using ApiClient api = new(apiBaseUrl);
            await api.AuthenticateAsync(loginEmail);

            EntityImporter entityImporter = new(api);

            switch (operation)
            {
                case 1:
                    await RunFullImportAsync(api, entityImporter, mahlBaseUrl, scheduleUrl, scrapedDataDir);
                    break;
                case 2:
                    await RunUpdateLogosAsync(api, entityImporter, mahlBaseUrl, scheduleUrl, scrapedDataDir);
                    break;
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\nFATAL: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private static async Task RunFullImportAsync(
        ApiClient api,
        EntityImporter entityImporter,
        string mahlBaseUrl,
        string scheduleUrl,
        string scrapedDataDir)
    {
        // ── Phase 1: Scrape ─────────────────────────────────────────
        MahlScraper scraper = new(mahlBaseUrl, scheduleUrl, scrapedDataDir);
        ScrapedSeason season = await scraper.ScrapeAllAsync();

        if (season.Teams.Count == 0 || season.Matches.Count == 0)
        {
            Console.WriteLine("No data scraped. Exiting.");
            return;
        }

        Console.WriteLine($"\nScraped: {season.Teams.Count} teams, {season.Matches.Count} matches, " +
                          $"{season.Teams.Sum(t => t.Players.Count)} total players\n");

        // ── Phase 2: Import entities ─────────────────────────────────
        Console.WriteLine("=== Phase 2: Importing to new system ===\n");

        Dictionary<string, ClubDto> clubMap = await entityImporter.ImportClubsAsync(season.Teams);
        DivisionDto division = await entityImporter.GetOrCreateLiigaDivisionAsync();
        Dictionary<string, (Guid PersonId, Guid PlayerId)> playerMap = await entityImporter.ImportPlayersAsync(season.Teams);
        Dictionary<string, FloorballTeamDto> teamMap = await entityImporter.ImportTeamsAsync(season.Teams, clubMap, division, playerMap);
        Guid refereeId = await entityImporter.GetOrCreateImportRefereeAsync(playerMap);
        FloorballSeasonDto newSeason = await entityImporter.ImportSeasonAsync(season.Name, division, teamMap, season.Matches);

        // ── Phase 3: Import matches ──────────────────────────────────
        Console.WriteLine("\n=== Phase 3: Importing Matches ===");

        string logDir = Path.Combine(scrapedDataDir, "..", "Logs");
        using ImportLogger importLog = new(logDir);
        MatchImporter matchImporter = new(api, importLog);
        await matchImporter.ImportAllMatchesAsync(season.Matches, newSeason, teamMap, playerMap, season.Teams, refereeId);

        Console.WriteLine("\n==========================================================");
        Console.WriteLine("  Import complete!");
        if (importLog.ErrorCount > 0)
            Console.WriteLine($"  {importLog.ErrorCount} errors logged to: {importLog.LogPath}");
        Console.WriteLine("==========================================================");
    }

    private static async Task RunUpdateLogosAsync(
        ApiClient api,
        EntityImporter entityImporter,
        string mahlBaseUrl,
        string scheduleUrl,
        string scrapedDataDir)
    {
        Console.WriteLine("=== Update Logos ===\n");

        // Load cached/scraped data to find logo URLs for clubs/teams that have none yet
        MahlScraper scraper = new(mahlBaseUrl, scheduleUrl, scrapedDataDir);
        ScrapedSeason season = await scraper.ScrapeAllAsync();

        await entityImporter.UpdateLogosAsync(season.Teams);

        Console.WriteLine("\n==========================================================");
        Console.WriteLine("  Logo update complete!");
        Console.WriteLine("==========================================================");
    }

    // ── Prompts ─────────────────────────────────────────────────────

    private static string PromptForApiUrl()
    {
        Console.WriteLine("Select API environment:");
        Console.WriteLine($"  1. Local        ({LocalUrl})");
        Console.WriteLine($"  2. Azure Dev    ({AzureDevUrl})");
        Console.WriteLine("  3. Custom URL");
        Console.Write("\nChoice [1]: ");

        string? input = Console.ReadLine()?.Trim();

        string url = input switch
        {
            "2" => AzureDevUrl,
            "3" => PromptForCustomUrl(),
            _   => LocalUrl,
        };

        Console.WriteLine($"Using API: {url}\n");
        return url;
    }

    private static string PromptForCustomUrl()
    {
        Console.Write("Enter custom API base URL: ");
        string? input = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input))
            return LocalUrl;

        if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            input = "http://" + input;

        if (!input.EndsWith('/'))
            input += "/";

        return input;
    }

    private static string ResolveLoginEmail(IConfiguration config)
    {
        const string defaultEmail = "test@myleague.local";
        string? configEmail = config["MahlImporter:LoginEmail"]?.Trim();
        string promptDefault = string.IsNullOrWhiteSpace(configEmail) ? defaultEmail : configEmail;

        Console.Write($"Login email (press Enter for default) [{promptDefault}]: ");
        string? input = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input))
            return promptDefault;
        return input;
    }

    private static int PromptForOperation()
    {
        Console.WriteLine("Select operation:");
        Console.WriteLine("  1. Full import   (scrape + import all data)");
        Console.WriteLine("  2. Update logos  (re-upload mahl.fi logos to hosted storage)");
        Console.Write("\nChoice [1]: ");

        string? input = Console.ReadLine()?.Trim();

        int choice = input switch
        {
            "2" => 2,
            _   => 1,
        };

        Console.WriteLine();
        return choice;
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
