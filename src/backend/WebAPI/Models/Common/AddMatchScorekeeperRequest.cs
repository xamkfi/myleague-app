using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common;

/// <summary>
/// Request body for adding a scorekeeper (toimitsija) to a match.
/// </summary>
public class AddMatchScorekeeperRequest
{
    /// <summary>
    /// ID of the person acting as scorekeeper.
    /// </summary>
    [Required(ErrorMessage = "Person ID is required")]
    public Guid PersonId { get; set; }
}
