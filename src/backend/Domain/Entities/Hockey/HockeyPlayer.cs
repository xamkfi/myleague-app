using Domain.Enums.Hockey;
using Domain.Entities;
using Domain.Entities.Common;

namespace Domain.Entities.Hockey;

/// <summary>
/// Represents a Hockey player in the system
/// </summary>
public class HockeyPlayer : Person
{
    /// <summary>
    /// Gets whether the Hockey player is currently active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the player's preferred Hockey position
    /// </summary>
    public HockeyPosition PreferredPosition { get; private set; }

    /// <summary>
    /// Gets the player's total career goals in Hockey
    /// </summary>
    public int CareerGoals { get; private set; }

    /// <summary>
    /// Gets the player's total career assists in Hockey
    /// </summary>
    public int CareerAssists { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private HockeyPlayer() : base()
    {
        IsActive = true;
        CareerGoals = 0;
        CareerAssists = 0;
    }

    /// <summary>
    /// Initializes a new instance of the HockeyPlayer class
    /// </summary>
    /// <param name="firstName">The player's first name</param>
    /// <param name="lastName">The player's last name</param>
    /// <param name="birthDate">The player's birth date (optional)</param>
    /// <param name="preferredPosition">The player's preferred position</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public HockeyPlayer(
        string firstName,
        string lastName,
        DateTime? birthDate = null,
        HockeyPosition preferredPosition = HockeyPosition.Forward)
        : base(firstName, lastName, birthDate)
    {
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
        if (birthDate.HasValue && birthDate.Value > DateTime.UtcNow)
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));
        IsActive = true;
        PreferredPosition = preferredPosition;
        CareerGoals = 0;
        CareerAssists = 0;
    }

    /// <summary>
    /// Updates the player's active status
    /// </summary>
    /// <param name="isActive">The new active status</param>
    public void UpdateActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }

    /// <summary>
    /// Updates the player's preferred position
    /// </summary>
    /// <param name="position">The new preferred position</param>
    public void UpdatePreferredPosition(HockeyPosition position)
    {
        PreferredPosition = position;
    }

    /// <summary>
    /// Records a goal for the player
    /// </summary>
    public void RecordGoal()
    {
        CareerGoals++;
    }

    /// <summary>
    /// Records an assist for the player
    /// </summary>
    public void RecordAssist()
    {
        CareerAssists++;
    }
}
