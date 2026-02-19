namespace MahlImporter.Models;

public class ScrapedSeason
{
    public string Name { get; set; } = string.Empty;
    public List<ScrapedTeam> Teams { get; set; } = [];
    public List<ScrapedMatch> Matches { get; set; } = [];
}

public class ScrapedTeam
{
    public string Name { get; set; } = string.Empty;
    public string MahlTeamId { get; set; } = string.Empty;
    public string RosterUrl { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public List<ScrapedPlayer> Players { get; set; } = [];
}

public class ScrapedPlayer
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int JerseyNumber { get; set; }
    public bool IsGoalkeeper { get; set; }
    public string MahlPlayerId { get; set; } = string.Empty;
}

public class ScrapedMatch
{
    public string MahlMatchId { get; set; } = string.Empty;
    public string MatchReportUrl { get; set; } = string.Empty;
    public string HomeTeamName { get; set; } = string.Empty;
    public string AwayTeamName { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public DateTime OriginalDate { get; set; }
    public string? Venue { get; set; }
    public string? MatchNumber { get; set; }
    public List<ScrapedGoal> Goals { get; set; } = [];
    public List<ScrapedPenalty> Penalties { get; set; } = [];
}

public class ScrapedGoal
{
    public string TeamName { get; set; } = string.Empty;
    public string ScorerName { get; set; } = string.Empty;
    public string? AssisterName { get; set; }
    public int TimeMinutes { get; set; }
    public int TimeSeconds { get; set; }
}

public class ScrapedPenalty
{
    public string TeamName { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int TimeMinutes { get; set; }
    public int TimeSeconds { get; set; }
    public int DurationMinutes { get; set; }
    public string Reason { get; set; } = string.Empty;
}
