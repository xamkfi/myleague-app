using System.Globalization;
using System.Text.RegularExpressions;

namespace TournamentExporter;

internal static class ExportBuilder
{
    private static readonly Regex FieldFromVenue = new(
        @"Kentt[aä]\s*([0-9A-Za-z]+)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly HashSet<string> GroupStageLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        "GroupStage",
        "Group",
        "None",
        "",
    };

    public static ExportPayload Build(
        SourceTournament tournament,
        IReadOnlyList<SourceMatch> matches,
        IReadOnlyDictionary<Guid, SourceTeam> teams,
        string teamCategory)
    {
        Dictionary<Guid, string> groupNameById = tournament.Groups
            .ToDictionary(group => group.Id, group => group.Name);

        List<ExportClub> clubs = [];
        HashSet<string> seenClubs = new(StringComparer.OrdinalIgnoreCase);

        List<ExportTeam> exportTeams = [];
        HashSet<string> seenTeams = new(StringComparer.OrdinalIgnoreCase);

        foreach (SourceGroup group in tournament.Groups.OrderBy(g => g.Order).ThenBy(g => g.Name))
        {
            foreach (SourceGroupTeam groupTeam in group.Teams)
            {
                teams.TryGetValue(groupTeam.TeamId, out SourceTeam? team);
                string teamName = NonEmpty(team?.Name) ?? NonEmpty(groupTeam.TeamName) ?? groupTeam.TeamId.ToString();
                if (!seenTeams.Add(teamName))
                    continue;

                SourceClub? club = team?.Club;
                string clubName = NonEmpty(club?.Name) ?? teamName;
                if (seenClubs.Add(clubName))
                    clubs.Add(MapClub(clubName, club));

                exportTeams.Add(MapTeam(teamName, clubName, teamCategory, team));
            }
        }

        List<ExportGroup> groups = tournament.Groups
            .OrderBy(g => g.Order)
            .ThenBy(g => g.Name)
            .Select(group => new ExportGroup
            {
                Name = group.Name,
                TeamNames = group.Teams
                    .Select(t =>
                    {
                        teams.TryGetValue(t.TeamId, out SourceTeam? team);
                        return NonEmpty(team?.Name) ?? NonEmpty(t.TeamName) ?? t.TeamId.ToString();
                    })
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
            })
            .ToList();

        Dictionary<Guid, string> playerNames = BuildPlayerNameLookup(teams.Values);
        List<SourceMatch> exportableMatches = matches
            .Where(m => !string.IsNullOrWhiteSpace(m.HomeTeamName) && !string.IsNullOrWhiteSpace(m.AwayTeamName))
            .OrderBy(m => m.ScheduledDateTime)
            .ThenBy(m => m.Venue)
            .ToList();

        List<ExportMatch> exportMatches = [];
        for (int i = 0; i < exportableMatches.Count; i++)
        {
            SourceMatch match = exportableMatches[i];
            string? groupName = match.TournamentGroupId.HasValue
                && groupNameById.TryGetValue(match.TournamentGroupId.Value, out string? mapped)
                    ? mapped
                    : null;

            exportMatches.Add(MapMatch(match, i + 1, groupName, playerNames));
        }

        EnsureEventPlayersOnTeams(exportTeams, exportMatches);

        List<ExportPlayoffSlot> playoffSchedule = BuildPlayoffSchedule(tournament, matches);

        SourceMatchRules groupRules = tournament.TournamentRules.GroupStageMatchRules;
        SourceMatchRules playoffRules = tournament.TournamentRules.PlayoffMatchRules;

        return new ExportPayload
        {
            Tournament = new ExportTournament
            {
                Name = tournament.Name,
                StartDate = FormatDate(tournament.StartDate),
                EndDate = FormatDate(tournament.EndDate),
                Venue = NonEmpty(tournament.Venue),
                ContentHtml = NonEmpty(tournament.ContentHtml),
                GroupStageNumberOfPeriods = groupRules.NumberOfPeriods,
                GroupStagePeriodDurationMinutes = groupRules.PeriodDurationMinutes,
                GroupStageAllowOvertime = groupRules.AllowOvertime,
                GroupStageOvertimeDurationMinutes = groupRules.OvertimeDurationMinutes,
                GroupStageAllowShootout = groupRules.AllowShootout,
                PlayoffNumberOfPeriods = playoffRules.NumberOfPeriods,
                PlayoffPeriodDurationMinutes = playoffRules.PeriodDurationMinutes,
                PlayoffAllowOvertime = playoffRules.AllowOvertime,
                PlayoffOvertimeDurationMinutes = playoffRules.OvertimeDurationMinutes,
                PlayoffAllowShootout = playoffRules.AllowShootout,
                TeamsAdvancingPerGroup = tournament.TournamentRules.TeamsAdvancingPerGroup,
                HasPlayoffStage = tournament.TournamentRules.HasPlayoffStage || playoffSchedule.Count > 0,
                HasThirdPlaceMatch = tournament.TournamentRules.HasThirdPlaceMatch
                    || playoffSchedule.Exists(s => s.Round == "ThirdPlaceMatch"),
                TeamCategory = teamCategory,
            },
            Clubs = clubs,
            Teams = exportTeams,
            Groups = groups,
            Matches = exportMatches,
            PlayoffSchedule = playoffSchedule.Count > 0 ? playoffSchedule : null,
        };
    }

    public static string InferTeamCategory(string? tournamentName, string? sourceCategory)
    {
        if (TryParseCategory(sourceCategory, out string parsed))
            return parsed;

        if (string.IsNullOrWhiteSpace(tournamentName))
            return "Adult";

        string normalized = tournamentName.Trim().ToLowerInvariant();
        if (Regex.IsMatch(normalized, @"\b(naiset|naisten|nainen|ladies|women|woman)\b", RegexOptions.CultureInvariant))
            return "Women";
        if (Regex.IsMatch(normalized, @"\b(youth|junior(?:it|s)?|nuoret|nuorten|pojat|poikien|tytöt|tytot|u1[0-9]|u2[0-1])\b", RegexOptions.CultureInvariant))
            return "Youth";
        return "Adult";
    }

    public static bool TryParseCategory(string? value, out string category)
    {
        category = "Adult";
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string normalized = value.Trim();
        if (normalized.Equals("Adult", StringComparison.OrdinalIgnoreCase))
        {
            category = "Adult";
            return true;
        }
        if (normalized.Equals("Women", StringComparison.OrdinalIgnoreCase))
        {
            category = "Women";
            return true;
        }
        if (normalized.Equals("Youth", StringComparison.OrdinalIgnoreCase))
        {
            category = "Youth";
            return true;
        }

        return false;
    }

    public static string ToFileName(string tournamentName)
    {
        string slug = Regex.Replace(tournamentName.Trim().ToLowerInvariant(), @"[^a-z0-9äöå]+", "-");
        slug = slug.Trim('-');
        if (string.IsNullOrWhiteSpace(slug))
            slug = "tournament";
        return slug + ".json";
    }

    private static ExportClub MapClub(string clubName, SourceClub? club)
    {
        return new ExportClub
        {
            Name = clubName,
            City = NonEmpty(club?.City),
            Country = NonEmpty(club?.Country),
            WebsiteUrl = IsPlaceholderUrl(club?.WebsiteUrl) ? null : NonEmpty(club?.WebsiteUrl),
            LogoUrl = IsPlaceholderUrl(club?.LogoUrl) ? null : NonEmpty(club?.LogoUrl),
            ContactEmail = IsPlaceholderEmail(club?.ContactEmail) ? null : NonEmpty(club?.ContactEmail),
        };
    }

    private static ExportTeam MapTeam(string teamName, string clubName, string teamCategory, SourceTeam? team)
    {
        List<ExportPlayer> players = (team?.Roster ?? [])
            .Where(p => p.IsActive && !string.IsNullOrWhiteSpace(p.PlayerName))
            .Select(MapPlayer)
            .Where(p => p is not null)
            .Cast<ExportPlayer>()
            .ToList();

        return new ExportTeam
        {
            Name = teamName,
            ClubName = clubName,
            Category = teamCategory,
            HomeArena = NonEmpty(team?.HomeArena),
            PrimaryJerseyColor = NonEmpty(team?.PrimaryJerseyColor),
            SecondaryJerseyColor = NonEmpty(team?.SecondaryJerseyColor),
            Players = players.Count > 0 ? players : null,
        };
    }

    private static ExportMatch MapMatch(
        SourceMatch match,
        int matchNumber,
        string? groupName,
        IReadOnlyDictionary<Guid, string> playerNames)
    {
        List<ExportGoalEvent> goals = match.GoalEvents
            .Select(goal => new ExportGoalEvent
            {
                TeamName = ResolveTeamName(match, goal.TeamId),
                PlayerName = ResolvePlayerName(goal.PlayerName, goal.PlayerId, playerNames),
                AssisterName = ResolveOptionalPlayerName(goal.AssisterName, goal.AssisterId, playerNames),
                SecondaryAssisterName = ResolveOptionalPlayerName(goal.SecondaryAssisterName, goal.SecondaryAssisterId, playerNames),
                PeriodNumber = Math.Max(1, goal.PeriodNumber),
                TimeInSeconds = Math.Max(0, goal.TimeInSeconds),
                WasInOvertime = goal.WasInOvertime,
                WasInShootout = goal.WasInShootout,
                GoalType = NonEmpty(goal.GoalType),
            })
            .Where(goal => goal.PlayerName.Length > 0)
            .ToList();

        List<ExportPenaltyEvent> penalties = match.PenaltyEvents
            .Select(penalty => new ExportPenaltyEvent
            {
                TeamName = ResolveTeamName(match, penalty.TeamId),
                PlayerName = ResolveOptionalPlayerName(penalty.PlayerName, penalty.PlayerId, playerNames),
                PenaltyType = NonEmpty(penalty.PenaltyType) ?? "Minor",
                Minutes = penalty.Minutes > 0 ? penalty.Minutes : 2,
                PeriodNumber = Math.Max(1, penalty.PeriodNumber),
                TimeInSeconds = Math.Max(0, penalty.TimeInSeconds),
                Description = NonEmpty(penalty.Description),
            })
            .ToList();

        List<ExportSaveEvent> saves = match.SaveEvents
            .Select(save => new ExportSaveEvent
            {
                TeamName = ResolveTeamName(match, save.TeamId),
                GoalieName = ResolvePlayerName(save.GoalieName, save.GoalieId, playerNames),
                PeriodNumber = Math.Max(1, save.PeriodNumber),
                TimeInSeconds = Math.Max(0, save.TimeInSeconds),
                WasInOvertime = save.WasInOvertime,
                WasInShootout = save.WasInShootout,
            })
            .Where(save => save.GoalieName.Length > 0)
            .ToList();

        return new ExportMatch
        {
            MatchNumber = matchNumber,
            ScheduledDateTime = FormatDateTime(match.ScheduledDateTime),
            Field = ExtractField(match.Venue),
            Venue = NonEmpty(match.Venue),
            HomeTeamName = match.HomeTeamName!,
            AwayTeamName = match.AwayTeamName!,
            GroupName = groupName,
            TournamentStage = NormalizeTournamentStage(match.TournamentStage),
            Status = NonEmpty(match.Status) ?? "Scheduled",
            HomeScore = match.HomeScore,
            AwayScore = match.AwayScore,
            HomeGoalieName = ResolveOptionalPlayerName(null, match.HomeActiveGoalieId, playerNames),
            AwayGoalieName = ResolveOptionalPlayerName(null, match.AwayActiveGoalieId, playerNames),
            Goals = goals.Count > 0 ? goals : null,
            Penalties = penalties.Count > 0 ? penalties : null,
            Saves = saves.Count > 0 ? saves : null,
        };
    }

    private static void EnsureEventPlayersOnTeams(List<ExportTeam> teams, IEnumerable<ExportMatch> matches)
    {
        Dictionary<string, ExportTeam> byName = teams.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
        foreach (ExportMatch match in matches)
        {
            foreach (ExportGoalEvent goal in match.Goals ?? [])
            {
                AddNamedPlayer(byName, goal.TeamName, goal.PlayerName, "Forward");
                AddNamedPlayer(byName, goal.TeamName, goal.AssisterName, "Forward");
                AddNamedPlayer(byName, goal.TeamName, goal.SecondaryAssisterName, "Forward");
            }

            foreach (ExportPenaltyEvent penalty in match.Penalties ?? [])
                AddNamedPlayer(byName, penalty.TeamName, penalty.PlayerName, "Forward");

            foreach (ExportSaveEvent save in match.Saves ?? [])
                AddNamedPlayer(byName, save.TeamName, save.GoalieName, "Goalkeeper");

            AddNamedPlayer(byName, match.HomeTeamName, match.HomeGoalieName, "Goalkeeper");
            AddNamedPlayer(byName, match.AwayTeamName, match.AwayGoalieName, "Goalkeeper");
        }
    }

    private static void AddNamedPlayer(
        IReadOnlyDictionary<string, ExportTeam> teams,
        string? teamName,
        string? fullName,
        string position)
    {
        if (string.IsNullOrWhiteSpace(teamName) || string.IsNullOrWhiteSpace(fullName))
            return;
        if (!teams.TryGetValue(teamName, out ExportTeam? team))
            return;

        team.Players ??= [];
        (string firstName, string lastName) = SplitName(fullName);
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return;

        bool exists = team.Players.Exists(p =>
            string.Equals(p.FirstName, firstName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(p.LastName, lastName, StringComparison.OrdinalIgnoreCase));
        if (exists)
            return;

        team.Players.Add(new ExportPlayer
        {
            FirstName = firstName,
            LastName = lastName,
            Position = position,
        });
    }

    private static Dictionary<Guid, string> BuildPlayerNameLookup(IEnumerable<SourceTeam> teams)
    {
        Dictionary<Guid, string> names = [];
        foreach (SourceRosterPlayer player in teams.SelectMany(t => t.Roster))
        {
            if (player.PlayerId == Guid.Empty || string.IsNullOrWhiteSpace(player.PlayerName))
                continue;
            names.TryAdd(player.PlayerId, player.PlayerName.Trim());
        }

        return names;
    }

    private static string ResolveTeamName(SourceMatch match, Guid teamId)
    {
        if (match.HomeTeamId == teamId && !string.IsNullOrWhiteSpace(match.HomeTeamName))
            return match.HomeTeamName;
        if (match.AwayTeamId == teamId && !string.IsNullOrWhiteSpace(match.AwayTeamName))
            return match.AwayTeamName;
        return match.HomeTeamName ?? match.AwayTeamName ?? teamId.ToString();
    }

    private static string ResolvePlayerName(string? name, Guid playerId, IReadOnlyDictionary<Guid, string> playerNames)
    {
        return ResolveOptionalPlayerName(name, playerId, playerNames) ?? string.Empty;
    }

    private static string? ResolveOptionalPlayerName(string? name, Guid? playerId, IReadOnlyDictionary<Guid, string> playerNames)
    {
        string? trimmed = UsablePlayerName(name);
        if (trimmed is not null)
            return trimmed;
        if (playerId is Guid id && id != Guid.Empty && playerNames.TryGetValue(id, out string? mapped))
            return UsablePlayerName(mapped);
        return null;
    }

    internal static string? UsablePlayerName(string? name)
    {
        string? trimmed = NonEmpty(name);
        if (trimmed is null)
            return null;
        if (trimmed.Equals("Unknown Player", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return trimmed;
    }

    private static string NormalizeTournamentStage(string? value)
    {
        return (value ?? string.Empty).Trim() switch
        {
            "QuarterFinal" or "Quarterfinal" => "Quarterfinal",
            "SemiFinal" or "Semifinal" => "Semifinal",
            "ThirdPlaceMatch" or "ThirdPlace" => "ThirdPlace",
            "Final" => "Final",
            _ => "GroupStage",
        };
    }

    private static ExportPlayer? MapPlayer(SourceRosterPlayer player)
    {
        (string firstName, string lastName) = SplitName(player.PlayerName);
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return null;

        return new ExportPlayer
        {
            FirstName = firstName,
            LastName = lastName,
            JerseyNumber = player.JerseyNumber is > 0 ? player.JerseyNumber : null,
            Position = NormalizePosition(player.Position),
        };
    }

    private static List<ExportPlayoffSlot> BuildPlayoffSchedule(SourceTournament tournament, IReadOnlyList<SourceMatch> matches)
    {
        if (tournament.PlayoffSchedule.Count > 0)
        {
            return tournament.PlayoffSchedule
                .Select(slot => new ExportPlayoffSlot
                {
                    Round = NormalizePlayoffRound(slot.Round),
                    Order = slot.Order,
                    ScheduledDateTime = FormatDateTime(slot.ScheduledDateTime),
                    Venue = NonEmpty(slot.Venue),
                })
                .Where(slot => slot.Round.Length > 0)
                .ToList();
        }

        Dictionary<string, int> orderByRound = new(StringComparer.OrdinalIgnoreCase);
        List<ExportPlayoffSlot> derived = [];
        foreach (SourceMatch match in matches.Where(m => !IsGroupStage(m)).OrderBy(m => m.ScheduledDateTime))
        {
            string round = NormalizePlayoffRound(match.TournamentStage);
            if (round.Length == 0)
                continue;

            int order = orderByRound.GetValueOrDefault(round, 0);
            orderByRound[round] = order + 1;
            derived.Add(new ExportPlayoffSlot
            {
                Round = round,
                Order = order,
                ScheduledDateTime = FormatDateTime(match.ScheduledDateTime),
                Venue = NonEmpty(match.Venue),
            });
        }

        return derived;
    }

    private static bool IsGroupStage(SourceMatch match)
    {
        return GroupStageLabels.Contains(match.TournamentStage ?? string.Empty);
    }

    private static string NormalizePlayoffRound(string? value)
    {
        return (value ?? string.Empty).Trim() switch
        {
            "QuarterFinal" or "Quarterfinal" => "QuarterFinal",
            "SemiFinal" or "Semifinal" => "SemiFinal",
            "ThirdPlaceMatch" or "ThirdPlace" => "ThirdPlaceMatch",
            "Final" => "Final",
            _ => string.Empty,
        };
    }

    private static string? ExtractField(string? venue)
    {
        if (string.IsNullOrWhiteSpace(venue))
            return null;

        Match match = FieldFromVenue.Match(venue);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static (string FirstName, string LastName) SplitName(string fullName)
    {
        string trimmed = Regex.Replace(fullName.Trim(), @"\s+", " ");
        int lastSpace = trimmed.LastIndexOf(' ');
        if (lastSpace <= 0)
            return (trimmed, trimmed);

        return (trimmed[..lastSpace], trimmed[(lastSpace + 1)..]);
    }

    private static string? NormalizePosition(string? position)
    {
        return position switch
        {
            "Goalkeeper" or "Defender" or "Forward" or "Center" => position,
            _ => null,
        };
    }

    private static string FormatDate(DateTime value)
    {
        DateTime utc = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
        DateTime local = TimeZoneInfo.ConvertTimeFromUtc(utc, HelsinkiTimeZone);
        return local.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static string FormatDateTime(DateTime value)
    {
        DateTime utc = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
        DateTime local = TimeZoneInfo.ConvertTimeFromUtc(utc, HelsinkiTimeZone);
        TimeSpan offset = HelsinkiTimeZone.GetUtcOffset(local);
        string sign = offset < TimeSpan.Zero ? "-" : "+";
        return $"{local:yyyy-MM-ddTHH:mm:ss}{sign}{offset:hh\\:mm}";
    }

    private static TimeZoneInfo HelsinkiTimeZone
    {
        get
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById("Europe/Helsinki", out TimeZoneInfo? iana))
                return iana;
            if (TimeZoneInfo.TryFindSystemTimeZoneById("FLE Standard Time", out TimeZoneInfo? windows))
                return windows;
            return TimeZoneInfo.Utc;
        }
    }

    private static string? NonEmpty(string? value)
    {
        string? trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static bool IsPlaceholderUrl(string? url)
    {
        return !string.IsNullOrWhiteSpace(url)
            && url.Contains("example.com", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlaceholderEmail(string? email)
    {
        return !string.IsNullOrWhiteSpace(email)
            && email.Contains("example.com", StringComparison.OrdinalIgnoreCase);
    }
}
