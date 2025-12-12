namespace FloorballPlayerImporter.Models;

/// <summary>
/// Represents a team roster import from JSON
/// </summary>
public class TeamRosterImport
{
    /// <summary>
    /// The name of the team
    /// </summary>
    public string Team { get; set; } = string.Empty;

    /// <summary>
    /// List of players to import for this team
    /// </summary>
    public List<PlayerImport> Players { get; set; } = new List<PlayerImport>();
}

/// <summary>
/// Represents a player to import
/// </summary>
public class PlayerImport
{
    /// <summary>
    /// Player's jersey number
    /// </summary>
    public int JerseyNumber { get; set; }

    /// <summary>
    /// Player's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Player's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Player's position (Forward, Defender, Center, Goalie/Goalkeeper)
    /// </summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>
    /// Full name for convenience
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}

