// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Common
{
    public interface IMatchClockManager
    {
        void Start(Guid matchId);
        void Stop(Guid matchId);
        void Reset(Guid matchId);
        TimeSpan GetElapsedTime(Guid matchId);
        bool IsRunning(Guid matchId);
        bool Exists(Guid matchId);
    }
}
