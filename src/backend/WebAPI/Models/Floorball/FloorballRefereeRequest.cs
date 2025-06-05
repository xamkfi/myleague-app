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
        public DateTime LicenseIssueDate { get; set; }

        /// <summary>
        /// Gets or sets the license expiry date
        /// </summary>
        [Required(ErrorMessage = "License expiry date is required")]
        public DateTime LicenseExpiryDate { get; set; }
    }

    /// <summary>
    /// Request model for updating a floorball referee
    /// </summary>
    public class UpdateFloorballRefereeRequest
    {
        /// <summary>
        /// Gets or sets the license issue date
        /// </summary>
        public DateTime? LicenseIssueDate { get; set; }

        /// <summary>
        /// Gets or sets the license expiry date
        /// </summary>
        public DateTime? LicenseExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the license level
        /// </summary>
        [Required(ErrorMessage = "License level is required")]
        public int LicenseLevel { get; set; }

        /// <summary>
        /// Gets or sets whether the referee is active
        /// </summary>
        [Required(ErrorMessage = "IsActive status is required")]
        public bool IsActive { get; set; }
    }
} 