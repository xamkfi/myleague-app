using System.Text.Json.Serialization;

namespace TournamentExporter;

internal sealed class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = [];
}

internal sealed class SourceTournament
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ContentHtml { get; set; }
    public string? Venue { get; set; }
    public string TournamentStatus { get; set; } = string.Empty;
    public SourceTournamentRules TournamentRules { get; set; } = new();
    public List<SourceGroup> Groups { get; set; } = [];
    public int TeamCount { get; set; }
    public int MatchCount { get; set; }
    public List<SourcePlayoffSlot> PlayoffSchedule { get; set; } = [];
    public string? TeamCategory { get; set; }
}

internal sealed class SourceTournamentRules
{
    public SourceMatchRules GroupStageMatchRules { get; set; } = new();
    public SourceMatchRules PlayoffMatchRules { get; set; } = new();
    public int TeamsAdvancingPerGroup { get; set; }
    public bool HasPlayoffStage { get; set; }
    public bool HasThirdPlaceMatch { get; set; }
}

internal sealed class SourceMatchRules
{
    public int NumberOfPeriods { get; set; } = 2;
    public int PeriodDurationMinutes { get; set; } = 15;
    public bool AllowOvertime { get; set; }
    public int OvertimeDurationMinutes { get; set; } = 5;
    public bool AllowShootout { get; set; }
}

internal sealed class SourceGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<SourceGroupTeam> Teams { get; set; } = [];
}

internal sealed class SourceGroupTeam
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
}

internal sealed class SourcePlayoffSlot
{
    public string Round { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public string? Venue { get; set; }
}

internal sealed class SourceMatch
{
    public Guid Id { get; set; }
    public Guid CompetitionId { get; set; }
    public Guid? HomeTeamId { get; set; }
    public string? HomeTeamName { get; set; }
    public Guid? AwayTeamId { get; set; }
    public string? AwayTeamName { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public string? Venue { get; set; }
    public string? Status { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public bool WentToOvertime { get; set; }
    public bool WentToShootout { get; set; }
    public Guid? HomeActiveGoalieId { get; set; }
    public Guid? AwayActiveGoalieId { get; set; }
    public Guid? TournamentGroupId { get; set; }
    public string? TournamentStage { get; set; }
    public List<SourceGoalEvent> GoalEvents { get; set; } = [];
    public List<SourcePenaltyEvent> PenaltyEvents { get; set; } = [];
    public List<SourceSaveEvent> SaveEvents { get; set; } = [];
}

internal sealed class SourceGoalEvent
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid? AssisterId { get; set; }
    public Guid? SecondaryAssisterId { get; set; }
    public int PeriodNumber { get; set; }
    public int TimeInSeconds { get; set; }
    public bool WasInOvertime { get; set; }
    public bool WasInShootout { get; set; }
    public string? PlayerName { get; set; }
    public string? AssisterName { get; set; }
    public string? SecondaryAssisterName { get; set; }
    public string? GoalType { get; set; }
}

internal sealed class SourcePenaltyEvent
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public Guid? PlayerId { get; set; }
    public string? PenaltyType { get; set; }
    public int Minutes { get; set; }
    public int PeriodNumber { get; set; }
    public int TimeInSeconds { get; set; }
    public string? Description { get; set; }
    public string? PlayerName { get; set; }
}

internal sealed class SourceSaveEvent
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public Guid GoalieId { get; set; }
    public int PeriodNumber { get; set; }
    public int TimeInSeconds { get; set; }
    public bool WasInOvertime { get; set; }
    public bool WasInShootout { get; set; }
    public string? GoalieName { get; set; }
}

internal sealed class SourceTeam
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? HomeArena { get; set; }
    public string? PrimaryJerseyColor { get; set; }
    public string? SecondaryJerseyColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? TeamCategory { get; set; }
    public SourceClub? Club { get; set; }
    public List<SourceRosterPlayer> Roster { get; set; } = [];
}

internal sealed class SourceClub
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? ContactEmail { get; set; }
}

internal sealed class SourceRosterPlayer
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public int? JerseyNumber { get; set; }
    public bool IsActive { get; set; } = true;
}

internal sealed class ExportPayload
{
    [JsonPropertyName("$schema")]
    public string Schema { get; set; } = "myleague-tournament-import/v1";

    public ExportTournament Tournament { get; set; } = new();
    public List<ExportClub> Clubs { get; set; } = [];
    public List<ExportTeam> Teams { get; set; } = [];
    public List<ExportGroup> Groups { get; set; } = [];
    public List<ExportMatch> Matches { get; set; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ExportPlayoffSlot>? PlayoffSchedule { get; set; }
}

internal sealed class ExportTournament
{
    public string Name { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string? Venue { get; set; }
    public string? ContentHtml { get; set; }
    public int GroupStageNumberOfPeriods { get; set; }
    public int GroupStagePeriodDurationMinutes { get; set; }
    public bool GroupStageAllowOvertime { get; set; }
    public int GroupStageOvertimeDurationMinutes { get; set; }
    public bool GroupStageAllowShootout { get; set; }
    public int PlayoffNumberOfPeriods { get; set; }
    public int PlayoffPeriodDurationMinutes { get; set; }
    public bool PlayoffAllowOvertime { get; set; }
    public int PlayoffOvertimeDurationMinutes { get; set; }
    public bool PlayoffAllowShootout { get; set; }
    public int TeamsAdvancingPerGroup { get; set; }
    public bool HasPlayoffStage { get; set; }
    public bool HasThirdPlaceMatch { get; set; }
    public string TeamCategory { get; set; } = "Adult";
}

internal sealed class ExportClub
{
    public string Name { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? City { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Country { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? WebsiteUrl { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LogoUrl { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContactEmail { get; set; }
}

internal sealed class ExportTeam
{
    public string Name { get; set; } = string.Empty;
    public string ClubName { get; set; } = string.Empty;
    public string Category { get; set; } = "Adult";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HomeArena { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PrimaryJerseyColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SecondaryJerseyColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ExportPlayer>? Players { get; set; }
}

internal sealed class ExportPlayer
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? JerseyNumber { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Position { get; set; }
}

internal sealed class ExportGroup
{
    public string Name { get; set; } = string.Empty;
    public List<string> TeamNames { get; set; } = [];
}

internal sealed class ExportMatch
{
    public int MatchNumber { get; set; }
    public string ScheduledDateTime { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Field { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Venue { get; set; }

    public string HomeTeamName { get; set; } = string.Empty;
    public string AwayTeamName { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GroupName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TournamentStage { get; set; }

    public string Status { get; set; } = "Scheduled";
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HomeGoalieName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AwayGoalieName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ExportGoalEvent>? Goals { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ExportPenaltyEvent>? Penalties { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ExportSaveEvent>? Saves { get; set; }
}

internal sealed class ExportGoalEvent
{
    public string TeamName { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AssisterName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SecondaryAssisterName { get; set; }

    public int PeriodNumber { get; set; }
    public int TimeInSeconds { get; set; }
    public bool WasInOvertime { get; set; }
    public bool WasInShootout { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GoalType { get; set; }
}

internal sealed class ExportPenaltyEvent
{
    public string TeamName { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PlayerName { get; set; }

    public string PenaltyType { get; set; } = "Minor";
    public int Minutes { get; set; } = 2;
    public int PeriodNumber { get; set; }
    public int TimeInSeconds { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }
}

internal sealed class ExportSaveEvent
{
    public string TeamName { get; set; } = string.Empty;
    public string GoalieName { get; set; } = string.Empty;
    public int PeriodNumber { get; set; }
    public int TimeInSeconds { get; set; }
    public bool WasInOvertime { get; set; }
    public bool WasInShootout { get; set; }
}

internal sealed class ExportPlayoffSlot
{
    public string Round { get; set; } = string.Empty;
    public int Order { get; set; }
    public string ScheduledDateTime { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Venue { get; set; }
}
