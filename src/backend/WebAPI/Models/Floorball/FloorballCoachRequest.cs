using System;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for creating a new floorball coach
    /// </summary>
    public class CreateFloorballCoachRequest
    {
        /// <summary>
        /// ID of the person who will be the coach
        /// </summary>
        [Required(ErrorMessage = "Person ID is required")]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Number of years the coach has been coaching
        /// </summary>
        [Required(ErrorMessage = "Years of experience is required")]
        [Range(0, 100, ErrorMessage = "Years of experience must be between 0 and 100")]
        public int YearsOfExperience { get; set; }

        /// <summary>
        /// Coach's certification level (e.g., "Level 1", "Level 2", etc.)
        /// </summary>
        public string? CertificationLevel { get; set; }

        /// <summary>
        /// Coach's area of specialization (e.g., "Offense", "Defense", "Goalkeeper")
        /// </summary>
        public string? Specialization { get; set; }
    }

    /// <summary>
    /// Request model for updating an existing floorball coach
    /// </summary>
    public class UpdateFloorballCoachRequest
    {
        /// <summary>
        /// Whether the coach is currently active
        /// </summary>
        [Required(ErrorMessage = "Active status is required")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Number of years the coach has been coaching
        /// </summary>
        [Required(ErrorMessage = "Years of experience is required")]
        [Range(0, 100, ErrorMessage = "Years of experience must be between 0 and 100")]
        public int YearsOfExperience { get; set; }

        /// <summary>
        /// Coach's certification level (e.g., "Level 1", "Level 2", etc.)
        /// </summary>
        public string? CertificationLevel { get; set; }

        /// <summary>
        /// Coach's area of specialization (e.g., "Offense", "Defense", "Goalkeeper")
        /// </summary>
        public string? Specialization { get; set; }
    }
} 