using System.ComponentModel.DataAnnotations;
using Domain.Enums.Common;

namespace WebAPI.Models.Common;

/// <summary>
/// Request body for moving a season to another audience group.
/// </summary>
public record ChangeTeamCategoryRequest
{
    /// <summary>
    /// New audience group: Adult, Youth or Women.
    /// </summary>
    [Required(ErrorMessage = "Team category is required")]
    public TeamCategory? TeamCategory { get; init; }
}
