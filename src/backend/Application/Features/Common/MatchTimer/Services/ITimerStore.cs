using System;
using System.Collections.Generic;
using Domain.Entities.Common;

namespace Application.Services.Common
{
    /// <summary>
    /// Abstract store for active match timers
    /// </summary>
    public interface ITimerStore
    {
        bool Add(TimerState timer);
        bool TryRemove(Guid matchId, out TimerState? timer);
        bool TryGet(Guid matchId, out TimerState? timer);
        IEnumerable<TimerState> GetAll();
        IEnumerable<TimerState> GetActive();
    }
}