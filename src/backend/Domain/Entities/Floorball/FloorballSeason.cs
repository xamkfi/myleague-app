using Domain.Enums.Common;
using Domain.ValueObjects.Floorball;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball league season (e.g., "2023-2024")
/// </summary>
public class FloorballSeason : FloorballCompetition
{
    private readonly List<FloorballSeasonContentBlock> _contentBlocks = new();

    /// <summary>
    /// Gets the ordered HTML intro blocks for public season pages.
    /// </summary>
    public IReadOnlyCollection<FloorballSeasonContentBlock> ContentBlocks => _contentBlocks.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballSeason() : base() { }

    /// <summary>
    /// Initializes a new instance of the FloorballSeason class
    /// </summary>
    /// <param name="name">The name of the season</param>
    /// <param name="startDate">The start date of the season</param>
    /// <param name="endDate">The end date of the season</param>
    /// <param name="matchRules">Optional match rules configuration. If null, defaults are used.</param>
    /// <param name="teamCategory">Audience / age-group category. Defaults to Adult.</param>
    public FloorballSeason(
        string name,
        DateTime startDate,
        DateTime endDate,
        FloorballMatchRules? matchRules = null,
        TeamCategory teamCategory = TeamCategory.Adult)
        : base(name, startDate, endDate, matchRules, teamCategory) { }

    /// <summary>
    /// Replaces intro blocks. List order becomes <see cref="FloorballSeasonContentBlock.SortOrder"/>.
    /// Existing ids are updated; omitted ids are removed; missing ids are created.
    /// </summary>
    public void ReplaceContentBlocks(IReadOnlyList<(Guid? Id, string Title, string ContentHtml)> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        HashSet<Guid> keepIds = items
            .Where(item => item.Id.HasValue && item.Id.Value != Guid.Empty)
            .Select(item => item.Id!.Value)
            .ToHashSet();

        _contentBlocks.RemoveAll(block => !keepIds.Contains(block.Id));

        int sortOrder = 0;
        foreach ((Guid? Id, string Title, string ContentHtml) item in items)
        {
            FloorballSeasonContentBlock? existing = null;
            if (item.Id is Guid id && id != Guid.Empty)
            {
                existing = _contentBlocks.FirstOrDefault(block => block.Id == id);
            }

            if (existing is not null)
            {
                existing.Update(item.Title, item.ContentHtml, sortOrder);
            }
            else
            {
                _contentBlocks.Add(new FloorballSeasonContentBlock(Id, item.Title, item.ContentHtml, sortOrder));
            }

            sortOrder++;
        }
    }
}
