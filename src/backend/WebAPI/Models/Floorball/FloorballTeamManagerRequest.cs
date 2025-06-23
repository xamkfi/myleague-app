using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for floorball team manager operations
    /// </summary>
    public class FloorballTeamManagerRequest
    {
        /// <summary>
        /// Gets or sets the ID of the person who is the team manager
        /// </summary>
        [Required]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the team this manager is responsible for
        /// </summary>
        [Required]
        public Guid TeamId { get; set; }

        /// <summary>
        /// Gets or sets whether the team manager is active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
} 