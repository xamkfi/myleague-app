namespace MyLeague.Infrastructure.SignalR.Sports.Floorball
{
    /// <summary>
    /// Contains constants for Floorball SignalR notification event names
    /// </summary>
    public static class FloorballNotificationEvents
    {
        public const string PlayerAddedToTeam = "FloorballPlayerAddedToTeam";
        public const string GoalScored = "FloorballGoalScored";
        public const string MatchAddedToSeason = "FloorballMatchAddedToSeason";
        public const string MatchCompleted = "FloorballMatchCompleted";
        public const string MatchCreated = "FloorballMatchCreated";
        public const string OfficialAssigned = "FloorballOfficialAssigned";
        public const string MatchStarted = "FloorballMatchStarted";
    }
} 