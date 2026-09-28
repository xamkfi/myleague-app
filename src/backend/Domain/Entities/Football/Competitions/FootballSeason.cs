using Domain.Enums.Common;
using Domain.Services.Common;
using Domain.ValueObjects.Football;

namespace Domain.Entities.Football.Competitions;

/// <summary>
/// A football league season.
/// </summary>
public class FootballSeason : FootballCompetition
{
    private readonly List<FootballSeasonContentBlock> _contentBlocks = new();
    private readonly List<StandingSortCriterion> _rankingCriteria = new(StandingSortCriteria.Default);

    /// <summary>
    /// How many teams at the top of the table advance. Zero means no highlight.
    /// </summary>
    public int TeamsAdvancing { get; private set; }

    /// <summary>
    /// Ordered criteria used to rank the season table.
    /// </summary>
    public IReadOnlyList<StandingSortCriterion> RankingCriteria => _rankingCriteria;

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
    /// Sets how many teams advance and the order used to rank the table.
    /// A null criteria list keeps the default order.
    /// </summary>
    public void UpdateStandingsSettings(int teamsAdvancing, IEnumerable<StandingSortCriterion>? rankingCriteria)
    {
        if (teamsAdvancing < 0)
            throw new ArgumentOutOfRangeException(nameof(teamsAdvancing), "Teams advancing cannot be negative.");

        TeamsAdvancing = teamsAdvancing;
        List<StandingSortCriterion> resolved = StandingSortCriteria.Resolve(rankingCriteria);
        _rankingCriteria.Clear();
        _rankingCriteria.AddRange(resolved);
    }

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
