using Domain.Entities.Common;

namespace DomainTestProject.Common;

public class SiteSettingsTests
{
    [Fact]
    public void Constructor_ValidInput_SetsProperties()
    {
        SiteSettings settings = new(Guid.NewGuid(), 20, 14, 8, 6, 4);

        settings.AccessTokenExpirationMinutes.Should().Be(20);
        settings.RefreshTokenExpirationDays.Should().Be(14);
        settings.LoginCodeExpirationMinutes.Should().Be(8);
        settings.LoginCodeMaxAttempts.Should().Be(6);
        settings.SessionExpiryWarningMinutes.Should().Be(4);
    }

    [Theory]
    [InlineData(1, 7, 10, 5, 5, "accessTokenExpirationMinutes")]
    [InlineData(181, 7, 10, 5, 5, "accessTokenExpirationMinutes")]
    [InlineData(15, 0, 10, 5, 5, "refreshTokenExpirationDays")]
    [InlineData(15, 91, 10, 5, 5, "refreshTokenExpirationDays")]
    [InlineData(15, 7, 1, 5, 5, "loginCodeExpirationMinutes")]
    [InlineData(15, 7, 61, 5, 5, "loginCodeExpirationMinutes")]
    [InlineData(15, 7, 10, 2, 5, "loginCodeMaxAttempts")]
    [InlineData(15, 7, 10, 21, 5, "loginCodeMaxAttempts")]
    [InlineData(15, 7, 10, 5, 0, "sessionExpiryWarningMinutes")]
    [InlineData(15, 7, 10, 5, 31, "sessionExpiryWarningMinutes")]
    public void Constructor_OutOfRange_Throws(
        int accessMinutes,
        int refreshDays,
        int loginMinutes,
        int maxAttempts,
        int warningMinutes,
        string paramName)
    {
        Action act = () => new SiteSettings(
            Guid.NewGuid(),
            accessMinutes,
            refreshDays,
            loginMinutes,
            maxAttempts,
            warningMinutes);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(paramName);
    }

    [Fact]
    public void Update_ValidInput_ReplacesValues()
    {
        SiteSettings settings = new(Guid.NewGuid(), 15, 7, 10, 5, 5);

        settings.Update(30, 14, 15, 8, 3);

        settings.AccessTokenExpirationMinutes.Should().Be(30);
        settings.RefreshTokenExpirationDays.Should().Be(14);
        settings.LoginCodeExpirationMinutes.Should().Be(15);
        settings.LoginCodeMaxAttempts.Should().Be(8);
        settings.SessionExpiryWarningMinutes.Should().Be(3);
        settings.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_OutOfRange_Throws()
    {
        SiteSettings settings = new(Guid.NewGuid(), 15, 7, 10, 5, 5);

        Action act = () => settings.Update(1, 7, 10, 5, 5);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("accessTokenExpirationMinutes");
    }
}
