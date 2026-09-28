using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Teams.Validators;
using Domain.Enums.Floorball;
using FluentValidation.TestHelper;

namespace ApplicationTestProject.Validators.Commands.Floorball;

public class UpdateFloorballTeamPlayerCommandValidatorTests
{
    private readonly UpdateTeamPlayerCommandValidator _validator = new();

    [Fact]
    public void Validate_NullJerseyNumber_ShouldNotHaveValidationErrors()
    {
        UpdateFloorballTeamPlayerCommand command = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FloorballPosition.Forward,
            JerseyNumber: null,
            IsActive: false);

        TestValidationResult<UpdateFloorballTeamPlayerCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    public void Validate_JerseyNumberInRange_ShouldNotHaveValidationErrors(int jerseyNumber)
    {
        UpdateFloorballTeamPlayerCommand command = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FloorballPosition.Forward,
            jerseyNumber,
            IsActive: false);

        TestValidationResult<UpdateFloorballTeamPlayerCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Validate_JerseyNumberOutOfRange_ShouldHaveValidationError(int jerseyNumber)
    {
        UpdateFloorballTeamPlayerCommand command = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FloorballPosition.Forward,
            jerseyNumber,
            IsActive: true);

        TestValidationResult<UpdateFloorballTeamPlayerCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.JerseyNumber)
            .WithErrorMessage("Jersey number must be between 1 and 99");
    }
}
