using System;
using System.ComponentModel.DataAnnotations;
using Domain.Enums.Floorball;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for getting paginated floorball players
    /// </summary>
    public record GetFloorballPlayersRequest : PagedRequestBase
    {
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
    public record GetActiveFloorballPlayersRequest : PagedRequestBase
    {
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
    }
} 
