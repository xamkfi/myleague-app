using Domain.Enums.Common;

namespace Domain.Entities.Common;

/// <summary>
/// A rich-text content card shown on a sport landing page for a given season.
/// </summary>
public class SeasonContentBlock : BaseEntity
{
    public SportsCategory Sport { get; private set; }
    public Guid CompetitionId { get; private set; }
    public string SeasonYear { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string ContentHtml { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public string? LastModifiedBy { get; private set; }

    private SeasonContentBlock() { }

    public SeasonContentBlock(
        Guid id,
        SportsCategory sport,
        Guid competitionId,
        string seasonYear,
        string title,
        string contentHtml,
        int sortOrder,
        string? lastModifiedBy = null)
    {
        Id = id;
        Sport = ValidateSport(sport);
        CompetitionId = ValidateCompetitionId(competitionId);
        SeasonYear = ValidateSeasonYear(seasonYear);
        Title = ValidateTitle(title);
        ContentHtml = contentHtml ?? string.Empty;
        SortOrder = ValidateSortOrder(sortOrder);
        LastModifiedBy = lastModifiedBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(
        string title,
        string contentHtml,
        int sortOrder,
        string? lastModifiedBy = null)
    {
        Title = ValidateTitle(title);
        ContentHtml = contentHtml ?? string.Empty;
        SortOrder = ValidateSortOrder(sortOrder);
        LastModifiedBy = lastModifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSortOrder(int sortOrder, string? lastModifiedBy = null)
    {
        SortOrder = ValidateSortOrder(sortOrder);
        LastModifiedBy = lastModifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    private static SportsCategory ValidateSport(SportsCategory sport)
    {
        if (sport is not SportsCategory.Floorball and not SportsCategory.Football and not SportsCategory.Icehockey)
        {
            throw new ArgumentException("Sport must be Floorball, Football, or Icehockey", nameof(sport));
        }

        return sport;
    }

    private static Guid ValidateCompetitionId(Guid competitionId)
    {
        if (competitionId == Guid.Empty)
        {
            throw new ArgumentException("Competition id cannot be empty", nameof(competitionId));
        }

        return competitionId;
    }

    private static string ValidateSeasonYear(string seasonYear)
    {
        if (string.IsNullOrWhiteSpace(seasonYear))
        {
            throw new ArgumentException("Season year cannot be empty", nameof(seasonYear));
        }

        string trimmed = seasonYear.Trim();
        if (trimmed.Length > 32)
        {
            throw new ArgumentException("Season year cannot exceed 32 characters", nameof(seasonYear));
        }

        return trimmed;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Block title cannot be empty", nameof(title));
        }

        if (title.Length > 200)
        {
            throw new ArgumentException("Block title cannot exceed 200 characters", nameof(title));
        }

        return title.Trim();
    }

    private static int ValidateSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "Sort order cannot be negative");
        }

        return sortOrder;
    }
}
