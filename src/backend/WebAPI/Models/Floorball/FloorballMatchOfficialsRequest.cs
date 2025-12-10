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

