using System.Text.Json;
using System.Text.Json.Serialization;

namespace JoomleagueImporter.Import;

public static class ImportExpectedStatsWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string ResolveReportsDirectory()
    {
        return Path.GetFullPath(Path.Combine("..", "..", "..", "reports"), AppContext.BaseDirectory);
    }

    public static string ExpectedPath(string sport) =>
        Path.Combine(ResolveReportsDirectory(), $"expected-{NormalizeSport(sport)}.json");

    public static string ComparePath(string sport) =>
        Path.Combine(ResolveReportsDirectory(), $"compare-{NormalizeSport(sport)}.json");

    public static string WriteExpected(ImportExpectedReport report)
    {
        string path = ExpectedPath(report.Sport);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(report, JsonOptions));
        return path;
    }

    public static string WriteCompare(string sport, ImportCompareReport report)
    {
        string path = ComparePath(sport);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(report, JsonOptions));
        return path;
    }

    private static string NormalizeSport(string sport) =>
        sport.Trim().ToLowerInvariant();
}
