namespace MyLeague.Infrastructure.SignalR.Sports.Floorball
{
    /// <summary>
    /// Contains constants for Floorball SignalR notification event names
    /// </summary>
    public static class FloorballNotificationEvents
    {
        public const string PlayerAddedToTeam = "FloorballPlayerAddedToTeam";
        public const string GoalScored = "FloorballGoalScored";
        public const string SaveRecorded = "FloorballSaveRecorded";
        public const string MatchAddedToSeason = "FloorballMatchAddedToSeason";
        public const string MatchCompleted = "FloorballMatchCompleted";
        public const string MatchCreated = "FloorballMatchCreated";
        public const string OfficialAssigned = "FloorballOfficialAssigned";
        public const string MatchStarted = "FloorballMatchStarted";
        public const string PlayerPositionUpdated = "FloorballPlayerPositionUpdated";
        public const string PenaltyAssigned = "FloorballPenaltyAssigned";
        public const string PlayerRegistered = "FloorballPlayerRegistered";
        public const string PlayerRemovedFromTeam = "FloorballPlayerRemovedFromTeam";
        public const string SeasonCompleted = "FloorballSeasonCompleted";
        public const string SeasonDetailsUpdated = "FloorballSeasonDetailsUpdated";
        public const string SeasonDivisionUpdated = "FloorballSeasonDivisionUpdated";
        public const string SeasonDeactivated = "FloorballSeasonDeactivated";
        public const string TeamRemoved = "FloorballTeamRemoved";
        public const string TeamAddedToSeason = "FloorballTeamAddedToSeason";
        public const string TeamRemovedFromSeason = "FloorballTeamRemovedFromSeason";
        public const string SeasonDateRangeUpdated = "FloorballSeasonDateRangeUpdated";
        public const string SeasonActivated = "FloorballSeasonActivated";
        public const string PlayerStatUpdated = "FloorballPlayerStatUpdated";
    }
} 