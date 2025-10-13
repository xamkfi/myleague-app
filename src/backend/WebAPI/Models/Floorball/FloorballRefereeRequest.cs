using System;
using System.ComponentModel.DataAnnotations;
using WebAPI.Models.Common;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for creating a floorball referee
    /// </summary>
    public class CreateFloorballRefereeRequest
    {
        /// <summary>
        /// Gets or sets the person ID associated with the referee
        /// </summary>
        [Required(ErrorMessage = "Person ID is required")]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the license issue date
        /// </summary>
        [Required(ErrorMessage = "License issue date is required")]
        public string LicenseIssueDate { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the license expiry date
        /// </summary>
        [Required(ErrorMessage = "License expiry date is required")]
        public string LicenseExpiryDate { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for updating a floorball referee
    /// </summary>
    public class UpdateFloorballRefereeRequest
    {
        /// <summary>
        /// Gets or sets the license issue date
        /// </summary>
        public string? LicenseIssueDate { get; set; }

        /// <summary>
        /// Gets or sets the license expiry date
        /// </summary>
        public string? LicenseExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the number of matches officiated
        /// </summary>
        [Required(ErrorMessage = "Matches officiated is required")]
        public int MatchesOfficiated { get; set; }

        /// <summary>
        /// Gets or sets whether the referee is active
        /// </summary>
        [Required(ErrorMessage = "IsActive status is required")]
        public bool IsActive { get; set; }
    }
} 