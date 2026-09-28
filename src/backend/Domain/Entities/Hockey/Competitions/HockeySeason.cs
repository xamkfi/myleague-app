using Domain.Enums.Common;
using Domain.Enums.Hockey.Competitions;
using Domain.Services.Common;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Represents a hockey league season (e.g. "2024-2025").
/// </summary>
public class HockeySeason : HockeyCompetition
{
    private readonly List<HockeySeasonContentBlock> _contentBlocks = new();
    private readonly List<StandingSortCriterion> _rankingCriteria = new(StandingSortCriteria.Default);

    public string? SeasonCode { get; private set; }
    public Guid? ChampionCompetitionTeamId { get; private set; }

    /// <summary>
    /// How many teams at the top of the table advance. Zero means no highlight.
    /// </summary>
    public int TeamsAdvancing { get; private set; }

    /// <summary>
    /// Ordered criteria used to rank the season table.
    /// </summary>
    public IReadOnlyList<StandingSortCriterion> RankingCriteria => _rankingCriteria;
    public IReadOnlyCollection<HockeySeasonContentBlock> ContentBlocks => _contentBlocks.AsReadOnly();

    private HockeySeason() : base() { }

    public HockeySeason(
        string name,
        DateTime startDate,
        DateTime endDate,
        string? seasonCode = null,
        HockeyCompetitionRules? competitionRules = null,
        TeamCategory teamCategory = TeamCategory.Adult)
        : base(HockeyCompetitionType.Season, name, startDate, endDate, competitionRules, teamCategory)
    {
        SeasonCode = seasonCode;
    }

    public void UpdateSeasonCode(string? seasonCode) => SeasonCode = seasonCode;

    /// <summary>
    /// Sets how many teams advance and the order used to rank the table.
    /// A null criteria list keeps the current order. Point rules are left unchanged.
    /// </summary>
    public void UpdateStandingsSettings(int teamsAdvancing, IEnumerable<StandingSortCriterion>? rankingCriteria)
    {
        if (teamsAdvancing < 0)
            throw new ArgumentOutOfRangeException(nameof(teamsAdvancing), "Teams advancing cannot be negative.");

        TeamsAdvancing = teamsAdvancing;
        if (rankingCriteria is null)
            return;

        List<StandingSortCriterion> resolved = StandingSortCriteria.Resolve(rankingCriteria);
        _rankingCriteria.Clear();
        _rankingCriteria.AddRange(resolved);
    }

    public void SetChampion(Guid championCompetitionTeamId)
    {
        if (championCompetitionTeamId == Guid.Empty)
            throw new ArgumentException("Champion competition team id cannot be empty.", nameof(championCompetitionTeamId));
        if (Status != HockeyCompetitionStatus.Completed)
            throw new InvalidOperationException("Champion can only be set for a completed season.");

        ChampionCompetitionTeamId = championCompetitionTeamId;
    }

    /// <summary>
    /// Replaces intro blocks. List order becomes <see cref="HockeySeasonContentBlock.SortOrder"/>.
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
            HockeySeasonContentBlock? existing = item.Id is Guid itemId && itemId != Guid.Empty
                ? _contentBlocks.FirstOrDefault(block => block.Id == itemId)
                : null;

            if (existing is not null)
            {
                existing.Update(item.Title, item.ContentHtml, sortOrder);
            }
            else
            {
                _contentBlocks.Add(new HockeySeasonContentBlock(Id, item.Title, item.ContentHtml, sortOrder));
            }

            sortOrder++;
        }
    }
}
