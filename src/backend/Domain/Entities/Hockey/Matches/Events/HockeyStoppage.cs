using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches.Events;

public class HockeyStoppage : HockeyMatchEvent
{
    public HockeyStoppageReason Reason { get; private set; }

    public Guid? ResponsibleMatchTeamId { get; private set; }
    public HockeyMatchTeam? ResponsibleMatchTeam { get; private set; }

    public Guid? ResponsibleActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? ResponsiblePlayer { get; private set; }

    public HockeyFaceoffZone? NextFaceoffZone { get; private set; }
    public HockeyFaceoffSpot? NextFaceoffSpot { get; private set; }
    public string? RuleReference { get; private set; }

    private HockeyStoppage() { }

    public HockeyStoppage(
        Guid matchId,
        int periodNumber,
        TimeSpan gameTime,
        HockeyStoppageReason reason,
        Guid? responsibleMatchTeamId = null,
        Guid? responsibleActivePlayerId = null,
        HockeyFaceoffZone? nextFaceoffZone = null,
        HockeyFaceoffSpot? nextFaceoffSpot = null,
        string? ruleReference = null,
        string? description = null)
        : base(
            matchId,
            HockeyMatchEventType.Stoppage,
            periodNumber,
            gameTime,
            matchTeamId: responsibleMatchTeamId,
            matchActivePlayerId: responsibleActivePlayerId,
            description: description)
    {
        if (responsibleMatchTeamId == Guid.Empty)
            throw new ArgumentException("Responsible match team id cannot be empty.", nameof(responsibleMatchTeamId));
        if (responsibleActivePlayerId == Guid.Empty)
            throw new ArgumentException("Responsible active player id cannot be empty.", nameof(responsibleActivePlayerId));

        Reason = reason;
        ResponsibleMatchTeamId = responsibleMatchTeamId;
        ResponsibleActivePlayerId = responsibleActivePlayerId;
        NextFaceoffZone = nextFaceoffZone;
        NextFaceoffSpot = nextFaceoffSpot;
        RuleReference = ruleReference;
    }
}
