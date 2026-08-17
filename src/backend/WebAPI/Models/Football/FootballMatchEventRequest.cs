using System.ComponentModel.DataAnnotations;
using Domain.Enums.Football;

namespace WebAPI.Models.Football;

public abstract class FootballMatchEventBaseRequest
{
    [Required(ErrorMessage = "Match ID is required")]
    public Guid MatchId { get; set; }

    [Required(ErrorMessage = "Team ID is required")]
    public Guid TeamId { get; set; }

    [Required(ErrorMessage = "Period number is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Period number must be 1 or greater")]
    public int PeriodNumber { get; set; }

    [Required(ErrorMessage = "Time in seconds is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Time must be non-negative")]
    public int TimeInSeconds { get; set; }

    public string? Description { get; set; }
}

public class RecordCardEventRequest : FootballMatchEventBaseRequest
{
    [Required(ErrorMessage = "Player ID is required")]
    public Guid PlayerId { get; set; }

    [Required(ErrorMessage = "Card type is required")]
    public FootballCardType CardType { get; set; }
}

public class RecordSubstitutionEventRequest : FootballMatchEventBaseRequest
{
    [Required(ErrorMessage = "Player going off is required")]
    public Guid PlayerOffId { get; set; }

    [Required(ErrorMessage = "Player coming on is required")]
    public Guid PlayerOnId { get; set; }
}

public class MatchIdRequest
{
    [Required(ErrorMessage = "Match ID is required")]
    public Guid MatchId { get; set; }
}
