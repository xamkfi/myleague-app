using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Common;
using Application.Services.Common;

namespace MyLeague.Infrastructure.Services.Common
{
    /// <summary>
    /// In-memory store for active match timers
    /// </summary>
    public class InMemoryTimerStore : ITimerStore
    {
        private readonly ConcurrentDictionary<Guid, TimerState> _timers = new ConcurrentDictionary<Guid, TimerState>();

        /// <summary>
        /// Adds a timer to the store
        /// </summary>
        public bool Add(TimerState timer)
        {
            return _timers.TryAdd(timer.MatchId, timer);
        }

        /// <summary>
        /// Removes and returns a timer from the store by match ID
        /// </summary>
        public bool TryRemove(Guid matchId, out TimerState? timer)
        {
            return _timers.TryRemove(matchId, out timer);
        }

        /// <summary>
        /// Retrieves a timer by match ID
        /// </summary>
        public bool TryGet(Guid matchId, out TimerState? timer)
        {
            return _timers.TryGetValue(matchId, out timer);
        }

        /// <summary>
        /// Returns all timers in the store
        /// </summary>
        public IEnumerable<TimerState> GetAll()
        {
            return _timers.Values;
        }

        /// <summary>
        /// Returns only the active (running) timers
        /// </summary>
        public IEnumerable<TimerState> GetActive()
        {
            return _timers.Values.Where(t => t.IsRunning);
        }
    }
}