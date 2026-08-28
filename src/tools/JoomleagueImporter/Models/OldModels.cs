namespace JoomleagueImporter.Models;

public class OldClub
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Location { get; init; } = "";
    public string Website { get; init; } = "";
}

public class OldTeam
{
    public int Id { get; init; }
    public int? ClubId { get; init; }
    public string Name { get; init; } = "";
    public string ShortName { get; init; } = "";
}

public class OldPerson
{
    public int Id { get; init; }
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public DateTime? Birthday { get; init; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class OldProject
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public DateTime? StartDate { get; init; }
    public int GameRegularTime { get; init; } = 30;
    public int GameParts { get; init; } = 2;
    public int? SeasonId { get; init; }
    public string? Description { get; init; }
    public string? ProjectInfo { get; init; }
    public string? Extension { get; init; }
    public string? Extended { get; init; }
    public string? SeasonExtended { get; set; }

    public int NumberOfPeriods => GameParts > 0 ? GameParts : 2;
    public int PeriodDurationMinutes
    {
        get
        {
            int minutes = GameParts > 0 ? GameRegularTime / GameParts : 15;
            return minutes > 0 ? minutes : 15;
        }
    }
}

public class OldProjectTeam
{
    public int Id { get; init; }
    public int ProjectId { get; init; }
    public int TeamId { get; init; }
}

public class OldTeamPlayer
{
    public int Id { get; init; }
    public int ProjectTeamId { get; init; }
    public int PersonId { get; init; }
    public int? ProjectPositionId { get; init; }
    public int? JerseyNumber { get; init; }
}

public class OldRound
{
    public int Id { get; init; }
    public int ProjectId { get; init; }
}

public class OldMatch
{
    public int Id { get; init; }
    public int RoundId { get; init; }
    public int ProjectTeam1Id { get; init; }
    public int ProjectTeam2Id { get; init; }
    public int? PlaygroundId { get; init; }
    public DateTime? MatchDate { get; init; }
    public int? Team1Result { get; init; }
    public int? Team2Result { get; init; }
    public bool Cancelled { get; init; }

    public bool HasResult => Team1Result.HasValue && Team2Result.HasValue;
}

public class OldMatchEvent
{
    public int Id { get; init; }
    public int MatchId { get; init; }
    public int ProjectTeamId { get; init; }
    public int TeamPlayerId { get; init; }
    public string EventTime { get; init; } = "";
    public int EventTypeId { get; init; }
    public int Count { get; init; } = 1;
}
