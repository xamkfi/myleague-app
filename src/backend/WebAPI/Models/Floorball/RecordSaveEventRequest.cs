using System;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for recording a save event
    /// </summary>
    public class RecordSaveEventRequest : FloorballMatchEventBaseRequest
    {
        /// <summary>
        /// ID of the goalie who made the save
        /// </summary>
        [Required]
        public Guid GoalieId { get; set; }
        // Optionally, shooter ID, etc. can be added here
    }
}
