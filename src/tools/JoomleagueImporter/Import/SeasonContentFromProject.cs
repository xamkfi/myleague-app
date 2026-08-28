using System.Net;
using System.Text.RegularExpressions;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

public static class SeasonContentFromProject
{
    public const string HistoryHtml = "<p>History data</p>";

    private static readonly Regex PhpStringValue = new(
        @"s:\d+:""((?:\\.|[^""\\])*)""",
        RegexOptions.CultureInvariant);

    public static List<SeasonContentBlockPutItem> BuildItems(OldProject project)
    {
        List<SeasonContentBlockPutItem> items = [];
        string seasonTitle = string.IsNullOrWhiteSpace(project.Name) ? "Season" : project.Name.Trim();

        AddIfUsable(items, seasonTitle, project.Description);
        AddIfUsable(items, "Info", project.ProjectInfo);
        AddIfUsable(items, seasonTitle, ExtractReadableText(project.Extended));
        AddIfUsable(items, seasonTitle, ExtractReadableText(project.Extension));
        AddIfUsable(items, seasonTitle, ExtractReadableText(project.SeasonExtended));

        if (items.Count == 0)
        {
            items.Add(new SeasonContentBlockPutItem
            {
                Title = TruncateTitle(seasonTitle),
                ContentHtml = HistoryHtml,
            });
        }

        return items;
    }

    private static void AddIfUsable(List<SeasonContentBlockPutItem> items, string title, string? raw)
    {
        string? html = ToHtml(raw);
        if (html == null)
            return;

        if (items.Any(existing => string.Equals(existing.ContentHtml, html, StringComparison.Ordinal)))
            return;

        items.Add(new SeasonContentBlockPutItem
        {
            Title = TruncateTitle(title),
            ContentHtml = html,
        });
    }

    private static string? ExtractReadableText(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        string trimmed = raw.Trim();
        if (trimmed.StartsWith("a:", StringComparison.Ordinal) || trimmed.StartsWith("O:", StringComparison.Ordinal))
        {
            List<string> parts = [];
            foreach (Match match in PhpStringValue.Matches(trimmed))
            {
                string value = UnescapePhp(match.Groups[1].Value).Trim();
                if (LooksLikeContent(value))
                    parts.Add(value);
            }

            return parts.Count == 0 ? null : string.Join("\n\n", parts);
        }

        return LooksLikeContent(trimmed) ? trimmed : null;
    }

    private static string? ToHtml(string? raw)
    {
        string? text = ExtractReadableText(raw) ?? (LooksLikeContent(raw) ? raw!.Trim() : null);
        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (text.Contains('<') && text.Contains('>'))
            return text;

        string escaped = WebUtility.HtmlEncode(text);
        string[] paragraphs = escaped.Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (paragraphs.Length == 0)
            return "<p>" + escaped + "</p>";

        return string.Concat(paragraphs.Select(p => "<p>" + p.Replace("\n", "<br />") + "</p>"));
    }

    private static bool LooksLikeContent(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string trimmed = value.Trim();
        if (trimmed.Length < 12)
            return false;
        if (trimmed.StartsWith("a:", StringComparison.Ordinal) || trimmed.StartsWith("O:", StringComparison.Ordinal))
            return false;
        if (int.TryParse(trimmed, out _))
            return false;

        bool hasLetter = trimmed.Any(char.IsLetter);
        bool hasSpaceOrHtml = trimmed.Contains(' ') || trimmed.Contains('<');
        return hasLetter && hasSpaceOrHtml;
    }

    private static string UnescapePhp(string value) =>
        value.Replace("\\\"", "\"").Replace("\\\\", "\\");

    private static string TruncateTitle(string title)
    {
        string trimmed = string.IsNullOrWhiteSpace(title) ? "Season" : title.Trim();
        return trimmed.Length <= 200 ? trimmed : trimmed[..200];
    }
}

public sealed class SeasonContentBlockPutItem
{
    public string Title { get; set; } = "";
    public string ContentHtml { get; set; } = "";
}

public sealed class SeasonContentBlocksPayload
{
    public Guid? SeasonId { get; set; }
    public List<SeasonContentBlockPayload>? Blocks { get; set; }
}

public sealed class SeasonContentBlockPayload
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string ContentHtml { get; set; } = "";
}
