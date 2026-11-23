using System.ComponentModel.DataAnnotations;
using Domain.Enums.Common;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for correcting how a match was decided
/// </summary>
public class SetMatchDecisionRequest
{
    /// <summary>
    /// The phase of play that decided the match
    /// </summary>
    [Required(ErrorMessage = "Decision is required")]
    public MatchDecision Decision { get; set; }
}
