using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Floorball;

/// <summary>
/// Request payload for setting the officials on a floorball match.
/// </summary>
public class FloorballMatchOfficialsRequest
{
    /// <summary>
    /// Officials to set on the match (must contain at least one).
    /// </summary>
    [Required]
    public IReadOnlyCollection<Guid>? Officials { get; set; }
}

/// <summary>
/// Request payload for adding a single official to a match (append semantics).
/// </summary>
public class AddOfficialToMatchRequest
{
    /// <summary>
    /// ID of the referee to add.
    /// </summary>
    [Required(ErrorMessage = "Referee ID is required")]
    public Guid RefereeId { get; set; }
}
