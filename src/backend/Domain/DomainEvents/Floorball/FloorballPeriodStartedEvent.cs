// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DomainEvents.Floorball
{
    internal class FloorballPeriodStartedEvent : FloorballDomainEvent
    {
        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get; }

        /// <summary>
        /// Gets the number of the period that ended
        /// </summary>
        public int PeriodNumber { get; }

        /// <summary>
        /// Gets the home team's score at the end of the period
        /// </summary>
        public int HomeTeamScore { get; }

        /// <summary>
        /// Gets the away team's score at the end of the period
        /// </summary>
        public int AwayTeamScore { get; }

        /// <summary>
        /// Gets whether this was the last period of regular time
        /// </summary>
        public bool IsLastRegularPeriod { get; }

        /// <summary>
        /// Initializes a new instance of the FloorballPeriodEndedEvent class
        /// </summary>
        public FloorballPeriodStartedEvent(
            Guid matchId,
            int periodNumber,
            int homeTeamScore,
            int awayTeamScore,
            bool isLastRegularPeriod)
        {
            if (periodNumber < 1)
            {
                throw new ArgumentException("Period number must be positive", nameof(periodNumber));
            }

            if (homeTeamScore < 0 || awayTeamScore < 0)
            {
                throw new ArgumentException("Scores cannot be negative", nameof(homeTeamScore));
            }

            MatchId = matchId;
            PeriodNumber = periodNumber;
            HomeTeamScore = homeTeamScore;
            AwayTeamScore = awayTeamScore;
            IsLastRegularPeriod = isLastRegularPeriod;
        }
    }
}
