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
        public DateTime? LastResumedAt { get; set; } // When timer was last resumed
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
                    return TimeSpan.Zero;

                DateTime now = DateTime.UtcNow;

                if (IsRunning)
                {
                    return (now - StartedAt.Value) - TotalPausedDuration;
                }
                else if (PausedAt.HasValue)
                {
                    return (PausedAt.Value - StartedAt.Value) - TotalPausedDuration;
                }
                else
                {
                    return TimeSpan.Zero;
                }
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
                return TimeSpan.Zero;

            if (IsRunning)
            {
                return (asOf - StartedAt.Value) - TotalPausedDuration;
            }
            else if (PausedAt.HasValue)
            {
                return (PausedAt.Value - StartedAt.Value) - TotalPausedDuration;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Starts or resumes the timer
        /// </summary>
        public void Start()
        {
            if (!StartedAt.HasValue)
                StartedAt = DateTime.UtcNow;
            LastResumedAt = DateTime.UtcNow;
            IsRunning = true;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Pauses the running timer
        /// </summary>
        public void Pause()
        {
            if (IsRunning && LastResumedAt.HasValue)
            {
                TotalPausedDuration += DateTime.UtcNow - LastResumedAt.Value;
                PausedAt = DateTime.UtcNow;
                IsRunning = false;
                LastUpdated = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Resets the timer state to initial
        /// </summary>
        public void Reset()
        {
            StartedAt = null;
            LastResumedAt = null;
            PausedAt = null;
            TotalPausedDuration = TimeSpan.Zero;
            IsRunning = false;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates internal timestamp for running timer
        /// </summary>
        public void Tick()
        {
            if (IsRunning)
            {
                LastUpdated = DateTime.UtcNow;
            }
        }
    }
} 