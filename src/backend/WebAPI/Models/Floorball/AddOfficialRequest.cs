namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for adding an official to a floorball match
    /// </summary>
    public class AddOfficialRequest
    {
        /// <summary>
        /// Gets or sets the ID of the match
        /// </summary>
        public Guid MatchId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the referee (official)
        /// </summary>
        public Guid RefereeId { get; set; }
    }
} 