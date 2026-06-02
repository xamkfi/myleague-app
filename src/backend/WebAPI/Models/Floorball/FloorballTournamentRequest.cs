using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Enums.Floorball;

namespace WebAPI.Models.Floorball
{
    /// <summary>
    /// One pre-defined playoff bracket slot. Sent by the tournament import flow so the
    /// schedule view can show "TBD vs TBD" placeholders before the bracket is generated.
    /// </summary>
    public class PlayoffScheduleSlotRequest
    {
        /// <summary>
        /// Bracket round (QuarterFinal, SemiFinal, ThirdPlaceMatch, Final).
        /// </summary>
        [Required]
        public FloorballPlayoffRound Round { get; set; }

        /// <summary>
        /// 0-based position within the round (QF1 = 0, QF2 = 1, …).
        /// </summary>
        [Required]
        public int Order { get; set; }

        /// <summary>
        /// Kickoff time (UTC).
        /// </summary>
        [Required]
        public DateTime ScheduledDateTime { get; set; }

        /// <summary>
        /// Optional venue / court label.
        /// </summary>
        public string? Venue { get; set; }
    }

    /// <summary>
    /// Request model for creating a floorball tournament
    /// </summary>
    public class CreateFloorballTournamentRequest
    {
        /// <summary>
        /// Name of the tournament
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Start date of the tournament
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the tournament
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Primary venue for the tournament
        /// </summary>
        public string? Venue { get; set; }

        /// <summary>
        /// HTML content describing the tournament (rendered via Quill editor)
        /// </summary>
        public string? ContentHtml { get; set; }

        /// <summary>
        /// Number of periods for group stage matches. Default: 2.
        /// </summary>
        public int GroupStageNumberOfPeriods { get; set; } = 2;

        /// <summary>
        /// Duration in minutes per period for group stage matches. Default: 15.
        /// </summary>
        public int GroupStagePeriodDurationMinutes { get; set; } = 15;

        /// <summary>
        /// Whether overtime is allowed in group stage matches. Default: true.
        /// </summary>
        public bool GroupStageAllowOvertime { get; set; } = true;

        /// <summary>
        /// Duration in minutes for overtime in group stage matches. Default: 5.
        /// </summary>
        public int GroupStageOvertimeDurationMinutes { get; set; } = 5;

        /// <summary>
        /// Whether shootout is allowed in group stage matches. Default: true.
        /// </summary>
        public bool GroupStageAllowShootout { get; set; } = true;

        /// <summary>
        /// Number of periods for playoff matches. Default: 2.
        /// </summary>
        public int PlayoffNumberOfPeriods { get; set; } = 2;

        /// <summary>
        /// Duration in minutes per period for playoff matches. Default: 15.
        /// </summary>
        public int PlayoffPeriodDurationMinutes { get; set; } = 15;

        /// <summary>
        /// Whether overtime is allowed in playoff matches. Default: true.
        /// </summary>
        public bool PlayoffAllowOvertime { get; set; } = true;

        /// <summary>
        /// Duration in minutes for overtime in playoff matches. Default: 5.
        /// </summary>
        public int PlayoffOvertimeDurationMinutes { get; set; } = 5;

        /// <summary>
        /// Whether shootout is allowed in playoff matches. Default: true.
        /// </summary>
        public bool PlayoffAllowShootout { get; set; } = true;

        /// <summary>
        /// Number of teams advancing from each group to the playoff stage. Default: 2.
        /// </summary>
        public int TeamsAdvancingPerGroup { get; set; } = 2;

        /// <summary>
        /// Whether the tournament includes a playoff stage after group stage. Default: true.
        /// </summary>
        public bool HasPlayoffStage { get; set; } = true;

        /// <summary>
        /// Whether the tournament includes a third-place match. Default: false.
        /// </summary>
        public bool HasThirdPlaceMatch { get; set; } = false;

        /// <summary>
        /// Optional pre-defined playoff bracket schedule. When provided, the StartPlayoffStage
        /// action uses these times for the generated matches instead of auto-scheduling. The
        /// schedule is also surfaced as placeholder "TBD vs TBD" rows on the public tournament
        /// page so end-users see the full match program from day one.
        /// </summary>
        public List<PlayoffScheduleSlotRequest>? PlayoffSchedule { get; set; }
    }

    /// <summary>
    /// Request model for updating a floorball tournament
    /// </summary>
    public class UpdateFloorballTournamentRequest : CreateFloorballTournamentRequest { }

    /// <summary>
    /// Request model for adding a group to a tournament
    /// </summary>
    public class AddGroupToTournamentRequest
    {
        /// <summary>
        /// Name of the group (e.g., "Group A", "Group B")
        /// </summary>
        [Required]
        [StringLength(50)]
        public string GroupName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for adding a team to a tournament group
    /// </summary>
    public class AddTeamToTournamentGroupRequest
    {
        /// <summary>
        /// ID of the team to add
        /// </summary>
        [Required]
        public Guid TeamId { get; set; }
    }
}
