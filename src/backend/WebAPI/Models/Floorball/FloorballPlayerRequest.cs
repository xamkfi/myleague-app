using System;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for getting paginated floorball players
    /// </summary>
    public record GetFloorballPlayersRequest
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
        /// Gets the active status filter (null = all, true = active only, false = inactive only)
        /// </summary>
        public bool? IsActive { get; init; }

        /// <summary>
        /// Gets the position filter
        /// </summary>
        public string? Position { get; init; }

        /// <summary>
        /// Gets the team ID filter
        /// </summary>
        public Guid? TeamId { get; init; }

        /// <summary>
        /// Gets the search term for filtering by player name
        /// </summary>
        public string? SearchTerm { get; init; }
    }

    /// <summary>
    /// Request model for getting paginated active floorball players
    /// </summary>
    public record GetActiveFloorballPlayersRequest
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
        /// Gets the position filter
        /// </summary>
        public string? Position { get; init; }

        /// <summary>
        /// Gets the team ID filter
        /// </summary>
        public Guid? TeamId { get; init; }
    }

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