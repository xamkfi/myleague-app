using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches.Events;

public class HockeyShot : HockeyMatchEvent
{
    public Guid ShootingMatchTeamId { get; private set; }
    public HockeyMatchTeam ShootingMatchTeam { get; private set; } = null!;

    public Guid? ShooterActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? Shooter { get; private set; }

    public Guid? GoalieActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? Goalie { get; private set; }

    public HockeyShotResult ShotResult { get; private set; }
    public bool IsPowerPlayShot { get; private set; }
    public bool IsShortHandedShot { get; private set; }
    public bool IsShootoutShot { get; private set; }
    public bool CountsAsShotOnGoal { get; private set; }

    private HockeyShot() { }

    public HockeyShot(
        Guid matchId,
        Guid shootingMatchTeamId,
        int periodNumber,
        TimeSpan gameTime,
        HockeyShotResult shotResult,
        bool countsAsShotOnGoal,
        Guid? shooterActivePlayerId = null,
        Guid? goalieActivePlayerId = null,
        bool isPowerPlayShot = false,
        bool isShortHandedShot = false,
        bool isShootoutShot = false,
        string? description = null)
        : base(
            matchId,
            HockeyMatchEventType.Shot,
            periodNumber,
            gameTime,
            matchTeamId: shootingMatchTeamId,
            matchActivePlayerId: shooterActivePlayerId,
            description: description)
    {
        if (shootingMatchTeamId == Guid.Empty)
            throw new ArgumentException("Shooting match team id cannot be empty.", nameof(shootingMatchTeamId));
        if (goalieActivePlayerId == Guid.Empty)
            throw new ArgumentException("Goalie active player id cannot be empty.", nameof(goalieActivePlayerId));

        ShootingMatchTeamId = shootingMatchTeamId;
        ShooterActivePlayerId = shooterActivePlayerId;
        GoalieActivePlayerId = goalieActivePlayerId;
        ShotResult = shotResult;
        CountsAsShotOnGoal = countsAsShotOnGoal;
        IsPowerPlayShot = isPowerPlayShot;
        IsShortHandedShot = isShortHandedShot;
        IsShootoutShot = isShootoutShot;
    }

    /// <summary>
    /// Corrects shot details during live match operations.
    /// </summary>
    public void UpdateDetails(
        Guid shootingMatchTeamId,
        int periodNumber,
        TimeSpan gameTime,
        HockeyShotResult shotResult,
        bool countsAsShotOnGoal,
        Guid? shooterActivePlayerId = null,
        Guid? goalieActivePlayerId = null,
        string? description = null)
    {
        if (shootingMatchTeamId == Guid.Empty)
            throw new ArgumentException("Shooting match team id cannot be empty.", nameof(shootingMatchTeamId));
        if (goalieActivePlayerId == Guid.Empty)
            throw new ArgumentException("Goalie active player id cannot be empty.", nameof(goalieActivePlayerId));

        UpdateTiming(periodNumber, gameTime, description);
        UpdatePrimaryReferences(shootingMatchTeamId, shooterActivePlayerId);

        ShootingMatchTeamId = shootingMatchTeamId;
        ShooterActivePlayerId = shooterActivePlayerId;
        GoalieActivePlayerId = goalieActivePlayerId;
        ShotResult = shotResult;
        CountsAsShotOnGoal = countsAsShotOnGoal;
    }
}
