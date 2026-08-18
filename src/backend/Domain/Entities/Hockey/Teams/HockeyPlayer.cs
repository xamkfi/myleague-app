using Domain.Entities.Common;
using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Teams;

/// <summary>
/// Hockey player profile linked to a <see cref="Person"/> via <see cref="PersonId"/>,
/// using the same cross-context pattern as <c>FloorballPlayer</c>.
/// </summary>
public class HockeyPlayer : BaseEntity
{
    public Guid PersonId { get; private set; }
    public Person Person { get; private set; } = null!;
    public string? LicenseNumber { get; private set; }
    public bool IsActive { get; private set; }
    public HockeyPosition PrimaryPosition { get; private set; }
    public HockeyShoots Shoots { get; private set; }
    public HockeyCatches? Catches { get; private set; }
    public int CareerGamesPlayed { get; private set; }
    public int CareerGoals { get; private set; }
    public int CareerAssists { get; private set; }
    public int CareerPenaltyMinutes { get; private set; }
    public int CareerFaceoffWins { get; private set; }
    public int CareerFaceoffAttempts { get; private set; }

    public decimal CareerFaceoffPercentage =>
        CareerFaceoffAttempts > 0
            ? Math.Round((decimal)CareerFaceoffWins / CareerFaceoffAttempts, 4)
            : 0m;

    private HockeyPlayer() { }

    public HockeyPlayer(
        Guid personId,
        HockeyPosition primaryPosition,
        HockeyShoots shoots = HockeyShoots.Unknown,
        HockeyCatches? catches = null,
        string? licenseNumber = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person id cannot be empty.", nameof(personId));

        PersonId = personId;
        PrimaryPosition = primaryPosition;
        Shoots = shoots;
        Catches = catches;
        LicenseNumber = licenseNumber;
        IsActive = true;
    }

    public void UpdateActiveStatus(bool isActive) => IsActive = isActive;

    public void UpdateLicenseNumber(string? licenseNumber) => LicenseNumber = licenseNumber;

    public void UpdatePosition(HockeyPosition primaryPosition, HockeyShoots shoots, HockeyCatches? catches)
    {
        PrimaryPosition = primaryPosition;
        Shoots = shoots;
        Catches = catches;
    }

    public void RecordGamePlayed() => CareerGamesPlayed++;

    public void RecordGoal() => CareerGoals++;

    public void RecordAssist() => CareerAssists++;

    public void RecordPenaltyMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentOutOfRangeException(nameof(minutes), "Penalty minutes cannot be negative.");
        CareerPenaltyMinutes += minutes;
    }

    public void RecordFaceoffWin()
    {
        CareerFaceoffAttempts++;
        CareerFaceoffWins++;
    }

    public void RecordFaceoffLoss() => CareerFaceoffAttempts++;

    public void RemoveGoal()
    {
        if (CareerGoals > 0)
            CareerGoals--;
    }

    public void RemoveAssist()
    {
        if (CareerAssists > 0)
            CareerAssists--;
    }

    public void SetPerson(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);
        Person = person;
    }
}
