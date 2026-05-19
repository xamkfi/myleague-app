namespace Application.Constants
{
    public static class FloorballNotificationEvents
    {
        // Match lifecycle
        public const string MatchCreated = "FloorballMatchCreated";
        public const string MatchStarted = "FloorballMatchStarted";
        public const string MatchCompleted = "FloorballMatchCompleted";
        public const string MatchAddedToCompetition = "FloorballMatchAddedToCompetition";

        // In-match events
        public const string GoalScored = "FloorballGoalScored";
        public const string SaveRecorded = "FloorballSaveRecorded";
        public const string PenaltyAssigned = "FloorballPenaltyAssigned";

        // Player events
        public const string PlayerRegistered = "FloorballPlayerRegistered";
        public const string PlayerAddedToTeam = "FloorballPlayerAddedToTeam";
        public const string PlayerRemovedFromTeam = "FloorballPlayerRemovedFromTeam";
        public const string PlayerPositionUpdated = "FloorballPlayerPositionUpdated";
        public const string PlayerStatUpdated = "FloorballPlayerStatUpdated";

        // Team events
        public const string TeamAddedToCompetition = "FloorballTeamAddedToCompetition";
        public const string TeamRemovedFromCompetition = "FloorballTeamRemovedFromCompetition";
        public const string TeamRemoved = "FloorballTeamRemoved";

        // Official events
        public const string OfficialAssigned = "FloorballOfficialAssigned";

        // Competition events
        public const string CompetitionActivated = "FloorballCompetitionActivated";
        public const string CompetitionDeactivated = "FloorballCompetitionDeactivated";
        public const string CompetitionCompleted = "FloorballCompetitionCompleted";
        public const string CompetitionDetailsUpdated = "FloorballCompetitionDetailsUpdated";
        public const string CompetitionDivisionUpdated = "FloorballCompetitionDivisionUpdated";
        public const string CompetitionDateRangeUpdated = "FloorballCompetitionDateRangeUpdated";
    }
}
