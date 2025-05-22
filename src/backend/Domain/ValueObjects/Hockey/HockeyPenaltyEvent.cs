using System;
using Domain.Enums;
using Domain.Enums.Hockey;

namespace Domain.ValueObjects.Hockey
{
    /// <summary>
    /// Represents a penalty given during a hockey match
    /// </summary>
    public class HockeyPenaltyEvent : HockeyMatchEventBase
    {
        /// <summary>
        /// Gets the ID of the player who received the penalty
        /// </summary>
        public Guid? PlayerId { get; private set; }

        /// <summary>
        /// Gets the penalty type ID
        /// </summary>
        public int PenaltyTypeId { get; private set; }

        /// <summary>
        /// Gets the penalty minutes
        /// </summary>
        public int PenaltyMinutes { get; private set; }

        /// <summary>
        /// Private constructor for EF Core
        /// </summary>
        private HockeyPenaltyEvent() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the HockeyPenaltyEvent class
        /// </summary>
        /// <param name="matchId">The ID of the match</param>
        /// <param name="teamId">The ID of the penalized team</param>
        /// <param name="playerId">The ID of the penalized player</param>
        /// <param name="penaltyType">The type of penalty</param>
        /// <param name="penaltyMinutes">The penalty minutes</param>
        /// <param name="periodNumber">The period number</param>
        /// <param name="timeInSeconds">The time when the penalty occurred</param>
        /// <param name="description">Penalty description</param>
        public HockeyPenaltyEvent(
            Guid matchId,
            Guid teamId,
            Guid? playerId,
            HockeyPenaltyType penaltyType,
            int penaltyMinutes,
            int periodNumber,
            int timeInSeconds,
            string? description = null)
            : base(matchId, teamId, periodNumber, timeInSeconds, description)
        {
            PlayerId = playerId;
            PenaltyTypeId = (int)penaltyType;
            PenaltyMinutes = penaltyMinutes;
        }
    }
}
