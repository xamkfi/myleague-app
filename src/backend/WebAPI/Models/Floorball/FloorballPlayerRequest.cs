using System;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for creating a new floorball player
    /// </summary>
    public class CreateFloorballPlayerRequest
    {
        /// <summary>
        /// ID of the person who will be the player
        /// </summary>
        [Required(ErrorMessage = "Person ID is required")]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Player's jersey number
        /// </summary>
        [Required(ErrorMessage = "Jersey number is required")]
        [Range(1, 99, ErrorMessage = "Jersey number must be between 1 and 99")]
        public int JerseyNumber { get; set; }

        /// <summary>
        /// Player's position (e.g., "Forward", "Defense", "Goalkeeper")
        /// </summary>
        [Required(ErrorMessage = "Position is required")]
        public string Position { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for updating an existing floorball player
    /// </summary>
    public class UpdateFloorballPlayerRequest
    {
        /// <summary>
        /// Whether the player is currently active
        /// </summary>
        [Required(ErrorMessage = "Active status is required")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Player's jersey number
        /// </summary>
        [Required(ErrorMessage = "Jersey number is required")]
        [Range(1, 99, ErrorMessage = "Jersey number must be between 1 and 99")]
        public int JerseyNumber { get; set; }

        /// <summary>
        /// Player's position (e.g., "Forward", "Defense", "Goalkeeper")
        /// </summary>
        [Required(ErrorMessage = "Position is required")]
        public string Position { get; set; } = string.Empty;
    }
} 