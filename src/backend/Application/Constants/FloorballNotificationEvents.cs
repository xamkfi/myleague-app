namespace Application.Constants
{
    public static class FloorballNotificationEvents
    {
        // Match lifecycle
        public const string MatchCreated = "FloorballMatchCreated";
        public const string MatchStarted = "FloorballMatchStarted";
        public const string MatchCompleted = "FloorballMatchCompleted";
        public const string MatchAddedToSeason = "FloorballMatchAddedToSeason";

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
        public const string TeamAddedToSeason = "FloorballTeamAddedToSeason";
        public const string TeamRemovedFromSeason = "FloorballTeamRemovedFromSeason";
        public const string TeamRemoved = "FloorballTeamRemoved";

        // Official events
        public const string OfficialAssigned = "FloorballOfficialAssigned";

        // Season events
        public const string SeasonActivated = "FloorballSeasonActivated";
        public const string SeasonDeactivated = "FloorballSeasonDeactivated";
        public const string SeasonCompleted = "FloorballSeasonCompleted";
        public const string SeasonDetailsUpdated = "FloorballSeasonDetailsUpdated";
        public const string SeasonDivisionUpdated = "FloorballSeasonDivisionUpdated";
        public const string SeasonDateRangeUpdated = "FloorballSeasonDateRangeUpdated";
    }
}
