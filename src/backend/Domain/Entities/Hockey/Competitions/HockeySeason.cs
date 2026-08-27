using Domain.Enums.Common;
using Domain.Enums.Hockey.Competitions;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Represents a hockey league season (e.g. "2024-2025").
/// </summary>
public class HockeySeason : HockeyCompetition
{
    private readonly List<HockeySeasonContentBlock> _contentBlocks = new();

    public string? SeasonCode { get; private set; }
    public Guid? ChampionCompetitionTeamId { get; private set; }
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
            HockeySeasonContentBlock? existing = item.Id.HasValue && item.Id.Value != Guid.Empty
                ? _contentBlocks.FirstOrDefault(block => block.Id == item.Id.Value)
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
