using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Enums.Floorball;
using Domain.Enums.Common;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// Request model for getting paginated floorball teams
    /// </summary>
    public record GetFloorballTeamsRequest : PagedRequestBase
    {
        /// <summary>
        /// Gets the club ID filter
        /// </summary>
        public Guid? ClubId { get; init; }

        /// <summary>
        /// Gets the division filter
        /// </summary>
        public string? Division { get; init; }

        /// <summary>
        /// Gets the audience / age-group category filter. Accepts one or more values
        /// (e.g. ?teamCategory=Adult&amp;teamCategory=Women).
        /// </summary>
        [FromQuery(Name = "teamCategory")]
        public List<TeamCategory>? TeamCategories { get; init; }
    }

    /// <summary>
    /// Request model for getting paginated floorball teams without roster
    /// </summary>
    public record GetAllTeamsWithoutRosterRequest : PagedRequestBase
    {
        /// <summary>
        /// Gets the search term filter
        /// </summary>
        public string? SearchTerm { get; init; }

        /// <summary>
        /// Gets the team category filter
        /// </summary>
        public TeamCategory? TeamCategory { get; init; }
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
        public Guid? DivisionId { get; set; }

        /// <summary>
        /// The ID of the club the team belongs to
        /// </summary>
        [Required(ErrorMessage = "Club ID is required")]
        public Guid ClubId { get; set; }

        /// <summary>
        /// The home arena of the team (optional). When omitted the team is stored without a home
        /// arena — useful for tournament-only teams that don't represent a single permanent venue.
        /// </summary>
        [StringLength(200, ErrorMessage = "Home arena cannot exceed 200 characters")]
        public string? HomeArena { get; set; }

        /// <summary>
        /// The primary jersey color of the team (optional).
        /// </summary>
        [StringLength(50, ErrorMessage = "Primary jersey color cannot exceed 50 characters")]
        public string? PrimaryJerseyColor { get; set; }

        /// <summary>
        /// The secondary jersey color of the team (optional)
        /// </summary>
        [StringLength(50, ErrorMessage = "Secondary jersey color cannot exceed 50 characters")]
        public string? SecondaryJerseyColor { get; set; }

        /// <summary>
        /// The logo URL of the team (optional)
        /// </summary>
        [Url(ErrorMessage = "Please provide a valid logo URL")]
        [StringLength(500, ErrorMessage = "Logo URL cannot exceed 500 characters")]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// The category of the team (Adult, Youth, Women). Defaults to <see cref="TeamCategory.Adult"/>
        /// when not supplied — most tournament imports don't carry this metadata.
        /// </summary>
        public TeamCategory? Category { get; set; }

        /// <summary>
        /// The short name / acronym of the team (max 4 characters)
        /// </summary>
        [StringLength(4, MinimumLength = 1, ErrorMessage = "Short name must be 1 to 4 characters")]
        public string? ShortName { get; set; }
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
