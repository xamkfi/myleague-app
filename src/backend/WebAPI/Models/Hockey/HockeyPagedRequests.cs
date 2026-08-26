using Domain.Enums.Common;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Teams;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Models.Hockey;

/// <summary>
/// Query parameters for paginated hockey teams.
/// </summary>
public record GetPagedHockeyTeamsRequest : PagedRequestBase
{
    /// <summary>Optional team name or short name search.</summary>
    public string? SearchTerm { get; init; }

    /// <summary>Optional club filter.</summary>
    public Guid? ClubId { get; init; }

    /// <summary>Optional audience category filter.</summary>
    public TeamCategory? TeamCategory { get; init; }
}

/// <summary>
/// Query parameters for paginated hockey players.
/// </summary>
public record GetPagedHockeyPlayersRequest : PagedRequestBase
{
    /// <summary>Optional license number search.</summary>
    public string? SearchTerm { get; init; }

    /// <summary>Optional active status filter.</summary>
    public bool? IsActive { get; init; }

    /// <summary>Optional primary position filter.</summary>
    public HockeyPosition? Position { get; init; }

    /// <summary>Optional club filter via roster membership.</summary>
    public Guid? ClubId { get; init; }

    /// <summary>Optional team filter via roster membership.</summary>
    public Guid? TeamId { get; init; }

    /// <summary>Optional team category filter via roster membership.</summary>
    public TeamCategory? TeamCategory { get; init; }
}

/// <summary>
/// Query parameters for paginated hockey officials.
/// </summary>
public record GetPagedHockeyOfficialsRequest : PagedRequestBase
{
    /// <summary>Optional active status filter.</summary>
    public bool? IsActive { get; init; }

    /// <summary>Optional official number search.</summary>
    public string? SearchTerm { get; init; }

    /// <summary>Optional filter for licenses expiring within the given days.</summary>
    public int? LicenseExpiringWithinDays { get; init; }
}

/// <summary>
/// Query parameters for paginated hockey matches.
/// </summary>
public record GetPagedHockeyMatchesRequest : PagedRequestBase
{
    /// <summary>Optional competition filter.</summary>
    public Guid? CompetitionId { get; init; }

    /// <summary>Optional career team filter (home or away).</summary>
    public Guid? TeamId { get; init; }

    /// <summary>Optional scheduled start lower bound.</summary>
    public DateTime? StartDate { get; init; }

    /// <summary>Optional scheduled start upper bound.</summary>
    public DateTime? EndDate { get; init; }

    /// <summary>Optional match status filter.</summary>
    public HockeyMatchStatus? Status { get; init; }

    /// <summary>Sort order for scheduled start time (<c>asc</c> or <c>desc</c>).</summary>
    public string SortOrder { get; init; } = "desc";

    /// <summary>Optional venue search.</summary>
    public string? SearchQuery { get; init; }
}
