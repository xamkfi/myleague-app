using System;

namespace Application.Features.Floorball.Matches.DTOs
{
    /// <summary>
    /// DTO for a floorball save event
    /// </summary>
    public class FloorballSaveEventDto
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public Guid GoalieId { get; set; }
        public int PeriodNumber { get; set; }
        public int TimeInSeconds { get; set; }
        public bool WasInOvertime { get; set; }
        public bool WasInShootout { get; set; }
        public string? GoalieName { get; set; }
        // Optionally, shooter info, etc.
    }
}
