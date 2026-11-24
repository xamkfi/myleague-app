using Domain.Entities.Floorball.Matches.Events;

namespace Domain.Entities.Floorball.Matches;

/// <summary>
/// A team's shots in a match, counted from its events.
/// </summary>
/// <param name="OnGoal">Goals scored, own goals excluded</param>
/// <param name="Total">Goals and shots saved by the opposing goalies</param>
public sealed record FloorballTeamShots(int OnGoal, int Total);

/// <summary>
/// Saves and conceded goals moved away from a goalie.
/// </summary>
public sealed record FloorballGoalieEventTransfer(Guid FromGoalieId, int Saves, int GoalsAgainst);

/// <summary>
/// A goal moved to another period, with the goalie it was charged to before the move.
/// </summary>
public sealed record FloorballMovedGoal(FloorballGoal Goal, Guid? PreviousGoalieId);
