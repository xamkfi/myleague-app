using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using MahlImporter.Models;

namespace MahlImporter.Scraping;

public class MahlScraper
{
    private readonly HttpClient _http;
    private readonly string _mahlBaseUrl;
    private readonly string _scheduleUrl;
    private readonly string _scrapedDataDir;

    private static readonly Dictionary<string, int> FinnishMonths = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Tammikuu"] = 1, ["Helmikuu"] = 2, ["Maaliskuu"] = 3,
        ["Huhtikuu"] = 4, ["Toukokuu"] = 5, ["Kesäkuu"] = 6,
        ["Heinäkuu"] = 7, ["Elokuu"] = 8, ["Syyskuu"] = 9,
        ["Lokakuu"] = 10, ["Marraskuu"] = 11, ["Joulukuu"] = 12
    };

    public MahlScraper(string mahlBaseUrl, string scheduleRelativeUrl, string scrapedDataDir)
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("User-Agent", "MahlImporter/1.0");
        _mahlBaseUrl = mahlBaseUrl.TrimEnd('/');
        _scheduleUrl = _mahlBaseUrl + "/" + scheduleRelativeUrl;
        _scrapedDataDir = scrapedDataDir;
    }

    public async Task<ScrapedSeason> ScrapeAllAsync()
    {
        string cacheFile = Path.Combine(_scrapedDataDir, "scraped_season.json");
        if (File.Exists(cacheFile))
        {
            Console.WriteLine($"Found cached scraped data at {cacheFile}");
            Console.Write("Use cached data? (Y/n): ");
            string? answer = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(answer) || answer.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                string cached = await File.ReadAllTextAsync(cacheFile);
                ScrapedSeason? cachedSeason = JsonSerializer.Deserialize<ScrapedSeason>(cached, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (cachedSeason != null)
                {
                    Console.WriteLine($"Loaded {cachedSeason.Teams.Count} teams and {cachedSeason.Matches.Count} matches from cache.");
                    return cachedSeason;
                }
            }
        }

        Console.WriteLine("=== Phase 1: Scraping MAHL website ===\n");

        ScrapedSeason season = new();

        Console.WriteLine($"Fetching schedule page: {_scheduleUrl}");
        string scheduleHtml = await FetchPageAsync(_scheduleUrl);

        IBrowsingContext context = BrowsingContext.New(Configuration.Default);
        IDocument scheduleDoc = await context.OpenAsync(req => req.Content(scheduleHtml));

        season.Name = ExtractSeasonName(scheduleDoc);
        Console.WriteLine($"Season: {season.Name}");

        List<TeamLink> teamLinks = ExtractTeamLinks(scheduleDoc);
        teamLinks = teamLinks.Where(t =>
            !t.Name.Equals("VE1", StringComparison.OrdinalIgnoreCase) &&
            !t.Name.Equals("VE2", StringComparison.OrdinalIgnoreCase) &&
            !t.Name.StartsWith("VE", StringComparison.OrdinalIgnoreCase)).ToList();
        Console.WriteLine($"Found {teamLinks.Count} teams");

        List<MatchRow> matchRows = ExtractMatchRows(scheduleDoc);
        Console.WriteLine($"Found {matchRows.Count} matches (before VE filtering)");

        matchRows = matchRows.Where(m => !IsPlayoffMatch(m)).ToList();
        Console.WriteLine($"After filtering VE1/VE2: {matchRows.Count} matches\n");

        Console.WriteLine("--- Scraping team rosters ---");
        foreach (TeamLink tl in teamLinks)
        {
            Console.Write($"  {tl.Name}... ");
            ScrapedTeam team = await ScrapeRosterAsync(context, tl);
            season.Teams.Add(team);
            string logoInfo = team.LogoUrl != null ? $", logo: {team.LogoUrl}" : "";
            Console.WriteLine($"{team.Players.Count} players ({team.Players.Count(p => p.IsGoalkeeper)} GK){logoInfo}");
            await Task.Delay(200);
        }

        Console.WriteLine($"\n--- Scraping match reports ({matchRows.Count} matches) ---");
        int matchIdx = 0;
        foreach (MatchRow mr in matchRows)
        {
            matchIdx++;
            Console.Write($"  [{matchIdx}/{matchRows.Count}] {mr.HomeTeam} vs {mr.AwayTeam}... ");
            ScrapedMatch match = await ScrapeMatchReportAsync(context, mr, season.Teams);
            season.Matches.Add(match);
            Console.WriteLine($"{match.Goals.Count} goals, {match.Penalties.Count} penalties");
            await Task.Delay(200);
        }

        Directory.CreateDirectory(_scrapedDataDir);
        string json = JsonSerializer.Serialize(season, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(cacheFile, json);
        Console.WriteLine($"\nScraped data saved to {cacheFile}");

        return season;
    }

    private async Task<string> FetchPageAsync(string url)
    {
        HttpResponseMessage resp = await _http.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    private static string ExtractSeasonName(IDocument doc)
    {
        string title = doc.Title ?? "";
        Match m = Regex.Match(title, @"Otteluohjelma:\s*(.+)");
        return m.Success ? m.Groups[1].Value.Trim() : title.Trim();
    }

    private List<TeamLink> ExtractTeamLinks(IDocument doc)
    {
        Dictionary<string, TeamLink> teams = new(StringComparer.OrdinalIgnoreCase);

        IElement? fixturesTable = doc.QuerySelector("table.fixtures");
        if (fixturesTable == null) return teams.Values.ToList();

        foreach (IElement link in fixturesTable.QuerySelectorAll("a"))
        {
            string href = link.GetAttribute("href") ?? "";
            string onclick = link.GetAttribute("onclick") ?? "";
            if (!href.Contains("javascript:void") || !onclick.Contains("switchMenu")) continue;

            string name = link.TextContent.Trim();
            if (string.IsNullOrEmpty(name) || name.Length < 2 || teams.ContainsKey(name)) continue;

            IElement? hiddenDiv = link.NextElementSibling;
            if (hiddenDiv?.TagName.Equals("DIV", StringComparison.OrdinalIgnoreCase) != true) continue;

            IElement? rosterLink = hiddenDiv.QuerySelector("a[href*='view=roster']");
            if (rosterLink == null) continue;

            string rosterHref = rosterLink.GetAttribute("href") ?? "";
            string tidRaw = ExtractParam(rosterHref, "tid");
            string tid = tidRaw.Split(':')[0].Split('%')[0];
            string fullUrl = rosterHref.StartsWith("http") ? rosterHref : _mahlBaseUrl + "/" + rosterHref.TrimStart('/');
            teams[name] = new TeamLink { Name = name, Tid = tid, RosterUrl = fullUrl };
        }

        return teams.Values.ToList();
    }

    private List<MatchRow> ExtractMatchRows(IDocument doc)
    {
        List<MatchRow> matches = [];

        IElement? fixturesTable = doc.QuerySelector("table.fixtures");
        if (fixturesTable == null) return matches;

        foreach (IElement row in fixturesTable.QuerySelectorAll("tr.sectiontableentry1, tr.sectiontableentry2"))
        {
            IElement? matchReportLink = row.QuerySelector("a[href*='view=matchreport']");
            if (matchReportLink == null) continue;

            string mrHref = matchReportLink.GetAttribute("href") ?? "";
            string mid = ExtractParam(mrHref, "mid");
            if (string.IsNullOrEmpty(mid) || matches.Any(m => m.Mid == mid)) continue;

            List<string> teamNames = [];
            foreach (IElement link in row.QuerySelectorAll("a"))
            {
                string href = link.GetAttribute("href") ?? "";
                string onclick = link.GetAttribute("onclick") ?? "";
                if (href.Contains("javascript:void") && onclick.Contains("switchMenu"))
                {
                    string name = link.TextContent.Trim();
                    if (!string.IsNullOrEmpty(name)) teamNames.Add(name);
                }
            }

            if (teamNames.Count < 2) continue;

            Match scoreMatch = Regex.Match(matchReportLink.TextContent.Trim(), @"(\d+)\s*[-–:]\s*(\d+)");
            int hs = scoreMatch.Success ? int.Parse(scoreMatch.Groups[1].Value) : 0;
            int asc = scoreMatch.Success ? int.Parse(scoreMatch.Groups[2].Value) : 0;

            DateTime? scheduleDate = null;
            string? roundLabel = null;

            foreach (IElement cell in row.QuerySelectorAll("td"))
            {
                string cellText = cell.TextContent.Trim();

                Match dateMatch = Regex.Match(cellText, @"^(\d{2})\.(\d{2})\.(\d{4})$");
                if (dateMatch.Success)
                {
                    int day = int.Parse(dateMatch.Groups[1].Value);
                    int month = int.Parse(dateMatch.Groups[2].Value);
                    int year = int.Parse(dateMatch.Groups[3].Value);
                    scheduleDate = new DateTime(year, month, day);
                }

                Match timeMatch = Regex.Match(cellText, @"^(\d{2}):(\d{2})\s*h$");
                if (timeMatch.Success && scheduleDate.HasValue)
                {
                    int hour = int.Parse(timeMatch.Groups[1].Value);
                    int minute = int.Parse(timeMatch.Groups[2].Value);
                    scheduleDate = scheduleDate.Value.Date.AddHours(hour).AddMinutes(minute);
                }

                IElement? roundLink = cell.QuerySelector("a[href*='view=results']");
                if (roundLink != null)
                    roundLabel = roundLink.TextContent.Trim();
            }

            string fullUrl = mrHref.StartsWith("http") ? mrHref : _mahlBaseUrl + "/" + mrHref.TrimStart('/');

            matches.Add(new MatchRow
            {
                Mid = mid,
                MatchReportUrl = fullUrl,
                HomeTeam = teamNames[0],
                AwayTeam = teamNames[1],
                HomeScore = hs,
                AwayScore = asc,
                ScheduleDate = scheduleDate,
                RoundLabel = roundLabel
            });
        }

        return matches;
    }

    private static bool IsPlayoffMatch(MatchRow match)
    {
        string combined = $"{match.HomeTeam} {match.AwayTeam} {match.MatchNumber} {match.RoundLabel}".ToUpperInvariant();
        return combined.Contains("VE1") || combined.Contains("VE2");
    }

    private async Task<ScrapedTeam> ScrapeRosterAsync(IBrowsingContext context, TeamLink teamLink)
    {
        ScrapedTeam team = new() { Name = teamLink.Name, MahlTeamId = teamLink.Tid, RosterUrl = teamLink.RosterUrl };

        try
        {
            string html = await FetchPageAsync(teamLink.RosterUrl);
            IDocument doc = await context.OpenAsync(req => req.Content(html));

            IElement? logoImg = doc.QuerySelector("img[src*='com_joomleague/teams/']");
            if (logoImg != null)
            {
                string src = logoImg.GetAttribute("src") ?? "";
                team.LogoUrl = src.StartsWith("http") ? src : _mahlBaseUrl + "/" + src.TrimStart('/');
            }

            foreach (IElement table in doc.QuerySelectorAll("table"))
            {
                string tableText = table.TextContent;
                if (!tableText.Contains("Maalivahti", StringComparison.OrdinalIgnoreCase) &&
                    !tableText.Contains("kk", StringComparison.OrdinalIgnoreCase))
                    continue;

                bool currentSectionIsGk = false;

                foreach (IElement row in table.QuerySelectorAll("tr"))
                {
                    if (row.ClassList.Contains("rosterheader") ||
                        row.ClassList.Contains("sectiontableheader"))
                    {
                        string headerText = row.TextContent.Trim();
                        currentSectionIsGk = headerText.Contains("Maalivahti", StringComparison.OrdinalIgnoreCase) ||
                                             headerText.Contains("alivahti", StringComparison.OrdinalIgnoreCase);
                        continue;
                    }

                    IElement? playerLink = row.QuerySelector("a[href*='pid=']");
                    if (playerLink == null) continue;

                    string playerName = playerLink.TextContent.Trim();
                    if (string.IsNullOrEmpty(playerName) || playerName.StartsWith("-")) continue;

                    string pid = ExtractParam(playerLink.GetAttribute("href") ?? "", "pid").Split(':')[0].Split('%')[0];
                    if (team.Players.Any(p => p.MahlPlayerId == pid)) continue;

                    IElement? jerseyCell = row.QuerySelector("td.td_c");
                    int jersey = 0;
                    if (jerseyCell != null)
                        int.TryParse(jerseyCell.TextContent.Trim(), out jersey);

                    string[] nameParts = playerName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    string firstName = nameParts.Length > 0 ? nameParts[0] : playerName;
                    string lastName = nameParts.Length > 1 ? nameParts[1] : "";

                    team.Players.Add(new ScrapedPlayer
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        JerseyNumber = jersey,
                        IsGoalkeeper = currentSectionIsGk,
                        MahlPlayerId = pid
                    });
                }

                if (team.Players.Count > 0) break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" WARN: Failed to scrape roster for {teamLink.Name}: {ex.Message}");
        }

        return team;
    }

    private async Task<ScrapedMatch> ScrapeMatchReportAsync(IBrowsingContext context, MatchRow matchRow, List<ScrapedTeam> teams)
    {
        ScrapedMatch match = new()
        {
            MahlMatchId = matchRow.Mid,
            MatchReportUrl = matchRow.MatchReportUrl,
            HomeTeamName = matchRow.HomeTeam,
            AwayTeamName = matchRow.AwayTeam,
            HomeScore = matchRow.HomeScore,
            AwayScore = matchRow.AwayScore,
            OriginalDate = matchRow.ScheduleDate ?? default
        };

        Dictionary<string, string> tidToTeamName = teams.ToDictionary(t => t.MahlTeamId, t => t.Name, StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> pidToTeamName = [];
        foreach (ScrapedTeam t in teams)
        {
            foreach (ScrapedPlayer p in t.Players)
            {
                pidToTeamName[p.MahlPlayerId] = t.Name;
            }
        }

        try
        {
            string html = await FetchPageAsync(matchRow.MatchReportUrl);
            IDocument doc = await context.OpenAsync(req => req.Content(html));

            string title = doc.Title ?? "";
            Match titleMatch = Regex.Match(title, @"Raportti:\s*(.+?)\s+vs\s+(.+)");
            if (titleMatch.Success)
            {
                if (string.IsNullOrEmpty(match.HomeTeamName))
                    match.HomeTeamName = titleMatch.Groups[1].Value.Trim();
                if (string.IsNullOrEmpty(match.AwayTeamName))
                    match.AwayTeamName = titleMatch.Groups[2].Value.Trim();
            }

            ExtractMatchDetails(doc, match);
            ExtractEventsFromReport(doc, match, tidToTeamName, pidToTeamName);
        }
        catch (Exception ex)
        {
            Console.Write($"WARN: {ex.Message} ");
            if (matchRow.HomeScore > 0 || matchRow.AwayScore > 0)
            {
                match.HomeScore = matchRow.HomeScore;
                match.AwayScore = matchRow.AwayScore;
            }
        }

        return match;
    }

    private void ExtractMatchDetails(IDocument doc, ScrapedMatch match)
    {
        string bodyText = doc.Body?.TextContent ?? "";

        if (match.OriginalDate == default)
        {
            Match dateMatch = Regex.Match(bodyText, @"Ottelup.iv.:\s*\w+,\s*(\d{1,2})\s+(\w+)\s+(\d{4})");
            if (dateMatch.Success)
            {
                int day = int.Parse(dateMatch.Groups[1].Value);
                string monthName = dateMatch.Groups[2].Value;
                int year = int.Parse(dateMatch.Groups[3].Value);
                if (FinnishMonths.TryGetValue(monthName, out int month))
                {
                    Match timeMatch = Regex.Match(bodyText, @"aloitusaika:\s*(\d{1,2}):(\d{2})");
                    int hour = timeMatch.Success ? int.Parse(timeMatch.Groups[1].Value) : 18;
                    int minute = timeMatch.Success ? int.Parse(timeMatch.Groups[2].Value) : 0;
                    match.OriginalDate = new DateTime(year, month, day, hour, minute, 0);
                }
            }
        }

        Match mnMatch = Regex.Match(bodyText, @"Match number:\s*(\d+)");
        if (mnMatch.Success)
            match.MatchNumber = mnMatch.Groups[1].Value;

        IHtmlCollection<IElement> scoreCells = doc.QuerySelectorAll("table td");
        foreach (IElement cell in scoreCells)
        {
            string txt = cell.TextContent.Trim();
            if (int.TryParse(txt, out int score) && score >= 0 && score <= 50)
            {
                IElement? nextSibling = cell.NextElementSibling;
                if (nextSibling != null && int.TryParse(nextSibling.TextContent.Trim(), out int otherScore) && otherScore >= 0 && otherScore <= 50)
                {
                    if (score + otherScore > 0)
                    {
                        match.HomeScore = score;
                        match.AwayScore = otherScore;
                        break;
                    }
                }
            }
        }
    }

    private static void ExtractEventsFromReport(
        IDocument doc,
        ScrapedMatch match,
        Dictionary<string, string> tidToTeamName,
        Dictionary<string, string> pidToTeamName)
    {
        IElement? eventsTable = doc.QuerySelector("table.eventstable");

        if (eventsTable == null)
        {
            foreach (IElement table in doc.QuerySelectorAll("table"))
            {
                if (table.TextContent.Contains("Aika") && table.TextContent.Contains("Event"))
                {
                    eventsTable = table;
                    break;
                }
            }
        }

        if (eventsTable == null) return;

        ScrapedGoal? pendingGoal = null;

        foreach (IElement row in eventsTable.QuerySelectorAll("tr[id^='event-'], tr.sectiontableentry1, tr.sectiontableentry2"))
        {
            IHtmlCollection<IElement> cells = row.QuerySelectorAll("td");
            if (cells.Length < 3) continue;

            string timeText = cells[0].TextContent.Trim();
            Match timeParse = Regex.Match(timeText, @"(\d{1,2})[:\.](\d{2})");
            if (!timeParse.Success) continue;

            int minutes = int.Parse(timeParse.Groups[1].Value);
            int seconds = int.Parse(timeParse.Groups[2].Value);

            IElement? eventImg = cells[1].QuerySelector("img");
            string eventType = eventImg?.GetAttribute("alt")?.Trim() ?? "";

            IElement descCell = cells[cells.Length - 1];
            string eventText = descCell.TextContent.Trim();

            IElement? playerLink = descCell.QuerySelector("a[href*='pid=']");
            string playerName = playerLink?.TextContent.Trim() ?? "";
            string playerHref = playerLink?.GetAttribute("href") ?? "";
            string tid = ExtractParam(playerHref, "tid").Split(':')[0].Split('%')[0];
            string pid = ExtractParam(playerHref, "pid").Split(':')[0].Split('%')[0];

            string teamName = ResolveTeamName(tid, pid, tidToTeamName, pidToTeamName, match);

            bool isGoal = eventType.Equals("Maali", StringComparison.OrdinalIgnoreCase) ||
                          eventText.Contains("Maali", StringComparison.OrdinalIgnoreCase);
            bool isAssist = eventType.Contains("Sy", StringComparison.OrdinalIgnoreCase) &&
                            eventType.Contains("tt", StringComparison.OrdinalIgnoreCase) ||
                            eventText.Contains("Sy", StringComparison.OrdinalIgnoreCase) &&
                            eventText.Contains("tt", StringComparison.OrdinalIgnoreCase);
            bool isPenalty = eventType.Contains("hy", StringComparison.OrdinalIgnoreCase) ||
                             eventText.Contains("hy", StringComparison.OrdinalIgnoreCase) &&
                             eventText.Contains("min", StringComparison.OrdinalIgnoreCase);

            if (isGoal)
            {
                if (pendingGoal != null)
                {
                    match.Goals.Add(pendingGoal);
                    pendingGoal = null;
                }

                pendingGoal = new ScrapedGoal
                {
                    TeamName = teamName,
                    ScorerName = playerName,
                    TimeMinutes = minutes,
                    TimeSeconds = seconds
                };
            }
            else if (isAssist)
            {
                if (pendingGoal != null && pendingGoal.TimeMinutes == minutes && pendingGoal.TimeSeconds == seconds)
                {
                    pendingGoal.AssisterName = playerName;
                    match.Goals.Add(pendingGoal);
                    pendingGoal = null;
                }
                else if (pendingGoal != null)
                {
                    match.Goals.Add(pendingGoal);
                    pendingGoal = null;
                }
            }
            else if (isPenalty)
            {
                if (pendingGoal != null)
                {
                    match.Goals.Add(pendingGoal);
                    pendingGoal = null;
                }

                Match penaltyParse = Regex.Match(eventText, @"\((\d+)\s*(?:\||min)?\s*(.*?)\)");
                int duration = penaltyParse.Success ? int.Parse(penaltyParse.Groups[1].Value) : 2;
                string reason = penaltyParse.Success ? penaltyParse.Groups[2].Value.Trim().Trim('|').Trim() : "";

                match.Penalties.Add(new ScrapedPenalty
                {
                    TeamName = teamName,
                    PlayerName = playerName,
                    TimeMinutes = minutes,
                    TimeSeconds = seconds,
                    DurationMinutes = duration,
                    Reason = reason
                });
            }
        }

        if (pendingGoal != null)
        {
            match.Goals.Add(pendingGoal);
        }

        if (match.Goals.Count == 0)
        {
            ExtractEventsFromScheduleInline(doc, match);
        }
    }

    private static string ResolveTeamName(
        string tid, string pid,
        Dictionary<string, string> tidToTeamName,
        Dictionary<string, string> pidToTeamName,
        ScrapedMatch match)
    {
        if (!string.IsNullOrEmpty(tid) && tidToTeamName.TryGetValue(tid, out string? byTid))
            return byTid;

        if (!string.IsNullOrEmpty(pid) && pidToTeamName.TryGetValue(pid, out string? byPid))
            return byPid;

        return match.HomeTeamName;
    }

    private static void ExtractEventsFromScheduleInline(IDocument doc, ScrapedMatch match)
    {
        string body = doc.Body?.TextContent ?? "";

        MatchCollection goalMatches = Regex.Matches(body, @"(\d{1,2})[:\.](\d{2})['']?\s+(\S+(?:\s+\S+)?)\s*\((\d+)\)");
        foreach (Match gm in goalMatches)
        {
            int min = int.Parse(gm.Groups[1].Value);
            int sec = int.Parse(gm.Groups[2].Value);
            string name = gm.Groups[3].Value.Trim();

            match.Goals.Add(new ScrapedGoal
            {
                ScorerName = name,
                TimeMinutes = min,
                TimeSeconds = sec
            });
        }
    }

    private static string ExtractParam(string url, string param)
    {
        if (string.IsNullOrEmpty(url)) return "";

        string decoded = HttpUtility.UrlDecode(url);
        Match m = Regex.Match(decoded, param + @"=([^&]+)");
        return m.Success ? m.Groups[1].Value : "";
    }

    private class TeamLink
    {
        public string Name { get; set; } = "";
        public string Tid { get; set; } = "";
        public string RosterUrl { get; set; } = "";
    }

    private class MatchRow
    {
        public string Mid { get; set; } = "";
        public string MatchReportUrl { get; set; } = "";
        public string HomeTeam { get; set; } = "";
        public string AwayTeam { get; set; } = "";
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public string? MatchNumber { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string? RoundLabel { get; set; }
    }
}
