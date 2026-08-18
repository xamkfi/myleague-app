namespace Application.Constants;

public static class FootballNotificationEvents
{
    public const string MatchCreated = "FootballMatchCreated";
    public const string MatchStarted = "FootballMatchStarted";
    public const string MatchCompleted = "FootballMatchCompleted";
    public const string MatchReopened = "FootballMatchReopened";
    public const string MatchAddedToCompetition = "FootballMatchAddedToCompetition";

    public const string GoalScored = "FootballGoalScored";
    public const string CardAssigned = "FootballCardAssigned";
    public const string SubstitutionRecorded = "FootballSubstitutionRecorded";

    public const string PlayerRegistered = "FootballPlayerRegistered";
    public const string PlayerAddedToTeam = "FootballPlayerAddedToTeam";
    public const string PlayerRemovedFromTeam = "FootballPlayerRemovedFromTeam";
    public const string PlayerPositionUpdated = "FootballPlayerPositionUpdated";
    public const string PlayerStatUpdated = "FootballPlayerStatUpdated";

    public const string TeamAddedToCompetition = "FootballTeamAddedToCompetition";
    public const string TeamRemovedFromCompetition = "FootballTeamRemovedFromCompetition";
    public const string TeamRemoved = "FootballTeamRemoved";

    public const string OfficialAssigned = "FootballOfficialAssigned";

    public const string CompetitionActivated = "FootballCompetitionActivated";
    public const string CompetitionDeactivated = "FootballCompetitionDeactivated";
    public const string CompetitionCompleted = "FootballCompetitionCompleted";
    public const string CompetitionDetailsUpdated = "FootballCompetitionDetailsUpdated";
    public const string CompetitionDivisionUpdated = "FootballCompetitionDivisionUpdated";
    public const string CompetitionDateRangeUpdated = "FootballCompetitionDateRangeUpdated";
}
