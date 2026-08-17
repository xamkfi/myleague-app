using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace TournamentExporter;

public static class Program
{
    private static readonly Guid[] DefaultTournamentIds =
    [
        Guid.Parse("cf48480f-f4fe-4dcb-a2d6-234a05f90222"),
        Guid.Parse("156a836f-9e15-49ee-9bbb-13b37f60da75"),
    ];

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("==========================================================");
        Console.WriteLine("  Tournament exporter");
        Console.WriteLine("  Pulls live floorball tournaments into import JSON");
        Console.WriteLine("==========================================================\n");

        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        ExportOptions options = ParseArgs(args, config);
        Console.WriteLine($"Source API : {options.SourceApiUrl}");
        Console.WriteLine($"Output dir : {options.OutputDirectory}");
        Console.WriteLine($"Tournaments: {options.TournamentIds.Count}");
        if (options.Import)
        {
            Console.WriteLine($"Import API : {options.TargetApiUrl} as {options.LoginEmail}");
            if (options.Replace)
                Console.WriteLine("Replace    : existing tournaments with the same name will be deleted first");
        }
        Console.WriteLine();

        Directory.CreateDirectory(options.OutputDirectory);

        JsonSerializerOptions json = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        try
        {
            using SourceApiClient api = new(options.SourceApiUrl);
            List<string> written = [];

            foreach (Guid tournamentId in options.TournamentIds)
            {
                Console.WriteLine($"Fetching tournament {tournamentId}...");
                SourceTournament tournament = await api.GetTournamentAsync(tournamentId);
                List<SourceMatch> matches = await api.GetMatchesAsync(tournamentId);
                Console.WriteLine($"  {tournament.Name}: {matches.Count} listed matches — loading events…");
                await api.HydrateMatchEventsAsync(matches);

                Dictionary<Guid, SourceTeam> teams = [];
                IEnumerable<Guid> teamIds = tournament.Groups
                    .SelectMany(g => g.Teams)
                    .Select(t => t.TeamId)
                    .Concat(matches.SelectMany(m => new[] { m.HomeTeamId, m.AwayTeamId }.OfType<Guid>()))
                    .Distinct();

                foreach (Guid teamId in teamIds)
                {
                    SourceTeam? team = await api.GetTeamAsync(teamId);
                    if (team is not null)
                        teams[teamId] = team;
                    else
                        Console.WriteLine($"  WARN: team {teamId} could not be loaded; using group listing only.");
                }

                string category = options.CategoryOverride
                    ?? ExportBuilder.InferTeamCategory(tournament.Name, tournament.TeamCategory);
                ExportPayload payload = ExportBuilder.Build(tournament, matches, teams, category);

                string fileName = ExportBuilder.ToFileName(tournament.Name);
                string path = Path.Combine(options.OutputDirectory, fileName);
                await File.WriteAllTextAsync(path, JsonSerializer.Serialize(payload, json) + Environment.NewLine, Encoding.UTF8);

                written.Add(path);
                int goals = payload.Matches.Sum(m => m.Goals?.Count ?? 0);
                int penalties = payload.Matches.Sum(m => m.Penalties?.Count ?? 0);
                int saves = payload.Matches.Sum(m => m.Saves?.Count ?? 0);
                Console.WriteLine(
                    $"  Wrote {fileName}  [{category}]  " +
                    $"{payload.Teams.Count} teams, {payload.Matches.Count} matches, " +
                    $"{payload.PlayoffSchedule?.Count ?? 0} playoff slots, " +
                    $"{payload.Teams.Sum(t => t.Players?.Count ?? 0)} players, " +
                    $"{goals} goals, {penalties} penalties, {saves} saves");
            }

            Console.WriteLine("\n==========================================================");
            Console.WriteLine("  Export complete.");
            foreach (string path in written)
                Console.WriteLine($"  {Path.GetFullPath(path)}");
            Console.WriteLine("==========================================================");

            if (options.Import)
            {
                Console.WriteLine();
                await TournamentImporter.ImportDirectoryAsync(
                    options.TargetApiUrl,
                    options.LoginEmail,
                    options.OutputDirectory,
                    options.Replace);
                Console.WriteLine("==========================================================");
                Console.WriteLine("  Import complete.");
                Console.WriteLine("==========================================================");
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

    private static ExportOptions ParseArgs(string[] args, IConfiguration config)
    {
        string sourceApi = config["TournamentExporter:SourceApiUrl"] ?? "https://myleague-dev-api.azurewebsites.net/";
        string outputDir = config["TournamentExporter:OutputDirectory"] ?? "exports";
        string targetApi = config["TournamentExporter:TargetApiUrl"] ?? "http://localhost:8080/";
        string loginEmail = config["TournamentExporter:LoginEmail"] ?? "test@myleague.local";
        List<Guid> ids = [];
        string? categoryOverride = null;
        bool import = false;
        bool replace = false;

        IConfigurationSection configuredIds = config.GetSection("TournamentExporter:TournamentIds");
        foreach (IConfigurationSection child in configuredIds.GetChildren())
        {
            if (Guid.TryParse(child.Value, out Guid configuredId))
                ids.Add(configuredId);
        }

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            string? next = i + 1 < args.Length ? args[i + 1] : null;
            switch (arg)
            {
                case "--api" when next is not null:
                    sourceApi = next;
                    i++;
                    break;
                case "--out" when next is not null:
                    outputDir = next;
                    i++;
                    break;
                case "--id" when next is not null:
                    if (!Guid.TryParse(next, out Guid parsedId))
                        throw new ArgumentException($"Invalid tournament id: {next}");
                    ids.Add(parsedId);
                    i++;
                    break;
                case "--category" when next is not null:
                    if (!ExportBuilder.TryParseCategory(next, out string parsedCategory))
                        throw new ArgumentException("Category must be Adult, Women, or Youth.");
                    categoryOverride = parsedCategory;
                    i++;
                    break;
                case "--import":
                    import = true;
                    break;
                case "--replace":
                    replace = true;
                    import = true;
                    break;
                case "--target" when next is not null:
                    targetApi = next;
                    import = true;
                    i++;
                    break;
                case "--email" when next is not null:
                    loginEmail = next;
                    i++;
                    break;
                case "--help":
                case "-h":
                    PrintUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {arg}");
            }
        }

        if (ids.Count == 0)
            ids.AddRange(DefaultTournamentIds);

        if (!Path.IsPathRooted(outputDir))
            outputDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), outputDir));

        return new ExportOptions(
            SourceApiClient.NormalizeBaseUrl(sourceApi),
            outputDir,
            ids.Distinct().ToList(),
            categoryOverride,
            import,
            SourceApiClient.NormalizeBaseUrl(targetApi),
            loginEmail,
            replace);
    }

    private static void PrintUsage()
    {
        Console.WriteLine("""
            Usage:
              dotnet run --project src/tools/TournamentExporter -- [options]

            Options:
              --api <url>          Source API (default: Azure Dev)
              --out <dir>          Output directory (default: ./exports)
              --id <guid>          Tournament id (repeatable; defaults to the two PMT 2026 tournaments)
              --category <value>   Force Adult, Women, or Youth on every exported file
              --import             After export, import the JSON files into --target
              --replace            Delete existing tournaments with the same name before import
              --target <url>       Local/target API (implies --import; default: http://localhost:8080/)
              --email <email>      Login email for the target API (default: test@myleague.local)
              --help               Show this help

            The JSON is the myleague-tournament-import/v1 format used by the admin import modal.
            Team category is inferred from the tournament name (Naiset → Women, otherwise Adult)
            when the source API does not already send one.
            """);
    }

    private sealed record ExportOptions(
        string SourceApiUrl,
        string OutputDirectory,
        List<Guid> TournamentIds,
        string? CategoryOverride,
        bool Import,
        string TargetApiUrl,
        string LoginEmail,
        bool Replace);
}
