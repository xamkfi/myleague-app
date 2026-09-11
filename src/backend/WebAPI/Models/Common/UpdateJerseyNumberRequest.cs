using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common;

/// <summary>
/// Request body for updating a roster player's jersey number.
/// </summary>
public class UpdateJerseyNumberRequest
{
    /// <summary>
    /// The new jersey number (1-99), or null to clear the number.
    /// </summary>
    [Range(1, 99)]
    public int? JerseyNumber { get; set; }
}
