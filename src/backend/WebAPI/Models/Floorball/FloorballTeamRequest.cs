using System;
using System.ComponentModel.DataAnnotations;
using Domain.Enums.Floorball;
using Domain.Enums.Common;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for getting paginated floorball teams
    /// </summary>
    public record GetFloorballTeamsRequest
    {
        /// <summary>
        /// Gets the page number (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; init; } = 1;

        /// <summary>
        /// Gets the number of items per page (0 means use default)
        /// </summary>
        [Range(0, 100, ErrorMessage = "Page size must be between 0 and 100")]
        public int PageSize { get; init; } = 0;

        /// <summary>
        /// Gets the club ID filter
        /// </summary>
        public Guid? ClubId { get; init; }

        /// <summary>
        /// Gets the division filter
        /// </summary>
        public string? Division { get; init; }
    }

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
        public Guid DivisionId { get; set; }

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
        /// The logo URL of the team (optional)
        /// </summary>
        [Url(ErrorMessage = "Please provide a valid logo URL")]
        [StringLength(500, ErrorMessage = "Logo URL cannot exceed 500 characters")]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// The category of the team (Adult, Youth, Women)
        /// </summary>
        [Required(ErrorMessage = "Team category is required")]
        public TeamCategory Category { get; set; }
    }
    
    /// <summary>
    /// Request model for updating a player in a team
    /// </summary>
    public record UpdateFloorballTeamPlayerRequest
    {
        /// <summary>
        /// The position of the player
        /// </summary>
        [Required(ErrorMessage = "Position is required")]
        [EnumDataType(typeof(FloorballPosition), ErrorMessage = "Invalid position value")]
        public FloorballPosition Position { get; set; }
        
        /// <summary>
        /// The jersey number of the player
        /// </summary>
        [Required(ErrorMessage = "Jersey number is required")]
        [Range(0, 99, ErrorMessage = "Jersey number must be between 0 and 99")]
        public int JerseyNumber { get; set; }

        /// <summary>
        /// Whether the player is active
        /// </summary>
        [Required(ErrorMessage = "Active status is required")]
        public bool IsActive { get; set; }        
        
    }
} 
