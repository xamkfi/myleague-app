using System;
using System.ComponentModel.DataAnnotations;
using Domain.Enums.Floorball;
using Domain.Enums.Common;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for creating or updating a floorball team
    /// </summary>
    public class FloorballTeamRequest
    {
        /// <summary>
        /// The name of the team
        /// </summary>
        [Required(ErrorMessage = "Team name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Team name must be between 2 and 100 characters")]
        public required string Name { get; set; }

        /// <summary>
        /// The division the team plays in
        /// </summary>
        [Required(ErrorMessage = "Division is required")]
        public FloorballDivision Division { get; set; }

        /// <summary>
        /// The ID of the club the team belongs to
        /// </summary>
        [Required(ErrorMessage = "Club ID is required")]
        public Guid ClubId { get; set; }

        /// <summary>
        /// The home arena of the team
        /// </summary>
        [Required(ErrorMessage = "Home arena is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Home arena must be between 2 and 200 characters")]
        public required string HomeArena { get; set; }

        /// <summary>
        /// The primary jersey color of the team
        /// </summary>
        [Required(ErrorMessage = "Primary jersey color is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Primary jersey color must be between 2 and 50 characters")]
        public required string PrimaryJerseyColor { get; set; }

        /// <summary>
        /// The secondary jersey color of the team (optional)
        /// </summary>
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Secondary jersey color must be between 2 and 50 characters")]
        public string? SecondaryJerseyColor { get; set; }

        /// <summary>
        /// The category of the team (Adult, Youth, Women)
        /// </summary>
        [Required(ErrorMessage = "Team category is required")]
        public TeamCategory Category { get; set; }
    }
} 