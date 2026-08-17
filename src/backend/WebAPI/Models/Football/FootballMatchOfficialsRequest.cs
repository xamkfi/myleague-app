using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Football;

public class FootballMatchOfficialsRequest
{
    [Required]
    public IReadOnlyCollection<Guid>? Officials { get; set; }
}

public class AddOfficialToMatchRequest
{
    [Required(ErrorMessage = "Referee ID is required")]
    public Guid RefereeId { get; set; }
}
