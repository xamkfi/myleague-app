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
        /// Gets or sets the primary responsibility of the team manager
        /// </summary>
        public string? PrimaryResponsibility { get; set; }

        /// <summary>
        /// Gets or sets the years of experience of the team manager
        /// </summary>
        [Required]
        [Range(0, 100)]
        public int YearsOfExperience { get; set; }

        /// <summary>
        /// Gets or sets whether the team manager is active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
} 