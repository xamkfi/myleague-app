using System;

namespace Domain.Entities.Common
{
    /// <summary>
    /// Represents the state of a match timer with calculated elapsed time
    /// </summary>
    public class TimerState
    {
        public Guid MatchId { get; set; }
        public int? PeriodNumber { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? PausedAt { get; set; }
        public TimeSpan TotalPausedDuration { get; set; }
        public bool IsRunning { get; set; }
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Calculates the elapsed time based on stored state
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get
            {
                if (!StartedAt.HasValue)
                {
                    return TimeSpan.Zero;
                }

                DateTime now = DateTime.UtcNow;
                TimeSpan totalRunningTime = IsRunning
                ? now - StartedAt.Value
                : (PausedAt ?? now) - StartedAt.Value;

                return totalRunningTime - TotalPausedDuration;
            }
        }

        /// <summary>
        /// Calculates the elapsed time as of a specific point in time
        /// </summary>
        /// <param name="asOf">The point in time to calculate elapsed time for</param>
        /// <returns>The calculated elapsed time as of the specified time</returns>
        public TimeSpan GetElapsedTimeAsOf(DateTime asOf)
        {
            if (!StartedAt.HasValue)
            {
                return TimeSpan.Zero;
            }

            TimeSpan totalRunningTime = IsRunning 
                ? asOf - StartedAt.Value 
                : (PausedAt ?? asOf) - StartedAt.Value;

            return totalRunningTime - TotalPausedDuration;
        }
    }
} 