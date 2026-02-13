using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace DataImporter;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Load configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        string defaultUrl = configuration["BaseUrl"] ?? "https://localhost:5001";
        string baseUrl = PromptForBaseUrl(defaultUrl);

        // Initialize HttpClient
        HttpClient http = new HttpClient();
        http.BaseAddress = new Uri(baseUrl);
        http.DefaultRequestHeaders.Accept.Clear();
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        Console.WriteLine($"Importing persons from .jlg files");
        Console.WriteLine($"Target API: {http.BaseAddress}");

        // Setup JSON serializer options
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
        jsonOptions.PropertyNameCaseInsensitive = true;
        jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        // Get DataFiles folder path - try multiple locations
        string dataFilesPath = FindDataFilesFolder();

        if (string.IsNullOrEmpty(dataFilesPath) || !Directory.Exists(dataFilesPath))
        {
            Console.Error.WriteLine($"DataFiles folder not found.");
            Console.Error.WriteLine("Searched locations:");
            Console.Error.WriteLine($"  - {Path.Combine(AppContext.BaseDirectory, "DataFiles")}");
            Console.Error.WriteLine($"  - {Path.Combine(Directory.GetCurrentDirectory(), "DataFiles")}");
            Console.Error.WriteLine($"  - {Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "DataFiles")}");
            Console.Error.WriteLine("Please ensure the DataFiles folder exists in the project directory.");
            http.Dispose();
            return 1;
        }

        Console.WriteLine($"Using DataFiles folder: {dataFilesPath}");

        try
        {
            // Import persons from .jlg files
            ImportStatistics stats = await PersonImporter.ImportFromJlgFilesAsync(http, jsonOptions, dataFilesPath);

            // Display summary
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("Import Summary");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Total processed: {stats.TotalProcessed}");
            Console.WriteLine($"Created: {stats.Created}");
            Console.WriteLine($"Skipped (duplicates): {stats.DuplicatePersonNames.Count}");
            Console.WriteLine($"Skipped (missing data): {stats.SkippedPersonNames.Count}");
            Console.WriteLine($"Failed: {stats.FailedPersons.Count}");

            if (stats.CreatedPersonNames.Count > 0)
            {
                Console.WriteLine("\n" + new string('-', 60));
                Console.WriteLine($"Successfully Created Persons ({stats.CreatedPersonNames.Count}):");
                Console.WriteLine(new string('-', 60));
                foreach (string name in stats.CreatedPersonNames)
                {
                    Console.WriteLine($"  • {name}");
                }
            }

            if (stats.DuplicatePersonNames.Count > 0)
            {
                Console.WriteLine("\n" + new string('-', 60));
                Console.WriteLine($"Failed - Duplicates ({stats.DuplicatePersonNames.Count}):");
                Console.WriteLine(new string('-', 60));
                foreach (string name in stats.DuplicatePersonNames)
                {
                    Console.WriteLine($"  • {name}");
                }
            }

            if (stats.SkippedPersonNames.Count > 0)
            {
                Console.WriteLine("\n" + new string('-', 60));
                Console.WriteLine($"Skipped - Missing Data ({stats.SkippedPersonNames.Count}):");
                Console.WriteLine(new string('-', 60));
                foreach (string name in stats.SkippedPersonNames)
                {
                    Console.WriteLine($"  • {name}");
                }
            }

            if (stats.FailedPersons.Count > 0)
            {
                Console.WriteLine("\n" + new string('-', 60));
                Console.WriteLine($"Failed - Other Errors ({stats.FailedPersons.Count}):");
                Console.WriteLine(new string('-', 60));
                foreach ((string name, string error) in stats.FailedPersons)
                {
                    Console.WriteLine($"  • {name}");
                    Console.WriteLine($"    Error: {error}");
                }
            }

            if (stats.Errors.Count > 0)
            {
                Console.WriteLine("\n" + new string('-', 60));
                Console.WriteLine("Detailed Error Messages:");
                Console.WriteLine(new string('-', 60));
                foreach (string error in stats.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            http.Dispose();
            return stats.Failed > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Import failed: {ex.Message}\n{ex}");
            http.Dispose();
            return 1;
        }
    }

    private static string PromptForBaseUrl(string defaultUrl)
    {
        Console.WriteLine("==========================================================");
        Console.WriteLine("DataImporter - API URL Configuration");
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

        Console.WriteLine($"Using custom URL: {input}");
        return input;
    }

    private static string FindDataFilesFolder()
    {
        // Try 1: Relative to executable directory (for deployed scenarios)
        string path = Path.Combine(AppContext.BaseDirectory, "DataFiles");
        if (Directory.Exists(path))
            return path;

        // Try 2: Relative to current working directory
        path = Path.Combine(Directory.GetCurrentDirectory(), "DataFiles");
        if (Directory.Exists(path))
            return path;

        // Try 3: Go up from executable directory to find project root
        // bin/Debug/net9.0 -> bin/Debug -> bin -> project root
        string? currentDir = AppContext.BaseDirectory;
        for (int i = 0; i < 4; i++)
        {
            path = Path.Combine(currentDir ?? "", "DataFiles");
            if (Directory.Exists(path))
                return Path.GetFullPath(path);
            
            currentDir = Path.GetDirectoryName(currentDir);
            if (string.IsNullOrEmpty(currentDir))
                break;
        }

        // Try 4: Look for DataImporter.csproj and use its directory
        string? projectDir = FindProjectDirectory();
        if (!string.IsNullOrEmpty(projectDir))
        {
            path = Path.Combine(projectDir, "DataFiles");
            if (Directory.Exists(path))
                return Path.GetFullPath(path);
        }

        return string.Empty;
    }

    private static string? FindProjectDirectory()
    {
        // Start from executable directory and search up for DataImporter.csproj
        string? currentDir = AppContext.BaseDirectory;
        for (int i = 0; i < 6; i++)
        {
            if (string.IsNullOrEmpty(currentDir))
                break;

            string csprojPath = Path.Combine(currentDir, "DataImporter.csproj");
            if (File.Exists(csprojPath))
                return currentDir;

            currentDir = Path.GetDirectoryName(currentDir);
        }

        return null;
    }
}
