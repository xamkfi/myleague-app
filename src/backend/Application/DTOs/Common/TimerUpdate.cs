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
        /// The calculated elapsed time as a formatted string
        /// </summary>
        public string ElapsedTime { get; set; } = string.Empty;

        /// <summary>
        /// The elapsed time in milliseconds for precise client handling
        /// </summary>
        public long ElapsedMilliseconds { get; set; }

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
        /// Monotonically increasing sequence number per match (set by publisher)
        /// </summary>
        public long Sequence { get; set; }

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
                ElapsedTime = elapsedTime.ToString(@"hh\:mm\:ss"),
                ElapsedMilliseconds = (long)elapsedTime.TotalMilliseconds,
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
                ElapsedTime = elapsedTime.ToString(@"hh\:mm\:ss"),
                ElapsedMilliseconds = (long)elapsedTime.TotalMilliseconds,
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
                ElapsedTime = TimeSpan.Zero.ToString(@"hh\:mm\:ss"),
                ElapsedMilliseconds = 0,
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
                ElapsedTime = elapsedTime.ToString(@"hh\:mm\:ss"),
                ElapsedMilliseconds = (long)elapsedTime.TotalMilliseconds,
                IsRunning = isRunning,
                LastUpdated = DateTime.UtcNow,
                EventType = "TimerUpdate"
            };
        }
    }
} 