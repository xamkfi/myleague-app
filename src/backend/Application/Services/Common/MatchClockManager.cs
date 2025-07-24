// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Common
{
    using System.Diagnostics;

    public class MatchClockManager : IMatchClockManager
    {
        private readonly Dictionary<Guid, Stopwatch> _matchTimers = new();
        private readonly object _lock = new();

        public void Start(Guid matchId)
        {
            lock (_lock)
            {
                if (!_matchTimers.TryGetValue(matchId, out Stopwatch? stopwatch))
                {
                    stopwatch = new Stopwatch();
                    _matchTimers[matchId] = stopwatch;
                }

                if (!stopwatch.IsRunning)
                {
                    stopwatch.Start();
                }
            }
        }

        public void Stop(Guid matchId)
        {
            lock (_lock)
            {
                if (_matchTimers.TryGetValue(matchId, out Stopwatch? stopwatch) && stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
            }
        }

        public void Reset(Guid matchId)
        {
            lock (_lock)
            {
                if (_matchTimers.TryGetValue(matchId, out Stopwatch? stopwatch))
                {
                    stopwatch.Reset();
                }
            }
        }

        public TimeSpan GetElapsedTime(Guid matchId)
        {
            lock (_lock)
            {
                return _matchTimers.TryGetValue(matchId, out Stopwatch? stopwatch)
                    ? stopwatch.Elapsed
                    : TimeSpan.Zero;
            }
        }

        public bool IsRunning(Guid matchId)
        {
            lock (_lock)
            {
                return _matchTimers.TryGetValue(matchId, out Stopwatch? stopwatch) && stopwatch.IsRunning;
            }
        }

        public bool Exists(Guid matchId)
        {
            lock (_lock)
            {
                return _matchTimers.ContainsKey(matchId);
            }
        }
    }

}
