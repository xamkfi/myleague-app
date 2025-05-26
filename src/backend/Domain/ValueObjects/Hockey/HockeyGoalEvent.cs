using System;
using Domain.Enums;

namespace Domain.ValueObjects.Hockey
{

    /// <summary>
    /// Represents a goal scored during a hockey match
    /// </summary>
    public class HockeyGoalEvent : HockeyMatchEventBase
    {
        /// <summary>
        /// Gets the ID of the player who scored the goal
        /// </summary>
        public Guid? ScoringPlayerId { get; private set; }

        /// <summary>
        /// Gets the ID of the player who primarily assisted the goal
        /// </summary>
        public Guid? PrimaryAssistingPlayerId { get; private set; }

        /// <summary>
        /// Gets the ID of the player who secondarily assisted the goal
        /// </summary>
        public Guid? SecondaryAssistingPlayerId { get; private set; }

        /// <summary>
        /// Gets the goal type ID
        /// </summary>
        public int? GoalTypeId { get; private set; }

        /// <summary>
        /// Private constructor for EF Core
        /// </summary>
        private HockeyGoalEvent() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the HockeyGoalEvent class
        /// </summary>
        /// <param name="matchId">The ID of the match</param>
        /// <param name="teamId">The ID of the scoring team</param>
        /// <param name="scoringPlayerId">The ID of the player who scored</param>
        /// <param name="primaryAssistingPlayerId">The ID of the primary assisting player</param>
        /// <param name="secondaryAssistingPlayerId">The ID of the secondary assisting player</param>
        /// <param name="periodNumber">The period number</param>
        /// <param name="timeInSeconds">The time in seconds</param>
        /// <param name="goalTypeId">The goal type ID</param>
        /// <param name="description">The description of the goal</param>
        public HockeyGoalEvent(
            Guid matchId,
            Guid teamId,
            Guid? scoringPlayerId,
            Guid? primaryAssistingPlayerId,
            Guid? secondaryAssistingPlayerId,
            int periodNumber,
            int timeInSeconds,
            int? goalTypeId = null,
            string? description = null)
            : base(matchId, teamId, periodNumber, timeInSeconds, description)
        {
            ScoringPlayerId = scoringPlayerId;
            PrimaryAssistingPlayerId = primaryAssistingPlayerId;
            SecondaryAssistingPlayerId = secondaryAssistingPlayerId;
            GoalTypeId = goalTypeId;
        }
    }
}
