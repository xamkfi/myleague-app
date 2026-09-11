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
