using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request body for replacing a tournament's pre-defined playoff schedule. The slot list is
    /// authoritative: any slots not present in the request are removed. Pass an empty list to
    /// clear the schedule entirely.
    /// </summary>
    public class UpdatePlayoffScheduleRequest
    {
        /// <summary>
        /// Full set of playoff schedule slots that should remain on the tournament after the
        /// update. Same shape as <see cref="PlayoffScheduleSlotRequest"/> used by create/update.
        /// </summary>
        [Required]
        public List<PlayoffScheduleSlotRequest> Slots { get; set; } = new();
    }
}
