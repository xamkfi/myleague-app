using Domain.Enums.Common;
using Domain.ValueObjects.Football;

namespace Domain.Entities.Football.Competitions;

/// <summary>
/// A football league season.
/// </summary>
public class FootballSeason : FootballCompetition
{
    private readonly List<FootballSeasonContentBlock> _contentBlocks = new();

    public IReadOnlyCollection<FootballSeasonContentBlock> ContentBlocks => _contentBlocks.AsReadOnly();

    private FootballSeason() : base() { }

    public FootballSeason(
        string name,
        DateTime startDate,
        DateTime endDate,
        FootballMatchRules? matchRules = null,
        FootballStandingRules? standingRules = null,
        TeamCategory teamCategory = TeamCategory.Adult)
        : base(name, startDate, endDate, matchRules, standingRules, teamCategory) { }

    /// <summary>
    /// Replaces intro blocks. List order becomes <see cref="FootballSeasonContentBlock.SortOrder"/>.
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
            Guid? itemId = item.Id;
            FootballSeasonContentBlock? existing = itemId is Guid id && id != Guid.Empty
                ? _contentBlocks.FirstOrDefault(block => block.Id == id)
                : null;

            if (existing is not null)
            {
                existing.Update(item.Title, item.ContentHtml, sortOrder);
            }
            else
            {
                _contentBlocks.Add(new FootballSeasonContentBlock(Id, item.Title, item.ContentHtml, sortOrder));
            }

            sortOrder++;
        }
    }
}
