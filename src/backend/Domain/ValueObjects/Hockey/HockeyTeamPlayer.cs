// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums.Hockey;

namespace Domain.ValueObjects.Hockey
{
    public class HockeyTeamPlayer
    {
        /// <summary>
        /// Gets the ID of the team
        /// </summary>
        public Guid TeamId { get; private set; }

        /// <summary>
        /// Gets the ID of the player
        /// </summary>
        public Guid PlayerId { get; private set; }

        /// <summary>
        /// Gets the player's position in the team
        /// </summary>
        public HockeyPosition Position { get; private set; }

        /// <summary>
        /// Gets whether the player is currently active in the team
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets the number of the player in this team
        /// </summary>
        public int? JerseyNumber { get; private set; }

        /// <summary>
        /// Gets the number of games played for this team
        /// </summary>
        public int GamesPlayed { get; private set; }

        /// <summary>
        /// Gets the number of goals scored for this team
        /// </summary>
        public int Goals {  get; private set; }

        /// <summary>
        /// Gets the number of saves made for this team
        /// </summary>
        public int Saves { get; private set; }

        /// <summary>
        /// Gets the number of assists made for this team
        /// </summary>
        public int Assists { get; private set; }

        /// <summary>
        /// Gets the number of penalty minutes for this team
        /// </summary>
        public int PenaltyMinutes { get; private set; }

        /// <summary>
        /// Private constructor for EF Core
        /// </summary>
        private HockeyTeamPlayer()
        {
            IsActive = true;
            GamesPlayed = 0;
            Goals = 0;
            Saves = 0;
            Assists = 0;
            PenaltyMinutes = 0;
        }

        /// <summary>
        /// Initializes a new instance of the HockeyTeamPlayer class
        /// </summary>
        /// <param name="teamId"> The ID of the team</param>
        /// <param name="playerId">The ID of the player</param>
        /// <param name="position">The player's position in the team</param>
        /// <param name="jerseyNumber">The player's jersey number in this team</param>
        public HockeyTeamPlayer(
            Guid teamId,
            Guid playerId,
            HockeyPosition position,
            int? jerseyNumber = null)
        {
            TeamId = teamId;
            PlayerId = playerId;
            Position = position;
            JerseyNumber = jerseyNumber;
            IsActive = true;
            GamesPlayed = 0;
            Goals = 0;
            Saves = 0;
            Assists = 0;
            PenaltyMinutes = 0;
        }

        /// <summary>
        /// Updates the player's position in the team
        /// </summary>
        /// <param name="newPosition">The new position</param>
        public void UpdatePosition(HockeyPosition newPosition)
        {
            Position = newPosition;
        }

        /// <summary>
        /// Updates the player's active status in the team
        /// </summary>
        /// <param name="isActive">The new active status</param>
        public void SetActiveStatus(bool isActive)
        {
            IsActive = isActive;
        }

        /// <summary>
        /// Updates the player's jersey number in this team
        /// </summary>
        /// <param name="jerseyNumber">The new jersey number</param>
        public void UpdateJerseyNumber(int? jerseyNumber)
        {
            JerseyNumber = jerseyNumber;
        }

        /// <summary>
        /// Records a game played by this player
        /// </summary>
        public void RecordGamePlayed()
        {
            GamesPlayed++;
        }

        /// <summary>
        /// Records a goal scored by this player
        /// </summary>
        public void RecordGoal()
        {
            Goals++;
        }

        /// <summary>
        /// Records a save made
        /// </summary>
        public void RecordSave()
        {
            Saves++;
        }

        /// <summary>
        /// Records an assist made by this player
        /// </summary>
        public void RecordAssist()
        {
            Assists++;
        }

        /// <summary>
        /// Records penalty minutes for this player
        /// </summary>
        /// <param name="minutes">The number of penalty minutes to add</param>

        public void RecordPenaltyMinutes(int minutes)
        {
            if (minutes < 0)
                throw new ArgumentException("Penalty minutes cannot be negative", nameof(minutes));

            PenaltyMinutes += minutes;
        }
    }
}
