using Application.Features.Football.Teams.Commands;
using Application.Features.Football.Teams.Validators;
using Domain.Enums.Football;
using FluentValidation.TestHelper;

namespace ApplicationTestProject.Validators.Commands.Football;

public class UpdateFootballTeamPlayerCommandValidatorTests
{
    private readonly UpdateTeamPlayerCommandValidator _validator = new();

    [Fact]
    public void Validate_NullJerseyNumber_ShouldNotHaveValidationErrors()
    {
        UpdateFootballTeamPlayerCommand command = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FootballPosition.Midfielder,
            JerseyNumber: null,
            IsActive: false);

        TestValidationResult<UpdateFootballTeamPlayerCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    public void Validate_JerseyNumberInRange_ShouldNotHaveValidationErrors(int jerseyNumber)
    {
        UpdateFootballTeamPlayerCommand command = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FootballPosition.Midfielder,
            jerseyNumber,
            IsActive: false);

        TestValidationResult<UpdateFootballTeamPlayerCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Validate_JerseyNumberOutOfRange_ShouldHaveValidationError(int jerseyNumber)
    {
        UpdateFootballTeamPlayerCommand command = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FootballPosition.Midfielder,
            jerseyNumber,
            IsActive: true);

        TestValidationResult<UpdateFootballTeamPlayerCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.JerseyNumber)
            .WithErrorMessage("Jersey number must be between 1 and 99");
    }
}
