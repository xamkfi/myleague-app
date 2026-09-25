using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.ValueObjects.Common;

namespace DomainTestProject.Common;

public class PersonDataSubjectTests
{
    [Fact]
    public void RestrictProcessing_ThenLift_ClearsTheFlag()
    {
        Person person = new Person("Aino", "Aalto");

        person.RestrictProcessing();
        person.IsProcessingRestricted.Should().BeTrue();

        person.LiftProcessingRestriction();
        person.IsProcessingRestricted.Should().BeFalse();
    }

    [Fact]
    public void ObjectToLegitimateInterest_RestrictsProcessing_WithdrawKeepsRestriction()
    {
        Person person = new Person("Aino", "Aalto");

        person.ObjectToLegitimateInterest();

        person.HasObjectedToLegitimateInterest.Should().BeTrue();
        person.IsProcessingRestricted.Should().BeTrue();

        person.WithdrawLegitimateInterestObjection();

        person.HasObjectedToLegitimateInterest.Should().BeFalse();
        person.IsProcessingRestricted.Should().BeTrue();
    }

    [Fact]
    public void Rectify_WhileRestricted_UpdatesIdentity()
    {
        Person person = new Person("Aino", "Aalto");
        person.RestrictProcessing();

        person.Rectify("Aino", "Korhonen", new DateTime(1990, 1, 2, 0, 0, 0, DateTimeKind.Utc), null, null);

        person.LastName.Should().Be("Korhonen");
        person.BirthDate.Should().Be(new DateTime(1990, 1, 2, 0, 0, 0, DateTimeKind.Utc));
        person.IsProcessingRestricted.Should().BeTrue();
    }

    [Fact]
    public void Anonymize_ClearsIdentity_AndBlocksLaterRectification()
    {
        Person person = new Person(
            "Aino",
            "Aalto",
            new DateTime(1990, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            PersonRole.User,
            new Address("Katu 1", "Mikkeli", "50100", "Finland"),
            new ContactInfo("aino@example.com", "0401234567", null));
        DateTime anonymizedAt = new DateTime(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc);

        person.Anonymize(anonymizedAt);

        person.FirstName.Should().Be(Person.AnonymizedFirstName);
        person.LastName.Should().Be(Person.AnonymizedLastName);
        person.BirthDate.Should().BeNull();
        person.Address.Should().BeNull();
        person.ContactInfo.Should().BeNull();
        person.IsProcessingRestricted.Should().BeTrue();
        person.AnonymizedAt.Should().Be(anonymizedAt);

        Action rectify = () => person.Rectify("Aino", "Aalto", null, null, null);
        rectify.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LiftProcessingRestriction_AfterAnonymize_Throws()
    {
        Person person = new Person("Aino", "Aalto");
        person.Anonymize(DateTime.UtcNow);

        Action lift = () => person.LiftProcessingRestriction();
        lift.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EraseAccount_ReplacesEmailAndClearsLoginCode()
    {
        User user = new User("aino@example.com", Guid.NewGuid(), UserRole.ClubAdmin);
        user.SetLoginCode("123456", DateTime.UtcNow.AddMinutes(10));

        user.EraseAccount("removed-abc@data-subject.invalid");

        user.Email.Should().Be("removed-abc@data-subject.invalid");
        user.IsActive.Should().BeFalse();
        user.LoginCode.Should().BeNull();
    }

    [Fact]
    public void SuspendAccess_DeactivatesAndClearsLoginCode()
    {
        User user = new User("aino@example.com", Guid.NewGuid());
        user.SetLoginCode("123456", DateTime.UtcNow.AddMinutes(10));

        user.SuspendAccess();

        user.IsActive.Should().BeFalse();
        user.LoginCode.Should().BeNull();
        user.Email.Should().Be("aino@example.com");
    }
}
