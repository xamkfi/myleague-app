using System;
using System.ComponentModel.DataAnnotations;
using Domain.Enums.Floorball;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for creating a floorball season
    /// </summary>
    public class CreateFloorballSeasonRequest
    {
        /// <summary>
        /// Name of the season
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Start date of the season
        /// </summary>
        [Required]
        public string StartDate { get; set; } = string.Empty;

        /// <summary>
        /// End date of the season
        /// </summary>
        [Required]
        public string EndDate { get; set; } = string.Empty;

        /// <summary>
        /// List of division IDs to associate with this season. At least one division must be provided.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one division must be specified.")]
        public List<Guid> DivisionIds { get; set; } = new();
    }

    /// <summary>
    /// Request model for updating a floorball season
    /// </summary>
    public class UpdateFloorballSeasonRequest
    {
        /// <summary>
        /// Name of the season
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Start date of the season
        /// </summary>
        [Required]
        public string StartDate { get; set; } = string.Empty;

        /// <summary>
        /// End date of the season
        /// </summary>
        [Required]
        public string EndDate { get; set; } = string.Empty;

    }
} 
