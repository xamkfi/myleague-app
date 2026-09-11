namespace Domain.Entities.Football.Competitions;

/// <summary>
/// Ordered HTML intro block shown on a football season's public pages.
/// </summary>
public class FootballSeasonContentBlock : BaseEntity
{
    public const int TitleMaxLength = 200;
    public const int ContentHtmlMaxLength = 50000;

    public Guid SeasonId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string ContentHtml { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private FootballSeasonContentBlock()
    {
    }

    public FootballSeasonContentBlock(Guid seasonId, string title, string contentHtml, int sortOrder)
    {
        if (seasonId == Guid.Empty)
        {
            throw new ArgumentException("Season id cannot be empty.", nameof(seasonId));
        }

        SeasonId = seasonId;
        Title = ValidateTitle(title);
        ContentHtml = ValidateContentHtml(contentHtml);
        SortOrder = ValidateSortOrder(sortOrder);
    }

    public void Update(string title, string contentHtml, int sortOrder)
    {
        Title = ValidateTitle(title);
        ContentHtml = ValidateContentHtml(contentHtml);
        SortOrder = ValidateSortOrder(sortOrder);
    }

    internal static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Block title cannot be empty.", nameof(title));
        }

        string trimmed = title.Trim();
        if (trimmed.Length > TitleMaxLength)
        {
            throw new ArgumentException($"Block title cannot exceed {TitleMaxLength} characters.", nameof(title));
        }

        return trimmed;
    }

    internal static string ValidateContentHtml(string? contentHtml)
    {
        string value = contentHtml?.Trim() ?? string.Empty;
        if (value.Length > ContentHtmlMaxLength)
        {
            throw new ArgumentException($"Block content cannot exceed {ContentHtmlMaxLength} characters.", nameof(contentHtml));
        }

        return value;
    }

    internal static int ValidateSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "Sort order cannot be negative.");
        }

        return sortOrder;
    }
}
