namespace Domain.ValueObjects.Hockey.Matches;

/// <summary>
/// Base class for hockey match event value objects.
/// </summary>
public abstract class HockeyMatchEventBaseValue
{
    public int PeriodNumber { get; private set; }
    public TimeSpan GameTime { get; private set; }
    public string? Description { get; private set; }

    public string FormattedGameTime
    {
        get
        {
            int totalSeconds = (int)Math.Floor(GameTime.TotalSeconds);
            if (totalSeconds < 0)
                totalSeconds = 0;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }
    }

    protected HockeyMatchEventBaseValue() { }

    protected HockeyMatchEventBaseValue(int periodNumber, TimeSpan gameTime, string? description = null)
    {
        if (periodNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period number must be positive.");
        if (gameTime < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(gameTime), "Game time cannot be negative.");

        PeriodNumber = periodNumber;
        GameTime = gameTime;
        Description = description;
    }
}
