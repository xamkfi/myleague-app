namespace JoomleagueImporter.Import;

public sealed class ImportExpectedReport
{
    public string Sport { get; set; } = "";
    public string DumpFile { get; set; } = "";
    public DateTime GeneratedAtUtc { get; set; }
    public List<int> ImportFailedMatchIds { get; set; } = [];
    public List<ExpectedSeasonStats> Seasons { get; set; } = [];
}

public sealed class ExpectedSeasonStats
{
    public int OldProjectId { get; set; }
    public string Name { get; set; } = "";
    public Guid? NewSeasonId { get; set; }
    public int MatchCount { get; set; }
    public int PlayedMatchCount { get; set; }
    public int CancelledMatchCount { get; set; }
    public int TotalGoals { get; set; }
    public List<ExpectedTeamStats> Teams { get; set; } = [];
    public List<ExpectedPlayerStats> Players { get; set; } = [];
}

public sealed class ExpectedTeamStats
{
    public int OldTeamId { get; set; }
    public int OldProjectTeamId { get; set; }
    public string Name { get; set; } = "";
    public Guid? NewTeamId { get; set; }
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public int Ties { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int Points { get; set; }
}

public sealed class ExpectedPlayerStats
{
    public int OldPersonId { get; set; }
    public int OldTeamId { get; set; }
    public string Name { get; set; } = "";
    public Guid? NewPlayerId { get; set; }
    public int GamesPlayed { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int PenaltyMinutes { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
}
