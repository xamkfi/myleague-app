using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.ValueObjects.Common;

namespace DomainTestProject.Common;

public class EmailAddressTests
{
    [Theory]
    [InlineData("Tuomas@Mahl.FI", "tuomas@mahl.fi")]
    [InlineData("  ADMIN@mahl.fi  ", "admin@mahl.fi")]
    [InlineData("already.lower@mahl.fi", "already.lower@mahl.fi")]
    public void Normalize_TrimsAndLowercases(string input, string expected)
    {
        EmailAddress.Normalize(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_MissingValue_Throws(string? input)
    {
        Action act = () => EmailAddress.Normalize(input!);

        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    [Fact]
    public void NormalizeOptional_MissingValue_ReturnsNull()
    {
        EmailAddress.NormalizeOptional("  ").Should().BeNull();
    }

    [Fact]
    public void ContactInfo_StoresEmailInLowercase()
    {
        ContactInfo contact = new("Tuomas.Reijonen@Mahl.FI", "0401234567");

        contact.Email.Should().Be("tuomas.reijonen@mahl.fi");
    }

    [Fact]
    public void User_Constructor_StoresEmailInLowercase()
    {
        User user = new("ClubAdmin@Mahl.FI", Guid.NewGuid(), UserRole.ClubAdmin);

        user.Email.Should().Be("clubadmin@mahl.fi");
    }

    [Fact]
    public void User_ChangeEmail_StoresEmailInLowercase()
    {
        User user = new("old@mahl.fi", Guid.NewGuid(), UserRole.SystemAdmin);

        user.ChangeEmail("  New.Admin@Mahl.FI ");

        user.Email.Should().Be("new.admin@mahl.fi");
    }
}
