using System.Globalization;
using System.Text.RegularExpressions;
using Domain.Enums.Football;
using Domain.Enums.Hockey.Teams;
using JoomleagueImporter.Sql;

namespace JoomleagueImporter.Models;

/// <summary>
/// Loads the JoomLeague MySQL dump into typed in-memory collections and exposes the
/// floorball subset selected by the configured project name filter.
/// </summary>
public class JoomleagueDatabase
{
    // JoomLeague event type ids (see jos_joomleague_eventtype in the dump).
    public const int EventGoal = 1;
    public const int EventAssist = 2;
    public const int EventPenalty = 3;
    public const int EventPowerPlayGoal = 4;
    public const int EventPowerPlayAssist = 5;
    public const int EventShortHandedAssist = 7;
    public const int EventShortHandedGoal = 8;
    public const int EventRedCard = 9;
    public const int EventYellowCard = 10;

    public static readonly int[] GoalEventTypes = [EventGoal, EventPowerPlayGoal, EventShortHandedGoal];
    public static readonly int[] AssistEventTypes = [EventAssist, EventPowerPlayAssist, EventShortHandedAssist];

    public Dictionary<int, OldClub> Clubs { get; } = [];
    public Dictionary<int, OldTeam> Teams { get; } = [];
    public Dictionary<int, OldPerson> Persons { get; } = [];
    public Dictionary<int, OldProject> Projects { get; } = [];
    public Dictionary<int, OldProjectTeam> ProjectTeams { get; } = [];
    public Dictionary<int, OldTeamPlayer> TeamPlayers { get; } = [];
    public Dictionary<int, OldRound> Rounds { get; } = [];
    public Dictionary<int, OldMatch> Matches { get; } = [];
    public List<OldMatchEvent> MatchEvents { get; } = [];
    public Dictionary<int, string> Playgrounds { get; } = [];

    /// <summary>project_position ids whose position is a goalkeeper position.</summary>
    public HashSet<int> GoalkeeperProjectPositionIds { get; } = [];

    /// <summary>project_position id → football position inferred from the JoomLeague position name.</summary>
    public Dictionary<int, FootballPosition> FootballPositionByProjectPositionId { get; } = [];

    /// <summary>project_position id → hockey position inferred from the JoomLeague position name.</summary>
    public Dictionary<int, HockeyPosition> HockeyPositionByProjectPositionId { get; } = [];

    public HashSet<int> FootballGoalEventTypeIds { get; } = [..GoalEventTypes];
    public HashSet<int> FootballAssistEventTypeIds { get; } = [..AssistEventTypes];
    public HashSet<int> FootballYellowCardEventTypeIds { get; } = [EventYellowCard];
    public HashSet<int> FootballRedCardEventTypeIds { get; } = [EventRedCard];

    private static readonly string[] TableNames =
    [
        "jos_joomleague_club",
        "jos_joomleague_team",
        "jos_joomleague_person",
        "jos_joomleague_project",
        "jos_joomleague_season",
        "jos_joomleague_project_team",
        "jos_joomleague_team_player",
        "jos_joomleague_round",
        "jos_joomleague_match",
        "jos_joomleague_match_event",
        "jos_joomleague_playground",
        "jos_joomleague_project_position",
        "jos_joomleague_position",
        "jos_joomleague_eventtype",
    ];

    public static JoomleagueDatabase Load(string dumpFilePath)
    {
        Dictionary<string, ParsedTable> tables = SqlDumpParser.Parse(dumpFilePath, TableNames);
        JoomleagueDatabase db = new();

        ParsedTable clubs = tables["jos_joomleague_club"];
        {
            int id = clubs.ColumnIndex("id"), name = clubs.ColumnIndex("name"),
                location = clubs.ColumnIndex("location"), website = clubs.ColumnIndex("website");
            foreach (string?[] r in clubs.Rows)
            {
                OldClub club = new()
                {
                    Id = Int(r[id]),
                    Name = Str(r[name]),
                    Location = Str(r[location]),
                    Website = Str(r[website]),
                };
                db.Clubs[club.Id] = club;
            }
        }

        ParsedTable teams = tables["jos_joomleague_team"];
        {
            int id = teams.ColumnIndex("id"), clubId = teams.ColumnIndex("club_id"),
                name = teams.ColumnIndex("name"), shortName = teams.ColumnIndex("short_name");
            foreach (string?[] r in teams.Rows)
            {
                OldTeam team = new()
                {
                    Id = Int(r[id]),
                    ClubId = IntOrNull(r[clubId]),
                    Name = Str(r[name]).Trim(),
                    ShortName = Str(r[shortName]).Trim(),
                };
                db.Teams[team.Id] = team;
            }
        }

        ParsedTable persons = tables["jos_joomleague_person"];
        {
            int id = persons.ColumnIndex("id"), fn = persons.ColumnIndex("firstname"),
                ln = persons.ColumnIndex("lastname"), bd = persons.ColumnIndex("birthday");
            foreach (string?[] r in persons.Rows)
            {
                OldPerson person = new()
                {
                    Id = Int(r[id]),
                    FirstName = Str(r[fn]).Trim(),
                    LastName = Str(r[ln]).Trim(),
                    Birthday = Date(r[bd]),
                };
                db.Persons[person.Id] = person;
            }
        }

        Dictionary<int, string> seasonExtendedById = LoadSeasonExtended(tables["jos_joomleague_season"]);

        ParsedTable projects = tables["jos_joomleague_project"];
        {
            int id = projects.ColumnIndex("id"), name = projects.ColumnIndex("name"),
                start = projects.ColumnIndex("start_date"),
                regTime = projects.ColumnIndex("game_regular_time"), parts = projects.ColumnIndex("game_parts");
            foreach (string?[] r in projects.Rows)
            {
                int? seasonId = OptInt(r, projects, "season_id");
                OldProject project = new()
                {
                    Id = Int(r[id]),
                    Name = Str(r[name]).Trim(),
                    StartDate = Date(r[start]),
                    GameRegularTime = Int(r[regTime], 30),
                    GameParts = Int(r[parts], 2),
                    SeasonId = seasonId,
                    Description = Opt(r, projects, "description"),
                    ProjectInfo = FirstOpt(r, projects, "projectinfo", "project_info", "info", "notes"),
                    Extension = Opt(r, projects, "extension"),
                    Extended = Opt(r, projects, "extended"),
                    SeasonExtended = seasonId is int sid
                        ? seasonExtendedById.GetValueOrDefault(sid)
                        : null,
                };
                db.Projects[project.Id] = project;
            }
        }

        ParsedTable projectTeams = tables["jos_joomleague_project_team"];
        {
            int id = projectTeams.ColumnIndex("id"), projectId = projectTeams.ColumnIndex("project_id"),
                teamId = projectTeams.ColumnIndex("team_id");
            foreach (string?[] r in projectTeams.Rows)
            {
                OldProjectTeam pt = new()
                {
                    Id = Int(r[id]),
                    ProjectId = Int(r[projectId]),
                    TeamId = Int(r[teamId]),
                };
                db.ProjectTeams[pt.Id] = pt;
            }
        }

        ParsedTable teamPlayers = tables["jos_joomleague_team_player"];
        {
            int id = teamPlayers.ColumnIndex("id"), ptId = teamPlayers.ColumnIndex("projectteam_id"),
                personId = teamPlayers.ColumnIndex("person_id"),
                posId = teamPlayers.ColumnIndex("project_position_id"),
                jersey = teamPlayers.ColumnIndex("jerseynumber");
            foreach (string?[] r in teamPlayers.Rows)
            {
                OldTeamPlayer tp = new()
                {
                    Id = Int(r[id]),
                    ProjectTeamId = Int(r[ptId]),
                    PersonId = Int(r[personId]),
                    ProjectPositionId = IntOrNull(r[posId]),
                    JerseyNumber = IntOrNull(r[jersey]),
                };
                db.TeamPlayers[tp.Id] = tp;
            }
        }

        ParsedTable rounds = tables["jos_joomleague_round"];
        {
            int id = rounds.ColumnIndex("id"), projectId = rounds.ColumnIndex("project_id");
            foreach (string?[] r in rounds.Rows)
            {
                OldRound round = new() { Id = Int(r[id]), ProjectId = Int(r[projectId]) };
                db.Rounds[round.Id] = round;
            }
        }

        ParsedTable matches = tables["jos_joomleague_match"];
        {
            int id = matches.ColumnIndex("id"), roundId = matches.ColumnIndex("round_id"),
                pt1 = matches.ColumnIndex("projectteam1_id"), pt2 = matches.ColumnIndex("projectteam2_id"),
                pg = matches.ColumnIndex("playground_id"), date = matches.ColumnIndex("match_date"),
                res1 = matches.ColumnIndex("team1_result"), res2 = matches.ColumnIndex("team2_result"),
                cancel = matches.ColumnIndex("cancel");
            foreach (string?[] r in matches.Rows)
            {
                OldMatch match = new()
                {
                    Id = Int(r[id]),
                    RoundId = Int(r[roundId]),
                    ProjectTeam1Id = Int(r[pt1]),
                    ProjectTeam2Id = Int(r[pt2]),
                    PlaygroundId = IntOrNull(r[pg]),
                    MatchDate = Date(r[date]),
                    Team1Result = FloatToIntOrNull(r[res1]),
                    Team2Result = FloatToIntOrNull(r[res2]),
                    Cancelled = Int(r[cancel]) != 0,
                };
                db.Matches[match.Id] = match;
            }
        }

        ParsedTable events = tables["jos_joomleague_match_event"];
        {
            int id = events.ColumnIndex("id"), matchId = events.ColumnIndex("match_id"),
                ptId = events.ColumnIndex("projectteam_id"), tpId = events.ColumnIndex("teamplayer_id"),
                time = events.ColumnIndex("event_time"), type = events.ColumnIndex("event_type_id"),
                sum = events.ColumnIndex("event_sum");
            foreach (string?[] r in events.Rows)
            {
                OldMatchEvent ev = new()
                {
                    Id = Int(r[id]),
                    MatchId = Int(r[matchId]),
                    ProjectTeamId = Int(r[ptId]),
                    TeamPlayerId = Int(r[tpId]),
                    EventTime = Str(r[time]).Trim(),
                    EventTypeId = Int(r[type]),
                    Count = Math.Max(1, FloatToIntOrNull(r[sum]) ?? 1),
                };
                db.MatchEvents.Add(ev);
            }
        }

        ParsedTable playgrounds = tables["jos_joomleague_playground"];
        {
            int id = playgrounds.ColumnIndex("id"), name = playgrounds.ColumnIndex("name");
            foreach (string?[] r in playgrounds.Rows)
                db.Playgrounds[Int(r[id])] = Str(r[name]).Trim();
        }

        // Position mapping: project_position -> position name (goalkeeper / outfield / football roles).
        ParsedTable positions = tables["jos_joomleague_position"];
        Dictionary<int, FootballPosition> footballPositionByPositionId = [];
        Dictionary<int, HockeyPosition> hockeyPositionByPositionId = [];
        HashSet<int> goaliePositionIds = [];
        if (positions.Columns.Count > 0)
        {
            int id = positions.ColumnIndex("id"), name = positions.ColumnIndex("name");
            foreach (string?[] r in positions.Rows)
            {
                string positionName = Str(r[name]);
                FootballPosition mapped = MapFootballPosition(positionName);
                footballPositionByPositionId[Int(r[id])] = mapped;
                hockeyPositionByPositionId[Int(r[id])] = MapHockeyPosition(positionName);
                if (mapped == FootballPosition.Goalkeeper)
                    goaliePositionIds.Add(Int(r[id]));
            }
        }
        ParsedTable projectPositions = tables["jos_joomleague_project_position"];
        if (projectPositions.Columns.Count > 0)
        {
            int id = projectPositions.ColumnIndex("id"), posId = projectPositions.ColumnIndex("position_id");
            foreach (string?[] r in projectPositions.Rows)
            {
                int projectPositionId = Int(r[id]);
                int positionId = Int(r[posId]);
                if (goaliePositionIds.Contains(positionId))
                    db.GoalkeeperProjectPositionIds.Add(projectPositionId);
                if (footballPositionByPositionId.TryGetValue(positionId, out FootballPosition mapped))
                    db.FootballPositionByProjectPositionId[projectPositionId] = mapped;
                if (hockeyPositionByPositionId.TryGetValue(positionId, out HockeyPosition hockeyMapped))
                    db.HockeyPositionByProjectPositionId[projectPositionId] = hockeyMapped;
            }
        }

        ParseEventTypes(tables["jos_joomleague_eventtype"], db);

        return db;
    }

    public FootballPosition ResolveFootballPosition(int? projectPositionId)
    {
        if (!projectPositionId.HasValue)
            return FootballPosition.Forward;
        if (FootballPositionByProjectPositionId.TryGetValue(projectPositionId.Value, out FootballPosition mapped))
            return mapped;
        if (GoalkeeperProjectPositionIds.Contains(projectPositionId.Value))
            return FootballPosition.Goalkeeper;
        return FootballPosition.Forward;
    }

    public HockeyPosition ResolveHockeyPosition(int? projectPositionId)
    {
        if (!projectPositionId.HasValue)
            return HockeyPosition.Center;
        if (HockeyPositionByProjectPositionId.TryGetValue(projectPositionId.Value, out HockeyPosition mapped))
            return mapped;
        if (GoalkeeperProjectPositionIds.Contains(projectPositionId.Value))
            return HockeyPosition.Goalie;
        return HockeyPosition.Center;
    }

    /// <summary>
    /// Selects the projects whose name matches <paramref name="includePattern"/> (and does not
    /// match <paramref name="excludePattern"/>), and builds the linked import set.
    /// </summary>
    public FloorballImportSet BuildImportSet(string includePattern, string? excludePattern, string? projectIdFilter)
    {
        Regex include = new(includePattern, RegexOptions.IgnoreCase);
        Regex? exclude = string.IsNullOrWhiteSpace(excludePattern) ? null : new Regex(excludePattern, RegexOptions.IgnoreCase);

        HashSet<int>? idFilter = null;
        if (!string.IsNullOrWhiteSpace(projectIdFilter))
        {
            idFilter = projectIdFilter
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse)
                .ToHashSet();
        }

        List<OldProject> selectedProjects = Projects.Values
            .Where(p => include.IsMatch(p.Name))
            .Where(p => exclude == null || !exclude.IsMatch(p.Name))
            .Where(p => idFilter == null || idFilter.Contains(p.Id))
            .OrderBy(p => p.Id)
            .ToList();

        FloorballImportSet set = new();

        // Group rosters by projectteam and matches by project up front.
        Dictionary<int, List<OldTeamPlayer>> rosterByProjectTeam = TeamPlayers.Values
            .GroupBy(tp => tp.ProjectTeamId)
            .ToDictionary(g => g.Key, g => g.ToList());
        Dictionary<int, List<OldMatchEvent>> eventsByMatch = MatchEvents
            .GroupBy(e => e.MatchId)
            .ToDictionary(g => g.Key, g => g.ToList());
        Dictionary<int, List<OldMatch>> matchesByRound = Matches.Values
            .GroupBy(m => m.RoundId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (OldProject project in selectedProjects)
        {
            ProjectImport pi = new() { Project = project };

            List<OldProjectTeam> projectTeams = ProjectTeams.Values
                .Where(pt => pt.ProjectId == project.Id)
                .ToList();

            foreach (OldProjectTeam pt in projectTeams)
            {
                if (!Teams.TryGetValue(pt.TeamId, out OldTeam? team))
                    continue;

                ProjectTeamImport pti = new() { ProjectTeam = pt, Team = team };
                if (rosterByProjectTeam.TryGetValue(pt.Id, out List<OldTeamPlayer>? roster))
                {
                    foreach (OldTeamPlayer tp in roster)
                    {
                        if (!Persons.TryGetValue(tp.PersonId, out OldPerson? person))
                            continue;
                        if (!IsImportablePerson(person))
                            continue;
                        FootballPosition footballPosition = ResolveFootballPosition(tp.ProjectPositionId);
                        HockeyPosition hockeyPosition = ResolveHockeyPosition(tp.ProjectPositionId);
                        pti.Roster.Add(new RosterEntry
                        {
                            TeamPlayer = tp,
                            Person = person,
                            IsGoalkeeper = footballPosition == FootballPosition.Goalkeeper ||
                                           hockeyPosition == HockeyPosition.Goalie,
                            FootballPosition = footballPosition,
                            HockeyPosition = hockeyPosition,
                        });
                    }
                }
                pi.Teams[pt.Id] = pti;
            }

            // Matches whose round belongs to this project. Some matches have round_id = 0;
            // fall back to the home projectteam's project.
            foreach (KeyValuePair<int, List<OldMatch>> kvp in matchesByRound)
            {
                int projectIdOfRound = Rounds.TryGetValue(kvp.Key, out OldRound? round) ? round.ProjectId : -1;
                foreach (OldMatch match in kvp.Value)
                {
                    int effectiveProjectId = projectIdOfRound;
                    if (effectiveProjectId < 0 &&
                        ProjectTeams.TryGetValue(match.ProjectTeam1Id, out OldProjectTeam? homePt))
                    {
                        effectiveProjectId = homePt.ProjectId;
                    }
                    if (effectiveProjectId != project.Id)
                        continue;

                    pi.Matches.Add(new MatchImport
                    {
                        Match = match,
                        Events = eventsByMatch.GetValueOrDefault(match.Id) ?? [],
                    });
                }
            }
            pi.Matches.Sort((a, b) => Nullable.Compare(a.Match.MatchDate, b.Match.MatchDate));

            set.Projects.Add(pi);
        }

        // Unique teams and persons across all selected projects.
        foreach (ProjectImport pi in set.Projects)
        {
            foreach (ProjectTeamImport pti in pi.Teams.Values)
            {
                set.UniqueTeams[pti.Team.Id] = pti.Team;
                foreach (RosterEntry re in pti.Roster)
                    set.UniquePersons[re.Person.Id] = re.Person;
            }
        }

        HashSet<int> teamsWithRoster = set.Projects
            .SelectMany(p => p.Teams.Values)
            .Where(t => t.Roster.Count > 0)
            .Select(t => t.Team.Id)
            .ToHashSet();
        foreach (int teamId in set.UniqueTeams.Keys.ToList())
        {
            if (teamsWithRoster.Contains(teamId))
                continue;
            set.UniqueTeams.Remove(teamId);
            set.SkippedEmptyRosterTeams++;
        }

        return set;
    }

    private static void ParseEventTypes(ParsedTable eventTypes, JoomleagueDatabase db)
    {
        if (eventTypes.Columns.Count == 0)
            return;

        int id = eventTypes.ColumnIndex("id");
        int name = eventTypes.ColumnIndex("name");
        foreach (string?[] r in eventTypes.Rows)
        {
            int eventTypeId = Int(r[id]);
            string eventName = Str(r[name]);
            if (eventName.Contains("keltainen", StringComparison.OrdinalIgnoreCase) ||
                eventName.Contains("yellow", StringComparison.OrdinalIgnoreCase))
            {
                db.FootballYellowCardEventTypeIds.Add(eventTypeId);
            }
            else if (eventName.Contains("punainen", StringComparison.OrdinalIgnoreCase) ||
                     eventName.Contains("red", StringComparison.OrdinalIgnoreCase))
            {
                db.FootballRedCardEventTypeIds.Add(eventTypeId);
            }
            else if (eventName.Contains("syöttö", StringComparison.OrdinalIgnoreCase) ||
                     eventName.Contains("syotto", StringComparison.OrdinalIgnoreCase) ||
                     eventName.Contains("assist", StringComparison.OrdinalIgnoreCase))
            {
                db.FootballAssistEventTypeIds.Add(eventTypeId);
            }
            else if (eventName.Contains("maali", StringComparison.OrdinalIgnoreCase) ||
                     eventName.Contains("goal", StringComparison.OrdinalIgnoreCase))
            {
                db.FootballGoalEventTypeIds.Add(eventTypeId);
            }
        }
    }

    private static HockeyPosition MapHockeyPosition(string name)
    {
        if (name.Contains("maalivahti", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("goalie", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("goalkeeper", StringComparison.OrdinalIgnoreCase))
            return HockeyPosition.Goalie;
        if (name.Contains("puolustaja", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("defenseman", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("defender", StringComparison.OrdinalIgnoreCase))
            return HockeyPosition.Defenseman;
        return HockeyPosition.Center;
    }

    private static FootballPosition MapFootballPosition(string name)
    {
        if (name.Contains("maalivahti", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("goalkeeper", StringComparison.OrdinalIgnoreCase))
            return FootballPosition.Goalkeeper;
        if (name.Contains("puolustaja", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("defender", StringComparison.OrdinalIgnoreCase))
            return FootballPosition.Defender;
        if (name.Contains("keskikenttä", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("keskikentta", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("midfielder", StringComparison.OrdinalIgnoreCase))
            return FootballPosition.Midfielder;
        if (name.Contains("hyökkääjä", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("hyokkaaja", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("forward", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("striker", StringComparison.OrdinalIgnoreCase))
            return FootballPosition.Forward;
        return FootballPosition.Forward;
    }

    private static bool IsImportablePerson(OldPerson person)
    {
        if (string.IsNullOrWhiteSpace(person.FirstName) || string.IsNullOrWhiteSpace(person.LastName))
            return false;
        // The old system used a "!Unknown !Player" ghost person for unattributed events.
        if (person.FirstName.StartsWith('!') || person.LastName.StartsWith('!'))
            return false;
        return true;
    }

    // ── Value helpers ────────────────────────────────────────────

    private static Dictionary<int, string> LoadSeasonExtended(ParsedTable seasons)
    {
        Dictionary<int, string> result = [];
        if (seasons.Columns.Count == 0 || !seasons.TryColumnIndex("id", out int idCol))
            return result;

        foreach (string?[] r in seasons.Rows)
        {
            int seasonId = Int(r[idCol]);
            string? text = FirstOpt(r, seasons, "description", "extended", "extension", "notes", "info");
            if (!string.IsNullOrWhiteSpace(text))
                result[seasonId] = text;
        }

        return result;
    }

    private static string? Opt(string?[] row, ParsedTable table, string column)
    {
        if (!table.TryColumnIndex(column, out int index) || index >= row.Length)
            return null;
        string value = Str(row[index]).Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static string? FirstOpt(string?[] row, ParsedTable table, params string[] columns)
    {
        foreach (string column in columns)
        {
            string? value = Opt(row, table, column);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static int? OptInt(string?[] row, ParsedTable table, string column)
    {
        if (!table.TryColumnIndex(column, out int index) || index >= row.Length)
            return null;
        return IntOrNull(row[index]);
    }

    private static string Str(string? v) => v ?? "";

    private static int Int(string? v, int fallback = 0) =>
        int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i) ? i : fallback;

    private static int? IntOrNull(string? v) =>
        int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i) ? i : null;

    private static int? FloatToIntOrNull(string? v) =>
        double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) ? (int)Math.Round(d) : null;

    private static DateTime? Date(string? v)
    {
        if (string.IsNullOrWhiteSpace(v) || v.StartsWith("0000-00-00", StringComparison.Ordinal))
            return null;
        return DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt) ? dt : null;
    }
}

// ── Import set structures ─────────────────────────────────────────

public class FloorballImportSet
{
    public List<ProjectImport> Projects { get; } = [];
    public Dictionary<int, OldTeam> UniqueTeams { get; } = [];
    public Dictionary<int, OldPerson> UniquePersons { get; } = [];
    public int SkippedEmptyRosterTeams { get; set; }

    public int TotalMatches => Projects.Sum(p => p.Matches.Count);
    public int TotalEvents => Projects.Sum(p => p.Matches.Sum(m => m.Events.Count));
}

public class ProjectImport
{
    public required OldProject Project { get; init; }

    /// <summary>Keyed by projectteam id.</summary>
    public Dictionary<int, ProjectTeamImport> Teams { get; } = [];

    public List<MatchImport> Matches { get; } = [];
}

public class ProjectTeamImport
{
    public required OldProjectTeam ProjectTeam { get; init; }
    public required OldTeam Team { get; init; }
    public List<RosterEntry> Roster { get; } = [];
}

public class RosterEntry
{
    public required OldTeamPlayer TeamPlayer { get; init; }
    public required OldPerson Person { get; init; }
    public bool IsGoalkeeper { get; init; }
    public FootballPosition FootballPosition { get; init; } = FootballPosition.Forward;
    public HockeyPosition HockeyPosition { get; init; } = HockeyPosition.Center;
}

public class MatchImport
{
    public required OldMatch Match { get; init; }
    public required List<OldMatchEvent> Events { get; init; }
}
