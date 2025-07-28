using System;

namespace Application.DTOs.Common
{
    /// <summary>
    /// Data transfer object for timer updates sent via SignalR
    /// </summary>
    public class TimerUpdate
    {
        /// <summary>
        /// The match ID
        /// </summary>
        public Guid MatchId { get; set; }

        /// <summary>
        /// The optional period number
        /// </summary>
        public int? PeriodNumber { get; set; }

        /// <summary>
        /// The calculated elapsed time
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// Whether the timer is currently running
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// When the timer was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// The type of timer event that occurred
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Creates a timer update for a started timer
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="periodNumber">The optional period number</param>
        /// <param name="elapsedTime">The elapsed time</param>
        /// <returns>A timer update for a started timer</returns>
        public static TimerUpdate CreateStarted(Guid matchId, int? periodNumber, TimeSpan elapsedTime)
        {
            return new TimerUpdate
            {
                MatchId = matchId,
                PeriodNumber = periodNumber,
                ElapsedTime = elapsedTime,
                IsRunning = true,
                LastUpdated = DateTime.UtcNow,
                EventType = "TimerStarted"
            };
        }

        /// <summary>
        /// Creates a timer update for a stopped timer
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="periodNumber">The optional period number</param>
        /// <param name="elapsedTime">The elapsed time</param>
        /// <returns>A timer update for a stopped timer</returns>
        public static TimerUpdate CreateStopped(Guid matchId, int? periodNumber, TimeSpan elapsedTime)
        {
            return new TimerUpdate
            {
                MatchId = matchId,
                PeriodNumber = periodNumber,
                ElapsedTime = elapsedTime,
                IsRunning = false,
                LastUpdated = DateTime.UtcNow,
                EventType = "TimerStopped"
            };
        }

        /// <summary>
        /// Creates a timer update for a reset timer
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="periodNumber">The optional period number</param>
        /// <returns>A timer update for a reset timer</returns>
        public static TimerUpdate CreateReset(Guid matchId, int? periodNumber)
        {
            return new TimerUpdate
            {
                MatchId = matchId,
                PeriodNumber = periodNumber,
                ElapsedTime = TimeSpan.Zero,
                IsRunning = false,
                LastUpdated = DateTime.UtcNow,
                EventType = "TimerReset"
            };
        }

        /// <summary>
        /// Creates a timer update for a periodic update
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="periodNumber">The optional period number</param>
        /// <param name="elapsedTime">The elapsed time</param>
        /// <param name="isRunning">Whether the timer is running</param>
        /// <returns>A timer update for a periodic update</returns>
        public static TimerUpdate CreateUpdate(Guid matchId, int? periodNumber, TimeSpan elapsedTime, bool isRunning)
        {
            return new TimerUpdate
            {
                MatchId = matchId,
                PeriodNumber = periodNumber,
                ElapsedTime = elapsedTime,
                IsRunning = isRunning,
                LastUpdated = DateTime.UtcNow,
                EventType = "TimerUpdate"
            };
        }
    }
} 