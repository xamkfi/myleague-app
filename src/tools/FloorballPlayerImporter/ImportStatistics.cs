namespace FloorballPlayerImporter;

/// <summary>
/// Statistics for tracking import results
/// </summary>
public class ImportStatistics
{
    /// <summary>
    /// Total number of players processed from JSON
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Number of new FloorballPlayer entities created
    /// </summary>
    public int PlayersCreated { get; set; }

    /// <summary>
    /// Number of players successfully assigned to the team
    /// </summary>
    public int PlayersAssignedToTeam { get; set; }

    /// <summary>
    /// Number of players skipped (person not found)
    /// </summary>
    public int PlayersSkippedPersonNotFound { get; set; }

    /// <summary>
    /// Number of players skipped due to duplicate jersey numbers
    /// </summary>
    public int PlayersSkippedDuplicateJersey { get; set; }

    /// <summary>
    /// Number of failed operations
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// List of error messages
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// List of player names where person was not found
    /// </summary>
    public List<string> PlayersPersonNotFound { get; set; } = new();

    /// <summary>
    /// List of players skipped due to duplicate jersey numbers
    /// </summary>
    public List<(string PlayerName, int JerseyNumber)> PlayersDuplicateJersey { get; set; } = new();

    /// <summary>
    /// List of successfully assigned players
    /// </summary>
    public List<string> PlayersAssigned { get; set; } = new();

    /// <summary>
    /// List of failed operations with details
    /// </summary>
    public List<(string PlayerName, string Error)> FailedPlayers { get; set; } = new();
}

