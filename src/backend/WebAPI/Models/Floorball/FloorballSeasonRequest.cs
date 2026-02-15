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

        /// <summary>
        /// Number of regular periods (e.g., 2 or 3). Default: 2.
        /// </summary>
        public int NumberOfPeriods { get; set; } = 2;

        /// <summary>
        /// Duration in minutes per regular period. Default: 15.
        /// </summary>
        public int PeriodDurationMinutes { get; set; } = 15;

        /// <summary>
        /// Whether overtime is allowed when the match is tied. Default: true.
        /// </summary>
        public bool AllowOvertime { get; set; } = true;

        /// <summary>
        /// Duration in minutes for the overtime period. Default: 5.
        /// </summary>
        public int OvertimeDurationMinutes { get; set; } = 5;

        /// <summary>
        /// Whether shootout is allowed after overtime. Default: true.
        /// </summary>
        public bool AllowShootout { get; set; } = true;
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

        /// <summary>
        /// Number of regular periods (e.g., 2 or 3). Default: 2.
        /// </summary>
        public int NumberOfPeriods { get; set; } = 2;

        /// <summary>
        /// Duration in minutes per regular period. Default: 15.
        /// </summary>
        public int PeriodDurationMinutes { get; set; } = 15;

        /// <summary>
        /// Whether overtime is allowed when the match is tied. Default: true.
        /// </summary>
        public bool AllowOvertime { get; set; } = true;

        /// <summary>
        /// Duration in minutes for the overtime period. Default: 5.
        /// </summary>
        public int OvertimeDurationMinutes { get; set; } = 5;

        /// <summary>
        /// Whether shootout is allowed after overtime. Default: true.
        /// </summary>
        public bool AllowShootout { get; set; } = true;
    }
} 
