using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// Assignment of an official to a specific hockey match with a role for that game.
/// </summary>
public class HockeyMatchOfficial : BaseEntity
{
    /// <summary>Gets the parent match identifier.</summary>
    public Guid MatchId { get; private set; }

    /// <summary>Gets the parent match aggregate.</summary>
    public HockeyMatch Match { get; private set; } = null!;

    /// <summary>Gets the official profile identifier.</summary>
    public Guid OfficialId { get; private set; }

    /// <summary>Gets the official navigation (ignored in EF when cross-context).</summary>
    public HockeyOfficial? Official { get; private set; }

    /// <summary>Gets the role this official performs in the match.</summary>
    public HockeyOfficialRole Role { get; private set; }

    /// <summary>Gets whether this official is marked as the main official for the match.</summary>
    public bool IsMainOfficial { get; private set; }

    private HockeyMatchOfficial() { }

    internal HockeyMatchOfficial(Guid matchId, Guid officialId, HockeyOfficialRole role, bool isMainOfficial)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(matchId));
        if (officialId == Guid.Empty)
            throw new ArgumentException("Official id cannot be empty.", nameof(officialId));

        MatchId = matchId;
        OfficialId = officialId;
        Role = role;
        IsMainOfficial = isMainOfficial;
    }

    internal void SetRole(HockeyOfficialRole role) => Role = role;

    internal void SetIsMainOfficial(bool isMainOfficial) => IsMainOfficial = isMainOfficial;
}
