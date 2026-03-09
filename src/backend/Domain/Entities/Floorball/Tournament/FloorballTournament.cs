using Domain.Enums.Floorball.Tournament;
using Domain.ValueObjects.Floorball;

namespace Domain.Entities.Floorball.Tournament;

/// <summary>
/// Represents a floorball tournament (e.g. Duuniturnaus).
/// A tournament has groups, matches, rules, and a rich HTML description.
/// </summary>
public class FloorballTournament : BaseEntity
{
    /// <summary>
    /// Gets the name of the tournament
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the rich HTML description of the tournament (supports images, formatting, etc.)
    /// </summary>
    public string? DescriptionHtml { get; private set; }

    /// <summary>
    /// Gets the start date of the tournament
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Gets the end date of the tournament
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Gets the location/venue where the tournament is held
    /// </summary>
    public string? Location { get; private set; }

    /// <summary>
    /// Gets the current lifecycle status of the tournament
    /// </summary>
    public FloorballTournamentStatus Status { get; private set; }

    /// <summary>
    /// Gets the match rules configuration for all matches in this tournament
    /// </summary>
    public FloorballMatchRules MatchRules { get; private set; }

    /// <summary>
    /// Gets the playoff format used after the group stage
    /// </summary>
    public FloorballTournamentPlayoffFormat PlayoffFormat { get; private set; }

    /// <summary>
    /// Gets the number of teams that advance from each group to the playoff stage
    /// </summary>
    public int GroupStageAdvancingCount { get; private set; }

    /// <summary>
    /// Gets the URLs of images associated with the tournament description.
    /// Stored as JSON in the database (same pattern as NewsArticle.ImageUrls).
    /// </summary>
    public IReadOnlyList<Uri> ImageUrls => _imageUrls.AsReadOnly();
    private readonly List<Uri> _imageUrls = new();

    /// <summary>
    /// Gets the groups in this tournament
    /// </summary>
    public IReadOnlyCollection<FloorballTournamentGroup> Groups => _groups.AsReadOnly();
    private readonly List<FloorballTournamentGroup> _groups = new();

    /// <summary>
    /// Gets the matches in this tournament
    /// </summary>
    public IReadOnlyCollection<FloorballMatch> Matches => _matches.AsReadOnly();
    private readonly List<FloorballMatch> _matches = new();

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballTournament()
    {
        Id = Guid.NewGuid();
        Name = string.Empty;
        Status = FloorballTournamentStatus.Draft;
        MatchRules = FloorballMatchRules.Default();
        PlayoffFormat = FloorballTournamentPlayoffFormat.None;
        GroupStageAdvancingCount = 1;
        _imageUrls = new List<Uri>();
        _groups = new List<FloorballTournamentGroup>();
        _matches = new List<FloorballMatch>();
    }

    /// <summary>
    /// Initializes a new instance of the FloorballTournament class
    /// </summary>
    public FloorballTournament(
        string name,
        DateTime startDate,
        DateTime endDate,
        string? location = null,
        string? descriptionHtml = null,
        FloorballMatchRules? matchRules = null,
        FloorballTournamentPlayoffFormat playoffFormat = FloorballTournamentPlayoffFormat.None,
        int groupStageAdvancingCount = 1)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tournament name cannot be empty.", nameof(name));
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.", nameof(endDate));
        if (groupStageAdvancingCount < 1)
            throw new ArgumentOutOfRangeException(nameof(groupStageAdvancingCount), "At least one team must advance from each group.");

        Id = Guid.NewGuid();
        Name = name;
        DescriptionHtml = descriptionHtml;
        StartDate = startDate;
        EndDate = endDate;
        Location = location;
        Status = FloorballTournamentStatus.Draft;
        MatchRules = matchRules ?? FloorballMatchRules.Default();
        PlayoffFormat = playoffFormat;
        GroupStageAdvancingCount = groupStageAdvancingCount;
        _imageUrls = new List<Uri>();
        _groups = new List<FloorballTournamentGroup>();
        _matches = new List<FloorballMatch>();
    }

    /// <summary>
    /// Updates the tournament's core details
    /// </summary>
    public void UpdateDetails(
        string name,
        DateTime startDate,
        DateTime endDate,
        string? location,
        string? descriptionHtml)
    {
        EnsureNotCompleted("update details");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tournament name cannot be empty.", nameof(name));
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.", nameof(endDate));

        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        Location = location;
        DescriptionHtml = descriptionHtml;
    }

    /// <summary>
    /// Updates the match rules configuration for this tournament
    /// </summary>
    public void UpdateMatchRules(FloorballMatchRules matchRules)
    {
        ArgumentNullException.ThrowIfNull(matchRules);
        EnsureNotCompleted("update match rules");

        MatchRules = matchRules;
    }

    /// <summary>
    /// Updates the playoff format and advancing team count
    /// </summary>
    public void UpdatePlayoffSettings(FloorballTournamentPlayoffFormat playoffFormat, int advancingCount)
    {
        EnsureNotCompleted("update playoff settings");

        if (advancingCount < 1)
            throw new ArgumentOutOfRangeException(nameof(advancingCount), "At least one team must advance from each group.");

        PlayoffFormat = playoffFormat;
        GroupStageAdvancingCount = advancingCount;
    }

    /// <summary>
    /// Activates the tournament, making it visible to the public
    /// </summary>
    public void Activate()
    {
        if (Status != FloorballTournamentStatus.Draft)
            throw new InvalidOperationException($"Cannot activate a tournament with status {Status}. Only Draft tournaments can be activated.");

        Status = FloorballTournamentStatus.Active;
    }

    /// <summary>
    /// Starts the tournament, transitioning from Active to InProgress
    /// </summary>
    public void Start()
    {
        if (Status != FloorballTournamentStatus.Active)
            throw new InvalidOperationException($"Cannot start a tournament with status {Status}. Only Active tournaments can be started.");

        if (_groups.Count == 0)
            throw new InvalidOperationException("Cannot start a tournament without any groups.");

        Status = FloorballTournamentStatus.InProgress;
    }

    /// <summary>
    /// Completes the tournament
    /// </summary>
    public void Complete()
    {
        if (Status != FloorballTournamentStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete a tournament with status {Status}.");

        Status = FloorballTournamentStatus.Completed;
    }

    /// <summary>
    /// Cancels the tournament
    /// </summary>
    public void Cancel()
    {
        if (Status == FloorballTournamentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed tournament.");

        Status = FloorballTournamentStatus.Cancelled;
    }

    /// <summary>
    /// Adds a group to the tournament
    /// </summary>
    public FloorballTournamentGroup AddGroup(string name, FloorballTournamentGroupPhase phase, int sortOrder = 0)
    {
        EnsureNotCompleted("add a group");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be empty.", nameof(name));

        if (_groups.Any(g => g.Name == name && g.Phase == phase))
            throw new InvalidOperationException($"A group with name '{name}' already exists in the {phase} phase.");

        FloorballTournamentGroup group = new(Id, name, phase, sortOrder);
        _groups.Add(group);
        return group;
    }

    /// <summary>
    /// Removes a group from the tournament
    /// </summary>
    public void RemoveGroup(Guid groupId)
    {
        EnsureNotCompleted("remove a group");

        FloorballTournamentGroup? group = _groups.FirstOrDefault(g => g.Id == groupId);
        if (group == null)
            throw new ArgumentException($"Group with ID {groupId} not found in this tournament.", nameof(groupId));

        bool hasMatches = _matches.Any(m => m.TournamentGroupId == groupId);
        if (hasMatches)
            throw new InvalidOperationException("Cannot remove a group that has matches. Delete the matches first.");

        _groups.Remove(group);
    }

    /// <summary>
    /// Adds a match to the tournament
    /// </summary>
    public void AddMatch(FloorballMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);
        EnsureNotCompleted("add a match");

        if (match.TournamentId != Id)
            throw new InvalidOperationException("Match does not belong to this tournament.");

        if (_matches.Contains(match))
            return;

        _matches.Add(match);
    }

    /// <summary>
    /// Adds an image URL (for tracking images used in DescriptionHtml)
    /// </summary>
    public void AddImageUrl(Uri imageUrl)
    {
        ArgumentNullException.ThrowIfNull(imageUrl);
        _imageUrls.Add(imageUrl);
    }

    /// <summary>
    /// Removes an image URL
    /// </summary>
    public void RemoveImageUrl(Uri imageUrl)
    {
        _imageUrls.Remove(imageUrl);
    }

    /// <summary>
    /// Replaces all image URLs
    /// </summary>
    public void SetImageUrls(IEnumerable<Uri> imageUrls)
    {
        _imageUrls.Clear();
        if (imageUrls != null)
            _imageUrls.AddRange(imageUrls);
    }

    // ── Private helpers ──────────────────────────────────────────────────

    private void EnsureNotCompleted(string action)
    {
        if (Status == FloorballTournamentStatus.Completed)
            throw new InvalidOperationException($"Cannot {action} for a completed tournament.");
        if (Status == FloorballTournamentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot {action} for a cancelled tournament.");
    }
}
