using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Football;

/// <summary>
/// Request model for replacing the officials assigned to a match
/// </summary>
public class FootballMatchOfficialsRequest
{
    /// <summary>
    /// Referee identifiers to assign
    /// </summary>
    [Required]
    public IReadOnlyCollection<Guid>? Officials { get; set; }
}

/// <summary>
/// Request model for adding a single official to a match
/// </summary>
public class AddOfficialToMatchRequest
{
    /// <summary>
    /// Referee identifier to add
    /// </summary>
    [Required(ErrorMessage = "Referee ID is required")]
    public Guid RefereeId { get; set; }
}
