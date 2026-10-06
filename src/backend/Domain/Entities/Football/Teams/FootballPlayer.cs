using Domain.Entities.Common;
using Domain.ValueObjects.Football;

namespace Domain.Entities.Football.Teams;

/// <summary>
/// A football player profile attached to a Person.
/// </summary>
public class FootballPlayer : BaseEntity
{
    public Guid PersonId { get; private set; }
    public Person Person { get; private set; }
    public bool IsActive { get; private set; }
    public FootballPositionPreference Position { get; private set; }
    public int CareerGoals { get; private set; }
    public int CareerAssists { get; private set; }

    /// <summary>
    /// Whether this profile is the team's reusable loan goalkeeper placeholder.
    /// </summary>
    public bool IsLoanGoalkeeper { get; private set; }

    /// <summary>
    /// Whether this profile is a reusable loan-player placeholder.
    /// </summary>
    public bool IsLoanPlayer { get; private set; }

    /// <summary>
    /// 1-based sequence used in the name Lainapelaaja #N. Zero when this is not a loan player.
    /// </summary>
    public int LoanPlayerNumber { get; private set; }

    private FootballPlayer()
    {
        Person = null!;
        PersonId = Guid.Empty;
        IsActive = true;
        Position = new FootballPositionPreference(Enums.Football.FootballPosition.None);
    }

    public FootballPlayer(Guid personId, FootballPositionPreference position)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person ID cannot be empty.", nameof(personId));
        ArgumentNullException.ThrowIfNull(position);

        Person = null!;
        PersonId = personId;
        IsActive = true;
        Position = position;
        IsLoanGoalkeeper = false;
        IsLoanPlayer = false;
        LoanPlayerNumber = 0;
    }

    /// <summary>
    /// Marks this profile as the team's reusable loan goalkeeper.
    /// </summary>
    public void MarkAsLoanGoalkeeper() => IsLoanGoalkeeper = true;

    /// <summary>
    /// Marks this profile as a reusable loan player with a stable sequence number.
    /// </summary>
    public void MarkAsLoanPlayer(int loanPlayerNumber)
    {
        if (loanPlayerNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(loanPlayerNumber), "Loan player number must be at least 1.");

        IsLoanPlayer = true;
        LoanPlayerNumber = loanPlayerNumber;
    }

    public void UpdateActiveStatus(bool isActive) => IsActive = isActive;

    public void UpdatePosition(FootballPositionPreference position)
    {
        ArgumentNullException.ThrowIfNull(position);
        Position = position;
    }

    public void RecordGoal() => CareerGoals++;
    public void RecordAssist() => CareerAssists++;

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
