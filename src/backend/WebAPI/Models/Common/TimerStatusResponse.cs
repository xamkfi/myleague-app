namespace WebAPI.Models.Common;

/// <summary>
/// Response model for timer status
/// </summary>
public class TimerStatusResponse
{
    /// <summary>
    /// Whether the timer exists
    /// </summary>
    public bool Exists { get; set; }

    /// <summary>
    /// Whether the timer is running
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// The elapsed time as a formatted string
    /// </summary>
    public string ElapsedTime { get; set; } = string.Empty;
} 