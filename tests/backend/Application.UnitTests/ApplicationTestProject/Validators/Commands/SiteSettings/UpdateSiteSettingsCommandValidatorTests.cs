using Application.Features.Common.SiteSettings.Commands;
using Application.Features.Common.SiteSettings.Validators;

namespace ApplicationTestProject.Validators.Commands.SiteSettings;

public class UpdateSiteSettingsCommandValidatorTests
{
    private readonly UpdateSiteSettingsCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        UpdateSiteSettingsCommand command = new(15, 7, 10, 5, 5);

        _validator.Validate(command).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, 7, 10, 5, 5)]
    [InlineData(181, 7, 10, 5, 5)]
    [InlineData(15, 0, 10, 5, 5)]
    [InlineData(15, 91, 10, 5, 5)]
    [InlineData(15, 7, 1, 5, 5)]
    [InlineData(15, 7, 61, 5, 5)]
    [InlineData(15, 7, 10, 2, 5)]
    [InlineData(15, 7, 10, 21, 5)]
    [InlineData(15, 7, 10, 5, 0)]
    [InlineData(15, 7, 10, 5, 31)]
    public void Validate_OutOfRange_IsInvalid(
        int accessMinutes,
        int refreshDays,
        int loginMinutes,
        int maxAttempts,
        int warningMinutes)
    {
        UpdateSiteSettingsCommand command = new(
            accessMinutes,
            refreshDays,
            loginMinutes,
            maxAttempts,
            warningMinutes);

        _validator.Validate(command).IsValid.Should().BeFalse();
    }
}
