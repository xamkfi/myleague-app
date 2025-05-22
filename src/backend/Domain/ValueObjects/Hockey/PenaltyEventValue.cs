
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums.Hockey;

namespace Domain.ValueObjects.Hockey
{
    /// <summary>
    /// Represents a penalty given during a hockey match
    /// </summary>
    public class PenaltyEventValue : MatchEventBaseValue
    {
        /// <summary>
        /// Gets the ID of the player who received the penalty
        /// </summary>
        public Guid PlayerId { get; private set; }

        /// <summary>
        /// Gets the type of the penalty
        /// </summary>
        public HockeyPenaltyType PenaltyType { get; private set; }

        /// <summary>
        /// Gets the duration of the penalty
        /// </summary>
        public int DurationInMinutes { get; private set; }

        /// <summary>
        /// Private contstructor for EF Core
        /// </summary>
        private PenaltyEventValue() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the PenaltyEventValue class
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="teamId"></param>
        /// <param name="playerId"></param>
        /// <param name="penaltyType"></param>
        /// <param name="durationInMinutes"></param>
        /// <param name="periodNumber"></param>
        /// <param name="timeInSeconds"></param>
        /// <param name="description"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public PenaltyEventValue(
            Guid matchId,
            Guid teamId,
            Guid playerId,
            HockeyPenaltyType penaltyType,
            int durationInMinutes,
            int periodNumber,
            int timeInSeconds,
            string? description = null)
            : base(matchId, teamId, periodNumber, timeInSeconds, description)
        {
            if (durationInMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(durationInMinutes), "Penalty duration must be positive.");

            PlayerId = playerId;
            PenaltyType = penaltyType;
            DurationInMinutes = durationInMinutes;
        }

            
    }
}
